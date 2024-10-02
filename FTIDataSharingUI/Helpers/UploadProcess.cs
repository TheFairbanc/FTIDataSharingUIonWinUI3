using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Serilog;

namespace FTIDataSharingUI.Helpers;
public class UploadProcessz
{
    private readonly ILogger _logger;

    private static string strStatusCode = "-1";

    private static string strResponseBody = "";

    private static string strZipFile = "";

    private static string strlogFileName = "";

    private static string strSandboxBoolean = "";

    private static string strSecureHTTP = "Y";

    private static string strSalesFileName = "";

    private static string strPayFileName = "";

    private static string strOutletFileName = "";

    private static string strDistID = "";

    private static string strDistName = "";

    private static string strDsDataSourceDir = "";

    private static string strDsExpDir = "";

    private static string strDsUploadDir = "";

    private static string strDsWorkingDir = "";

    private static string strSearchSubFolder = "N";

    private static string strDsPeriod = DateTime.Now.AddMonths(-1).ToString("yyyyMM");

    private static void CheckandRefreshFolder(string location)
    {
        try
        {
            if (Directory.Exists(location))
            {
                DeleteAllFilesAndSubdirectories(location);
            }
            Directory.CreateDirectory(location);
        }
        catch (Exception)
        {
            throw;
        }
    }

    private static bool IsDirectoryEmpty(string strPath)
    {
        return Directory.GetFiles(strPath).Length == 0;
    }

    public static void WriteLog(string logMessage, string strFileName)
    {
        using (StreamWriter streamWriter = File.AppendText(strFileName))
        {
            streamWriter.WriteLine($"Log Entry : {DateTime.Now:G} - :{logMessage}");
        }
    }

    private static string SendReq(string strFileDataInfo, string strSandboxBool, string strSecureHTTP)
    {
        try
        {
            string apiUrl = strSandboxBool == "Y"
                ? (strSecureHTTP == "Y" ? "https://sandbox.fairbanc.app/api/documents" : "http://sandbox.fairbanc.app/api/documents")
                : (strSecureHTTP == "Y" ? "https://dashboard.fairbanc.app/api/documents" : "http://dashboard.fairbanc.app/api/documents");

            using (var httpClient = new HttpClient())
            {
                MultipartFormDataContent multipartFormDataContent = new MultipartFormDataContent();
                multipartFormDataContent.Add(new StringContent(strSandboxBool == "Y" ? "KQtbMk32csiJvm8XDAx2KnRAdbtP3YVAnJpF8R5cb2bcBr8boT3dTvGc23c6fqk2NknbxpdarsdF3M4V" : "2S0VtpYzETxDrL6WClmxXXnOcCkNbR5nUCCLak6EHmbPbSSsJiTFTPNZrXKk2S0VtpYzETxDrL6WClmx"), "api_token");
                multipartFormDataContent.Add(new ByteArrayContent(File.ReadAllBytes(strFileDataInfo)), "file", Path.GetFileName(strFileDataInfo));
                HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, apiUrl);
                httpRequestMessage.Content = multipartFormDataContent;
                HttpResponseMessage httpResponseMessage = httpClient.Send(httpRequestMessage);
                Thread.Sleep(5000);
                httpResponseMessage.EnsureSuccessStatusCode();
                strResponseBody = httpResponseMessage.ToString();
                string[] array = strResponseBody.Split(':', ',');
                return array[1].Trim();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return "-1";
        }
    }

    public async Task ExecuteAsync()
    {
        try
        {
            //await Task.CompletedTask;

            var intNoOfDays = DateTime.DaysInMonth(DateTime.Now.AddMonths(-1).Year, DateTime.Now.AddMonths(-1).Month);

            strlogFileName = "DEBUG-" + strDistID + "-" + strDistName + "-" + strDsPeriod + ".log";

            CheckandRefreshFolder(strDsExpDir);
            CheckandRefreshFolder(strDsUploadDir);
            strlogFileName = strDsWorkingDir + Path.DirectorySeparatorChar + strlogFileName;
            WriteLog("Starting proces of Excel file sales, payment and outlet.", strlogFileName);
            WriteLog("Uploaded via FTI Submission App - Window Service.", strlogFileName);
            WriteLog($"Using Working folder -> {strDsWorkingDir} , Zip folder -> {strDsExpDir} , Upload Folder -> {strDsUploadDir}", strlogFileName);

            _logger.Information(">>>> [OUTPUT] Memulai applikasi...\n");

            if (strSalesFileName != "") WriteLog($"File Penjualan yang di proses adalah: {strSalesFileName.Trim()}", strlogFileName);
            if (strPayFileName != "") WriteLog($"File Pembayaran yang di proses adalah: {strPayFileName.Trim()}", strlogFileName);
            if (strOutletFileName != "") WriteLog($"File Outlet yang di proses adalah: {strOutletFileName.Trim()}", strlogFileName);

            if (strSalesFileName.Trim() != "" )
            {
                var strFileDataName = strSalesFileName.ToLower().EndsWith("xls") ? $"ds-{strDistID}-{strDistName}-{strDsPeriod}_SALES.xls" : $"ds-{strDistID}-{strDistName}-{strDsPeriod}_SALES.xlsx";
                if (strSalesFileName.Trim() != "")
                {
                    try
                    {
                        File.Copy(strSalesFileName, strDsExpDir + Path.DirectorySeparatorChar + strFileDataName, true);
                        File.Delete(strSalesFileName);
                    }
                    catch (Exception ex)
                    {
                        WriteLog("WARNING: Error occurred: " + ex.Message, strlogFileName);
                    }
                }
            }
            if (strPayFileName.Trim() != "" )
            {
                var strFileDataName = strPayFileName.ToLower().EndsWith("xls") ? $"ds-{strDistID}-{strDistName}-{strDsPeriod}_PAYMENT.xls" : $"ds-{strDistID}-{strDistName}-{strDsPeriod}_PAYMENT.xlsx";
                if (strPayFileName.Trim() != "")
                {
                    try
                    {
                        File.Copy(strPayFileName, strDsExpDir + Path.DirectorySeparatorChar + strFileDataName, true);
                        File.Delete(strPayFileName);
                    }
                    catch (Exception ex2)
                    {
                        WriteLog("WARNING: Error occurred: " + ex2.Message, strlogFileName);
                    }
                }
            }

            if ( strOutletFileName.Trim() != "")
            {
                var strFileDataName = strOutletFileName.ToLower().EndsWith("xls") ? $"ds-{strDistID}-{strDistName}-{strDsPeriod}_OUTLET.xls" : $"ds-{strDistID}-{strDistName}-{strDsPeriod}_OUTLET.xlsx";
                if (strOutletFileName.Trim() != "")
                {
                    try
                    {
                        File.Copy(strOutletFileName, strDsExpDir + Path.DirectorySeparatorChar + strFileDataName, true);
                        File.Delete(strOutletFileName);
                    }
                    catch (Exception ex3)
                    {
                        WriteLog("WARNING: Error occurred: " + ex3.Message, strlogFileName);
                    }
                }
            }

            if (!IsDirectoryEmpty(strDsExpDir))
            {
                WriteLog("Copy process for Excel files (sales, payment,outlet) done, Start archive process.", strlogFileName);
                strZipFile = $"{strDistID}-{strDistName}_{strDsPeriod}.zip";
                //DeleteAllFilesAndSubdirectories(strDsUploadDir);
                //DeleteAllFilesAndSubdirectories(strDsExpDir + strDsPeriod);

                ZipFile.CreateFromDirectory(strDsExpDir , strDsUploadDir + Path.DirectorySeparatorChar + strZipFile);
                WriteLog("Archive process Excel file sales, payment and outlet done", strlogFileName);
                strStatusCode = SendReq(strDsUploadDir + Path.DirectorySeparatorChar + strZipFile, strSandboxBoolean, strSecureHTTP);
                WriteLog("Upload process Excel file sales, payment and outlet done", strlogFileName);
                if (strStatusCode == "200")
                {
                    WriteLog("Data Sharing - SELESAI", strlogFileName);
                }
                else
                {
                    WriteLog($"WARNING:Gagal upload, Data Sharing cUrl STATUS CODE :{strStatusCode}", strlogFileName);
                }
            }
            else
            {
                WriteLog("WARNING: No uploaded file(s) found - Neither Sales and payment Excel Files Processed", strlogFileName);
            }
            SendReq(strlogFileName, strSandboxBoolean, strSecureHTTP);
            //FileEnumeratorHelper.Finished(strDsDataSourceDir, strDsUploadDir);
            _logger.Information(">>>> [OUTPUT] Excel Data Sharing process will be completed soon!");
            _logger.Information(">>>> [OUTPUT] Please wait, data is being uploaded.\n");
        }
        catch (Exception ex)
        {
            WriteLog($"WARNING: Error occurred: {ex.Message}", strlogFileName);
            _logger.Error(ex, "Error occurred in Main process");
        }
    }

    private static void DeleteAllFilesAndSubdirectories(string folderPath)
    {
        DirectoryInfo directory = new DirectoryInfo(folderPath);

        if (directory.Exists)
        {
            foreach (FileInfo file in directory.GetFiles())
            {
                file.Delete();
            }

            foreach (DirectoryInfo subDirectory in directory.GetDirectories())
            {
                subDirectory.Delete(true); // true to delete subdirectories and files
            }
            directory.Delete();
        }
    }

    public UploadProcessz(string _strSandboxBoolean, string _strSecureHTTP, string _strSalesPattern, string _strPayPattern, 
        string _strOutletPattern, string _strDataFolder, string _strDistID, string _strDistName, string _strWorkingFolder, 
        ILogger logger)
    {
        try
        {
#if DEBUG
            strSandboxBoolean = "Y";
            strDistID = "0";
            strDistName = "Testing-Only";
#else
            strSandboxBoolean = "N"";
            strDistID = _strDistID;
            strDistName = _strDistName;
#endif
            strSecureHTTP = _strSecureHTTP;

            strSalesFileName = _strSalesPattern;
            strPayFileName = _strPayPattern;
            strOutletFileName = _strOutletPattern;

            strDsDataSourceDir = _strDataFolder;

            strDsWorkingDir = _strWorkingFolder;

            strDsPeriod = DateTime.Now.AddMonths(-1).ToString("yyyyMM");

            strDsExpDir = Path.Combine(_strWorkingFolder, "FTI-sharing" + strDsPeriod);
            strDsUploadDir = Path.Combine(_strWorkingFolder, "FTI-upload");
            strSearchSubFolder = "N";
            _logger = logger;
        }
        catch (Exception ex)
        {
            _logger.Error("Unable to setup upload class configuration.", ex);
        }
    }

    public void UpdateProperties(string sales, string repayment, string outlet, string dataFolder, string dtid, string distName, string _strWorkingFolder)
    {
        strSalesFileName = sales;
        strPayFileName = repayment;
        strOutletFileName = outlet;
        strDsDataSourceDir = dataFolder;
#if DEBUG
        strDistID = "0";
        strDistName = "Testing-Only";
#else
        strDistID = dtid;
        strDistName = distName;
#endif

        strDsExpDir = Path.Combine(_strWorkingFolder, "FTI-sharing");

    }
}

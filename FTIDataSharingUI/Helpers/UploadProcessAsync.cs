using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace FTIDataSharingUI.Helpers;

public class UploadProcessAsync
{
    private readonly ILogger _logger;

    private string _statusCode = "-1";
    private string _responseBody = "";
    private string _zipFile = "";
    private string _logFileName = "";
    private string _sandboxBoolean = "";
    private string _secureHTTP = "Y";
    private string _salesFileName = "";
    private string _payFileName = "";
    private string _outletFileName = "";
    private string _distID = "";
    private string _distName = "";
    private string _dataSourceDir = "";
    private string _expDir = "";
    private string _uploadDir = "";
    private string _workingDir = "";
    private string _searchSubFolder = "N";
    private string _period = DateTime.Now.AddMonths(-1).ToString("yyyyMM");

    public UploadProcessAsync(string sandboxBoolean, string secureHTTP, string salesPattern, string payPattern,
                        string outletPattern, string dataFolder, string distID, string distName, string workingFolder,
                        ILogger logger)
    {
        try
        {
#if DEBUG
            _sandboxBoolean = "Y";
            _distID = "0";
            _distName = "Testing-Only";
#else
            _sandboxBoolean = sandboxBoolean;
            _distID = distID;
            _distName = distName;
#endif
            _secureHTTP = secureHTTP;

            _salesFileName = salesPattern;
            _payFileName = payPattern;
            _outletFileName = outletPattern;

            _dataSourceDir = dataFolder;
            _workingDir = workingFolder;

            _period = DateTime.Now.AddMonths(-1).ToString("yyyyMM");

            _expDir = Path.Combine(_workingDir, $"FTI-sharing{_period}");
            _uploadDir = Path.Combine(_workingDir, "FTI-upload");
            _searchSubFolder = "N";
            _logger = logger;
        }
        catch (Exception ex)
        {
            _logger.Error("Unable to setup upload class configuration.", ex);
        }
    }

    public async Task ExecuteAsync()
    {
        try
        {
            _logFileName = $"DEBUG-{_distID}-{_distName}-{_period}.log";
            _logFileName = Path.Combine(_workingDir, _logFileName);

            await CheckAndRefreshFolderAsync(_expDir);
            await CheckAndRefreshFolderAsync(_uploadDir);

            await WriteLogAsync("Starting process of Excel file sales, payment, and outlet.", _logFileName);
            await WriteLogAsync("Uploaded via FTI Submission App - Window Service.", _logFileName);
            await WriteLogAsync($"Using Working folder -> {_workingDir} , Zip folder -> {_expDir} , Upload Folder -> {_uploadDir}", _logFileName);

            _logger.Information(">>>> [OUTPUT] Starting application...\n");

            if (!string.IsNullOrWhiteSpace(_salesFileName))
                await WriteLogAsync($"Sales file to process: {_salesFileName.Trim()}", _logFileName);
            if (!string.IsNullOrWhiteSpace(_payFileName))
                await WriteLogAsync($"Payment file to process: {_payFileName.Trim()}", _logFileName);
            if (!string.IsNullOrWhiteSpace(_outletFileName))
                await WriteLogAsync($"Outlet file to process: {_outletFileName.Trim()}", _logFileName);

            if (!string.IsNullOrWhiteSpace(_salesFileName))
            {
                string salesFileDataName = _salesFileName.ToLower().EndsWith("xls")
                    ? $"ds-{_distID}-{_distName}-{_period}_SALES.xls"
                    : $"ds-{_distID}-{_distName}-{_period}_SALES.xlsx";

                await CopyAndDeleteFileAsync(_salesFileName, Path.Combine(_expDir, salesFileDataName), _logFileName);
            }

            if (!string.IsNullOrWhiteSpace(_payFileName))
            {
                string payFileDataName = _payFileName.ToLower().EndsWith("xls")
                    ? $"ds-{_distID}-{_distName}-{_period}_PAYMENT.xls"
                    : $"ds-{_distID}-{_distName}-{_period}_PAYMENT.xlsx";

                await CopyAndDeleteFileAsync(_payFileName, Path.Combine(_expDir, payFileDataName), _logFileName);
            }

            if (!string.IsNullOrWhiteSpace(_outletFileName))
            {
                string outletFileDataName = _outletFileName.ToLower().EndsWith("xls")
                    ? $"ds-{_distID}-{_distName}-{_period}_OUTLET.xls"
                    : $"ds-{_distID}-{_distName}-{_period}_OUTLET.xlsx";

                await CopyAndDeleteFileAsync(_outletFileName, Path.Combine(_expDir, outletFileDataName), _logFileName);
            }

            if (!await IsDirectoryEmptyAsync(_expDir))
            {
                await WriteLogAsync("Copy process for Excel files (sales, payment, outlet) done, starting archive process.", _logFileName);
                _zipFile = $"{_distID}-{_distName}_{_period}.zip";

                ZipFile.CreateFromDirectory(_expDir, Path.Combine(_uploadDir, _zipFile));
                await WriteLogAsync("Archive process for Excel files sales, payment, and outlet done.", _logFileName);

                _statusCode = await SendRequestAsync(Path.Combine(_uploadDir, _zipFile), _sandboxBoolean, _secureHTTP);
                await WriteLogAsync("Upload process for Excel files sales, payment, and outlet done.", _logFileName);

                if (_statusCode == "200")
                {
                    await WriteLogAsync("Data Sharing - COMPLETED", _logFileName);
                }
                else
                {
                    await WriteLogAsync($"WARNING: Failed to upload, Data Sharing cURL STATUS CODE: {_statusCode}", _logFileName);
                }
            }
            else
            {
                await WriteLogAsync("WARNING: No uploaded file(s) found - Neither Sales nor Payment Excel Files Processed.", _logFileName);
            }

            await SendRequestAsync(_logFileName, _sandboxBoolean, _secureHTTP);
            _logger.Information(">>>> [OUTPUT] Excel Data Sharing process will be completed soon!");
            _logger.Information(">>>> [OUTPUT] Please wait, data is being uploaded.\n");
        }
        catch (Exception ex)
        {
            await WriteLogAsync($"WARNING: Error occurred: {ex.Message}", _logFileName);
            _logger.Error(ex, "Error occurred in Main process");
        }
    }

    public UploadProcessAsync(string sales, string repayment, string outlet, string dataFolder, string dtid, string distName, string workingFolder)
    {
        _salesFileName = sales;
        _payFileName = repayment;
        _outletFileName = outlet;
        _dataSourceDir = dataFolder;

#if DEBUG
        _distID = "0";
        _distName = "Testing-Only";
#else
        _distID = dtid;
        _distName = distName;
#endif

        _expDir = Path.Combine(workingFolder, "FTI-sharing");
    }

    private async Task CheckAndRefreshFolderAsync(string location)
    {
        try
        {
            if (Directory.Exists(location))
            {
                await DeleteAllFilesAndSubdirectoriesAsync(location);
            }
            Directory.CreateDirectory(location);
        }
        catch (Exception)
        {
            throw;
        }
    }

    private async Task<bool> IsDirectoryEmptyAsync(string path)
    {
        return await Task.Run(() => Directory.GetFiles(path).Length == 0);
    }

    private async Task WriteLogAsync(string logMessage, string fileName)
    {
        try
        {
            using (StreamWriter streamWriter = new StreamWriter(fileName, append: true))
            {
                await streamWriter.WriteLineAsync($"Log Entry : {DateTime.Now:G} - :{logMessage}");
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to write log.");
        }
    }

    private async Task<string> SendRequestAsync(string fileDataInfo, string sandboxBool, string secureHTTP)
    {
        try
        {
            string apiUrl = sandboxBool == "Y"
                ? (secureHTTP == "Y" ? "https://sandbox.fairbanc.app/api/documents" : "http://sandbox.fairbanc.app/api/documents")
                : (secureHTTP == "Y" ? "https://dashboard.fairbanc.app/api/documents" : "http://dashboard.fairbanc.app/api/documents");

            using (var httpClient = new HttpClient())
            {
                using var multipartFormDataContent = new MultipartFormDataContent();
                string apiToken = sandboxBool == "Y"
                    ? "KQtbMk32csiJvm8XDAx2KnRAdbtP3YVAnJpF8R5cb2bcBr8boT3dTvGc23c6fqk2NknbxpdarsdF3M4V"
                    : "2S0VtpYzETxDrL6WClmxXXnOcCkNbR5nUCCLak6EHmbPbSSsJiTFTPNZrXKk2S0VtpYzETxDrL6WClmx";
                multipartFormDataContent.Add(new StringContent(apiToken), "api_token");
                byte[] fileBytes = await File.ReadAllBytesAsync(fileDataInfo);
                multipartFormDataContent.Add(new ByteArrayContent(fileBytes), "file", Path.GetFileName(fileDataInfo));

                using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, apiUrl)
                {
                    Content = multipartFormDataContent
                };

                HttpResponseMessage httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);
                await Task.Delay(5000);
                httpResponseMessage.EnsureSuccessStatusCode();
                _responseBody = await httpResponseMessage.Content.ReadAsStringAsync();
                string[] array = _responseBody.Split(':', ',');
                return array.Length > 1 ? array[1].Trim() : "-1";
            }
        }
        catch (Exception ex)
        {
            await WriteLogAsync(ex.Message, _logFileName);
            return "-1";
        }
    }

    private async Task DeleteAllFilesAndSubdirectoriesAsync(string folderPath)
    {
        await Task.Run(() =>
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
        });
    }

    private async Task CopyAndDeleteFileAsync(string sourcePath, string destinationPath, string logFile)
    {
        if (string.IsNullOrWhiteSpace(sourcePath))
            return;

        try
        {
            await Task.Run(() => File.Copy(sourcePath, destinationPath, overwrite: true));
            await Task.Run(() => File.Delete(sourcePath));
        }
        catch (Exception ex)
        {
            await WriteLogAsync($"WARNING: Error occurred: {ex.Message}", logFile);
        }
    }
}


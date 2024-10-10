using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using Microsoft.UI.Xaml.Controls;

namespace DataSubmission.Helpers;
internal class FileEnumeratorHelper
{

    // Use a private setter to allow initialization but prevent external modification
    private static ILogger _logger;

    public enum Ft
    {
        Sales,
        Payment,
        Outlet
    }


    private static string GetFiles(string strPattern, string dirPath)
    {
        var searchPattern = $"*{strPattern}*.xls*";
        var file = new DirectoryInfo(dirPath)
            .GetFiles(searchPattern, SearchOption.TopDirectoryOnly)
            .OrderByDescending(f => f.LastWriteTime)
            .FirstOrDefault();

        return file != null
            ? file.FullName
            : $">>>> [OUTPUT] No excel file found with name '*{strPattern}*'";
    }

    private static FileInfo? GetListFilesInfo(string strPattern, string dirPath)
    {
        var searchPattern = $"*{strPattern}*.xls*";
        return new DirectoryInfo(dirPath)
            .GetFiles(searchPattern, SearchOption.TopDirectoryOnly)
            .OrderByDescending(f => f.LastWriteTime)
            .FirstOrDefault();
    }

    private static List<string>? GetLatestFileInfo(List<FileInfo> files)
    {
        files.RemoveAll(item => item == null);
        if (!files.Any()) return null;

        var latestFile = files.OrderByDescending(f => f.LastWriteTime).First();
        return new List<string> { latestFile.FullName, latestFile.Name };
    }



    public static void Finished(string sourceDir, string destDir)
    {
        _logger.Information(">>>> [OUTPUT] Excel Data Sharing process will be completed soon!");
        _logger.Information(">>>> [OUTPUT] Please wait, data is being uploaded.\n");
    }

    public static string GetLatestFileName(List<string> strFilePattern, string strPath, Ft mode, string strSearchSubFolder, ILogger logger)
    {
        _logger = logger;
        List<string>? list2 = null;
        if (mode == Ft.Sales)
        {
            List<FileInfo> list = new List<FileInfo>();
            foreach (string item in strFilePattern)
            {
                _logger.Information(">>>> [OUTPUT] Mencari file Penjualan yang mempunyai nama '*" + item.Trim() + "*'...");
                _logger.Information(GetFiles(item, strPath));
                if (GetListFilesInfo(item, strPath) != null)
                {
                    list.Add(GetListFilesInfo(item, strPath));
                }
                if (!(strSearchSubFolder == "Y"))
                {
                    break;
                }
                DirectoryInfo directoryInfo = new DirectoryInfo(strPath);
                DirectoryInfo[] directories = directoryInfo.GetDirectories();
                foreach (DirectoryInfo directoryInfo2 in directories)
                {
                    if (GetListFilesInfo(item, directoryInfo2.FullName) != null && directoryInfo2.Name != "upload")
                    {
                        _logger.Information(GetFiles(item, directoryInfo2.FullName));
                        list.Add(GetListFilesInfo(item, directoryInfo2.FullName));
                    }
                }
            }
            list2 = strLatesFileOf(list);
            if (list2 != null)
            {
                _logger.Information("============================================");
                _logger.Information(">>>> [RESULT] File Penjualan yang akan di upload adalah : \n" + list2.First());
            }
            else
            {
                _logger.Information("********************************************");
                _logger.Information(">>>> [RESULT] Tidak ada file Penjualan di temukan !! ");
                _logger.Information("********************************************\n");
            }
        }
        else if (mode == Ft.Payment)
        {
            List<FileInfo> list3 = new List<FileInfo>();
            foreach (string item2 in strFilePattern)
            {
                _logger.Information(">>>> [OUTPUT] Mencari file Pembayaran yang mempunya nama '*" + item2.Trim() + "*'...");
                _logger.Information(GetFiles(item2, strPath));
                if (GetListFilesInfo(item2, strPath) != null)
                {
                    list3.Add(GetListFilesInfo(item2, strPath));
                }
                if (!(strSearchSubFolder == "Y"))
                {
                    break;
                }
                DirectoryInfo directoryInfo3 = new DirectoryInfo(strPath);
                DirectoryInfo[] directories2 = directoryInfo3.GetDirectories();
                foreach (DirectoryInfo directoryInfo4 in directories2)
                {
                    if (GetListFilesInfo(item2, directoryInfo4.FullName) != null && directoryInfo4.Name != "upload")
                    {
                        _logger.Information(GetFiles(item2, directoryInfo4.FullName));
                        list3.Add(GetListFilesInfo(item2, directoryInfo4.FullName));
                    }
                }
            }
            list2 = strLatesFileOf(list3);
            if (list2 != null)
            {
                _logger.Information("============================================");
                _logger.Information(">>>> [RESULT] File Pembayaran yang akan di upload adalah : \n" + list2.First());
            }
            else
            {
                _logger.Information("********************************************");
                _logger.Information(">>>> [RESULT] Tidak ada file Pembayaran di temukan !!");
                _logger.Information("********************************************\n");
            }
        }
        else
        {
            List<FileInfo> list4 = new List<FileInfo>();
            foreach (string item3 in strFilePattern)
            {
                _logger.Information(">>>> [OUTPUT] Mencari file Outlet yang mempunya nama '*" + item3.Trim() + "*'...");
                _logger.Information(GetFiles(item3, strPath));
                if (GetListFilesInfo(item3, strPath) != null)
                {
                    list4.Add(GetListFilesInfo(item3, strPath));
                }
                if (!(strSearchSubFolder == "Y"))
                {
                    break;
                }
                DirectoryInfo directoryInfo5 = new DirectoryInfo(strPath);
                DirectoryInfo[] directories3 = directoryInfo5.GetDirectories();
                foreach (DirectoryInfo directoryInfo6 in directories3)
                {
                    if (GetListFilesInfo(item3, directoryInfo6.FullName) != null && directoryInfo6.Name != "upload")
                    {
                        _logger.Information(GetFiles(item3, directoryInfo6.FullName));
                        list4.Add(GetListFilesInfo(item3, directoryInfo6.FullName));
                    }
                }
            }
            list2 = strLatesFileOf(list4);
            if (list2 != null)
            {
                _logger.Information("============================================");
                _logger.Information(">>>> [RESULT] File Outlet yang akan di upload adalah : \n" + list2.First());
            }
            else
            {
                _logger.Information("********************************************");
                _logger.Information(">>>> [RESULT] Tidak ada file Outlet di temukan !!");
                _logger.Information("********************************************\n");
            }
        }
        if (list2 != null)
        {
            _logger.Information(">>>> [OUTPUT] Dan waktu akses terakhir file tsb : " + File.GetLastWriteTime(list2.First()).ToLocalTime());
            _logger.Information("============================================\n");
            return list2.First();
        }
        else
        {
            return "";
        }
    }
    private static List<string> strLatesFileOf(List<FileInfo> files)
    {
        files.RemoveAll((item) => item == null);
        string text = "";
        string text2 = "";
        if (files.Any())
        {
            text = files.First().FullName;
            DateTime dateTime = files.First().LastWriteTime;
            foreach (FileInfo file in files)
            {
                DateTime lastWriteTime = file.LastWriteTime;
                if (lastWriteTime > dateTime)
                {
                    dateTime = lastWriteTime;
                    text = file.FullName;
                    text2 = file.Name;
                }
            }
            return new List<string> { text, text2 };
        }
        return null;
    }
}
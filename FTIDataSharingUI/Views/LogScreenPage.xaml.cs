using System.Collections.ObjectModel;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Extensions.Logging;
using Serilog.Core;
using WinUIEx.Messaging;
using System.Text.RegularExpressions;
using DataSubmission.ViewModels;
using DataSubmission.Contracts.Services;
using DataSubmission.Models;
using DataSubmission.Views;
using System.Text;

using System;
using System.Collections.Generic;
using System.IO;
//using System.Text;

namespace FTIDataSharingUI.Views;

public sealed partial class LogScreenPage : Page
{
    public LogScreenViewModel ViewModel
    {
        get;
    }

    public LogScreenPage()
    {
        ViewModel = App.GetService<LogScreenViewModel>();
        InitializeComponent();
        LoadLogData();
        ThemeHelper.ApplyTheme(this);
    }
    private MyParameterType _ParameterType = new();

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is MyParameterType parameter)
        {
            // Use the parameter
            _ParameterType = parameter;
            UserGreetings01.Text = "Hai, " + _ParameterType.Property2;
        }
    }

    public ObservableCollection<LogEntry> LogData { get; } = new ObservableCollection<LogEntry>();

    private async void LoadLogData()
    {
        try
        {

            var downloadsFolder = await StorageFolder.GetFolderFromPathAsync("C:\\");

            // Get the Data Sharing working folder
            var downloadsPath = Path.Combine(@"C:\ProgramData\FairbancData", "Datasharing-result");
            //downloadsPath = Path.Combine(@"C:\ProgramData\FairbancData", "");

            Directory.CreateDirectory(downloadsPath);
            downloadsFolder = await StorageFolder.GetFolderFromPathAsync(downloadsPath);

            // Get all files in the Downloads folder
            var files = await downloadsFolder.GetFilesAsync();

            StorageFile latestLogFile = null;
            DateTime latestCreationTime = DateTime.MinValue;

            foreach (var file in files)
            {
                if (file.FileType == ".log" && file.DateCreated > latestCreationTime.ToLocalTime())
                {
                    latestLogFile = file;
                    latestCreationTime = file.DateCreated.LocalDateTime;
                }
            }

            if (latestLogFile != null)
            {
                // Read log data from Log file
                string logContent = await FileIO.ReadTextAsync(latestLogFile);

                // Split log content into individual entries
                var logEntries = logContent.Split(new[] { "Log Entry :" }, StringSplitOptions.RemoveEmptyEntries);
                try
                {
                    foreach (var entry in logEntries.Reverse())
                    {
                        var pattern = @"[A-Za-z]+, ([A-Za-z]+ \d+, \d{4} \d+:\d+:\d+ [APap][Mm])";

                        Match match = Regex.Match(entry, pattern);
                        if (!match.Success)
                        { break; }
                        var dateTimeStr = match.Groups[1].Value;

                        var lines = entry.Split(new[] { '-' }, 2);
                        if (lines.Length >= 2)
                        {

                            var messagePart = lines[1].Trim();
                            string[] dateTimeParts = dateTimeStr.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
                            string datePart = $"{dateTimeParts[0]} {dateTimeParts[1]}";

                            if (dateTimeParts.Length >= 5)
                            {
                                LogData.Add(new LogEntry
                                {
                                    //Date = DateTime.Parse(dateTimeParts[0]).ToLongDateString(),
                                    Date = $"{dateTimeParts[0]} {dateTimeParts[1]} {dateTimeParts[2]}",
                                    Time = $"{dateTimeParts[3]} {dateTimeParts[4]}",
                                    Process = messagePart.Contains("WARNING") ? messagePart.Substring(1) : messagePart.Substring(1),
                                    Warning = messagePart.Contains("WARNING") ? "\uE7BA" : "\uE73E",
                                    Color = messagePart.Contains("WARNING") ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.ForestGreen)
                                });
                            }
                        }
                    }
                    logDataGrid.ItemsSource = LogData;
                }
                catch (Exception ex)
                {

                    LogException(ex);
                }
            }
            else
            {
                ContentDialog noLogFileDialog = new ContentDialog
                {
                    XamlRoot = this.XamlRoot,
                    Title = "Log File tidak ditemukan",
                    Content = "",
                    CloseButtonText = "Ok"
                };

                await noLogFileDialog.ShowAsync();
            }
        }
        catch (Exception ex)
        {
            ContentDialog errorDialog = new ContentDialog
            {
                XamlRoot = this.XamlRoot,
                Title = "Info",
                Content = $"An error occurred while loading log data: {ex.Message}",
                CloseButtonText = "Ok"
            };
            await errorDialog.ShowAsync();

            LogException(ex);
        }
        finally {
            //LogData.Clear();
        }
    }

    private void btnBack_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var navigationService = App.GetService<INavigationService>();
        navigationService.NavigateTo(typeof(MainMenuViewModel).FullName!, _ParameterType, true);
    }


    // Create a function to log exceptions
    string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "error.log");

    void LogException(Exception ex)
    {
        try
        {
            File.AppendAllText(logFilePath, $"{DateTime.Now}: {ex.Message}\n{ex.StackTrace}\n\n");
        }
        catch (Exception logEx)
        {
            Console.WriteLine($"Error while logging exception: {logEx.Message}");
        }
    }

    private async void Logout_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        ContentDialog errorDialog = new ContentDialog
        {
            XamlRoot = this.XamlRoot,
            Title = "Konfirmasi",
            PrimaryButtonText = "Tidak",
            CloseButtonText = "Ya",
            DefaultButton = ContentDialogButton.Primary,
            Content = $"Apakah anda ingin keluar ?"
        };
        var x = await errorDialog.ShowAsync();
        if (x == ContentDialogResult.Primary)
        {
            return;
        }
        var helper = new DataSubmission.Helpers.FileEnumeratorHelper();
        helper.DeleteIniFiles();
        App.MainWindow.Close();
    }
}



public class LogFileLoader
{
    public static List<string> LoadLogFiles(string folderPath, long maxSizeInKB)
    {
        // Step 1: Get list of all .log files in the folder, sorted by last modified date (newest first)
        var logFilesList = Directory.GetFiles(folderPath, "*.log");
        Array.Sort(logFilesList, (x, y) => File.GetLastWriteTime(y).CompareTo(File.GetLastWriteTime(x)));

        // Step 2: Create a collection to store log data
        var logDataCollection = new List<string>();
        long currentTotalSize = 0; // Track the total size in bytes

        // Step 3: Iterate through each log file
        foreach (var logFile in logFilesList)
        {
            try
            {
                // Step 3.1: Read the file content
                var logData = File.ReadAllText(logFile);
                var logDataSize = Encoding.UTF8.GetByteCount(logData); // Calculate size in bytes

                // Step 3.2: Check if adding this log would exceed the max size
                if (currentTotalSize + logDataSize <= maxSizeInKB * 1024)
                {
                    // Add the full log data if it fits within the limit
                    logDataCollection.Add(logData);
                    currentTotalSize += logDataSize;
                }
                else
                {
                    // If it exceeds, trim the log to fit the remaining space
                    var remainingSize = (maxSizeInKB * 1024) - currentTotalSize;
                    if (remainingSize > 0)
                    {
                        var trimmedLog = logData.Substring(logData.Length - (int)remainingSize / 2); // Keep only the latest part
                        logDataCollection.Add(trimmedLog);
                        currentTotalSize += remainingSize;
                    }
                    break; // Stop processing further logs once the limit is reached
                }
            }
            catch (Exception ex)
            {
                // Handle any errors (e.g., file reading issues)
                Console.WriteLine($"Error reading file {logFile}: {ex.Message}");
            }
        }

        // Step 4: Return the collection of log data
        return logDataCollection;
    }

    public static string CombineLogData(List<string> logDataCollection)
    {
        // Step 1: Initialize an empty string to hold the combined result
        var combinedLogData = string.Empty;

        // Step 2: Iterate through each log entry in the collection
        foreach (var logData in logDataCollection)
        {
            // Step 3: Append each log entry to the combined string
            combinedLogData += logData + Environment.NewLine; // Add a newline for separation
        }

        // Step 4: Return the combined result
        return combinedLogData;
    }

}

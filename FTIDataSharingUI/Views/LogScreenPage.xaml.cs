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
    }
    private MyParameterType _ParameterType = new();

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is MyParameterType parameter)
        {
            // Use the parameter
            _ParameterType = parameter;
            TextBlock_UserGreetings.Text = "Hai, " + _ParameterType.Property2;
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
}

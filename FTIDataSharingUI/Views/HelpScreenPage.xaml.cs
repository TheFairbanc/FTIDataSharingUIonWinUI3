using System.Collections.ObjectModel;
using FTIDataSharingUI.ViewModels;
using FTIDataSharingUI.Models;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.Storage;
using Microsoft.UI;

namespace FTIDataSharingUI.Views;

public sealed partial class HelpScreenPage : Page
{
    public HelpScreenViewModel ViewModel
    {
        get;
    }

    public HelpScreenPage()
    {
        ViewModel = App.GetService<HelpScreenViewModel>();
        InitializeComponent();
        LoadLogData();
    }

    public ObservableCollection<LogEntry> LogData { get; } = new ObservableCollection<LogEntry>();

    private async void LoadLogData()
    {
        try
        {
            // Get the Downloads folder
            var downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            var downloadsFolder = await StorageFolder.GetFolderFromPathAsync(downloadsPath);

            // Get all files in the Downloads folder
            var files = await downloadsFolder.GetFilesAsync();

            // Find the file that matches the pattern DEBUG-???.log
            var logFile = files.FirstOrDefault(file => file.Name.StartsWith("DEBUG-") && file.Name.EndsWith(".log"));

            if (logFile != null)
            {
                // Read log data from Debug.Log (adjust the path as needed)
                string logContent = await FileIO.ReadTextAsync(logFile);

                // Split log content into individual entries
                var logEntries = logContent.Split(new[] { "Log Entry :" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var entry in logEntries)
                {
                    var lines = entry.Split(new[] { '-' }, 2);
                    if (lines.Length >= 2)
                    {
                        var dateTimePart = lines[0].Trim();
                        var messagePart = lines[1].Trim();

                        var dateTimeParts = dateTimePart.Split(new[] { ' ' }, 2);
                        if (dateTimeParts.Length == 2)
                        {
                            LogData.Add(new LogEntry
                            {
                                Time = DateTime.Parse(dateTimeParts[1]).ToString("dd-MM-yyyy"),
                                Date = dateTimeParts[0],
                                Process = messagePart.Contains("WARNING") ? messagePart : messagePart,
                                Warning = messagePart.Contains("WARNING") ? "\uE7BA" : "\uE73E",
                                Color = messagePart.Contains("WARNING") ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.ForestGreen)
                            });
                        }
                    }
                }

                logDataGrid.ItemsSource = LogData;
            }
            else
            {
                // Handle the case where the log file is not found
                ContentDialog noLogFileDialog = new ContentDialog
                {
                    XamlRoot = this.XamlRoot,
                    Title = "Log File Not Found",
                    Content = "No log file matching the pattern DEBUG-???.log was found in the downloads folder.",
                    CloseButtonText = "Ok"
                };

                await noLogFileDialog.ShowAsync();
            }
        }
        catch (Exception ex)
        {
            // Handle exceptions
            ContentDialog errorDialog = new ContentDialog
            {
                XamlRoot = this.XamlRoot,
                Title = "Info",
                Content = $"An error occurred while loading log data: {ex.Message}",
                CloseButtonText = "Ok"
            };

            await errorDialog.ShowAsync();
        }
    }
}


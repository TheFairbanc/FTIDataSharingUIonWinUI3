using System.Diagnostics;
using System.Text;
using DataSubmission.Contracts.Services;
using DataSubmission.Models;
using DataSubmission.ViewModels;
using DataSubmission.Views;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

namespace FTIDataSharingUI.Views;

public sealed partial class AutoProcessPage : Page
{
    public string serviceName
    {
        get; set;
    } = "DataSubmission";

    public AutoProcessViewModel ViewModel
    {
        get;
    }

    public AutoProcessPage()
    {
        ViewModel = App.GetService<AutoProcessViewModel>();
        InitializeComponent();
        ThemeHelper.ApplyTheme(this);
        ConfigButton.Foreground = new SolidColorBrush(Colors.White);
    }

    private int _StartButclickCount = 0;
    private MyParameterType _ParameterType = new();
    private readonly string command = "sc query DataSubmission";

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

    private string GetServiceState(string command)
    {
        var processInfo = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/C {command}",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            Verb = "runas"
    };

        var process = new Process
        {
            StartInfo = processInfo
        };

        var outputBuilder = new StringBuilder();

        process.OutputDataReceived += (sender, args) => outputBuilder.AppendLine(args.Data);
        process.Start();
        process.BeginOutputReadLine();
        process.WaitForExit();

       var output = outputBuilder.ToString();
        if (output.Contains("RUNNING"))
        {
            return "RUNNING";
        }
        else if (output.Contains("STOPPED"))
        {
            return "STOPPED";
        }
        else if (output.Contains("not exist"))
        {
            return "NOTINSTALLED";
        }
        else
        {
            return "UNKNOWN";
        }
    }

    private async void StartButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var readconfig = await ReadDateTimeFromFileAsync();
            if (!readconfig)
            {
                ContentDialog infoDialog = new ContentDialog();
                infoDialog.XamlRoot = this.XamlRoot;
                infoDialog.Title = "Informasi";
                infoDialog.CloseButtonText = "OK";
                infoDialog.DefaultButton = ContentDialogButton.Close;
                infoDialog.Content = "Belum tersedia konfigurasi untuk upload data.\nMohon melakukan konfigurasi terlebih dahulu.\nSilahkan gunakan tombol 'Config'.";
                await infoDialog.ShowAsync();
                return;
            }
            _StartButclickCount++;



            var processInfo = new ProcessStartInfo();
            {
                processInfo.CreateNoWindow = true;
                processInfo.UseShellExecute = true;
                processInfo.FileName = "sc";
                processInfo.Verb = "runas";

                if (GetServiceState(command) == "RUNNING")
                {
                    processInfo.Arguments = $"stop {serviceName}";
                }
                else 
                {
                    processInfo.Arguments = $"start {serviceName}";
                }
            }

            Process.Start(processInfo);

            Thread.Sleep(2000);
            var ServiceStatus = GetServiceState(command);
            if (ServiceStatus == "RUNNING")
            {
                StartButton.Background = new SolidColorBrush(Colors.LightGray);
                StartButton.Content = "Stop";
                StartButton.IsEnabled = true;
                TxtStatusValue.Text = " Start";
                PnlStatus.Background = new SolidColorBrush(ColorHelper.FromArgb(255, 0xD4, 0xF1, 0xEB));//#D4F1EB
                TxtStatus.Foreground = new SolidColorBrush(Colors.Green); //#D4F1EB;
                TxtStatusValue.Foreground = new SolidColorBrush(Colors.Green);
            }
            else if (ServiceStatus == "STOPPED")
            {
                StartButton.Background = new SolidColorBrush(ColorHelper.FromArgb(255, 0x44, 0xAB, 0x96)); //background: #D4F1EB;
                StartButton.Content = "Start";
                StartButton.IsEnabled = true;
                TxtStatusValue.Text = " Stop";
                PnlStatus.Background = new SolidColorBrush(ColorHelper.FromArgb(255, 0xEA, 0xEC, 0xF0)); //background: #EAECF0; //new SolidColorBrush(Colors.LightGreen);
                TxtStatus.Foreground = new SolidColorBrush(Colors.Gray); ;
                TxtStatusValue.Foreground = new SolidColorBrush(Colors.Gray);
            }
            else 
            {
                StartButton.Background = new SolidColorBrush(ColorHelper.FromArgb(255, 0x44, 0xAB, 0x96)); //background: #D4F1EB;
                StartButton.Content = "Start";
                StartButton.IsEnabled = false;
                TxtStatusValue.Text = " N/A";
                PnlStatus.Background = new SolidColorBrush(ColorHelper.FromArgb(255, 0xEA, 0xEC, 0xF0)); //background: #EAECF0; //new SolidColorBrush(Colors.LightGreen);
                TxtStatus.Foreground = new SolidColorBrush(Colors.Gray); ;
                TxtStatusValue.Foreground = new SolidColorBrush(Colors.Gray);
            }
        }
        catch (Exception ex)
        {
            ContentDialog infoDialog = new ContentDialog();
            infoDialog.XamlRoot = this.XamlRoot;
            infoDialog.Title = "Peringatan";
            infoDialog.CloseButtonText = "OK";
            infoDialog.DefaultButton = ContentDialogButton.Close;
            infoDialog.Content = "Scheduler service belum data di jalankan/install.\nMohon hubungi pihak Fairbanc.\n\n\"Error starting service: {ex.Message}\"";
            await infoDialog.ShowAsync();
        }
        
    }

    private async void ConfigButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var readconfig = await ReadDateTimeFromFileAsync();
            if (!readconfig)
            {
                ContentDialog infoDialog = new ContentDialog();
                infoDialog.XamlRoot = this.XamlRoot;
                infoDialog.Title = "Informasi";
                infoDialog.CloseButtonText = "OK";
                infoDialog.DefaultButton = ContentDialogButton.Close;
                infoDialog.Content = "Sistem akan membuat konfigurasi baru. Lanjukan ?";
                await infoDialog.ShowAsync();
                //return;
            }
            var navigationService = App.GetService<INavigationService>();
            navigationService.NavigateTo(typeof(AutoConfigViewModel).FullName!, _ParameterType, false);
        }
        catch (Exception)
        {

        }
    }

    private void btnBack_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var navigationService = App.GetService<INavigationService>();
        navigationService.NavigateTo(typeof(MainMenuViewModel).FullName!, _ParameterType, true);
    }

    async Task<bool> ReadDateTimeFromFileAsync()
    {
        try
        {
            var ConfigFolder = @"C:\ProgramData\FairbancData";
            var filePath = Path.Combine(ConfigFolder, "DateTimeInfo.ini");

            if (File.Exists(filePath))
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string date1 = await reader.ReadLineAsync(); // [DATE#1]
                    string runDate1 = await reader.ReadLineAsync();
                    string date2 = await reader.ReadLineAsync(); // [DATE#2]
                    string runDate2 = await reader.ReadLineAsync();
                    string date3 = await reader.ReadLineAsync(); // [DATE#3]
                    string runDate3 = await reader.ReadLineAsync();
                    string timeLabel = await reader.ReadLineAsync(); // [TIME]
                    string time = await reader.ReadLineAsync();
                    string salesLabel = await reader.ReadLineAsync(); // [SALES]
                    string sales = await reader.ReadLineAsync();
                    string arLabel = await reader.ReadLineAsync(); // [AR]
                    string ar = await reader.ReadLineAsync();
                    string outletLabel = await reader.ReadLineAsync(); // [OUTLET]
                    string outlet = await reader.ReadLineAsync();
                    string foldernamelabel = await reader.ReadLineAsync(); // [FOLDER]
                    string foldername = await reader.ReadLineAsync();

                }
                return true;
            }
            else
            {
                return false;
            }
        }
        catch (Exception)
        {
            return false;
        }
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        await Task.Delay(3000);
        var ServiceStatus = GetServiceState(command);
        if (ServiceStatus == "RUNNING")
        //{
        //    StartButton.Background = new SolidColorBrush(Colors.LightGray);
        //    StartButton.Content = "Stop";
        //    TxtStatusValue.Text = " Start";
        //    PnlStatus.Background = new SolidColorBrush(Colors.LightGreen);
        //    TxtStatus.Foreground = new SolidColorBrush(Colors.Green);
        //    TxtStatusValue.Foreground = new SolidColorBrush(Colors.Green);
        //}
        //else
        //{
        //    StartButton.Background = new SolidColorBrush(Colors.ForestGreen);
        //    StartButton.Content = "Start";
        //    TxtStatusValue.Text = " Stop";
        //    PnlStatus.Background = new SolidColorBrush(Colors.LightGray);
        //    TxtStatus.Foreground = new SolidColorBrush(Colors.Gray);
        //    TxtStatusValue.Foreground = new SolidColorBrush(Colors.Gray);
        //}
        {
            StartButton.Background = new SolidColorBrush(Colors.LightGray);
            StartButton.Content = "Stop";
            TxtStatusValue.Text = " Start";
            PnlStatus.Background = new SolidColorBrush(ColorHelper.FromArgb(255, 0xD4, 0xF1, 0xEB));//#D4F1EB
            TxtStatus.Foreground = new SolidColorBrush(Colors.Green); //#D4F1EB;
            TxtStatusValue.Foreground = new SolidColorBrush(Colors.Green);
        }
        else if (ServiceStatus == "STOPPED")
        {
            StartButton.Background = new SolidColorBrush(ColorHelper.FromArgb(255, 0x44, 0xAB, 0x96)); //background: #D4F1EB;
            StartButton.Content = "Start";
            TxtStatusValue.Text = " Stop";
            PnlStatus.Background = new SolidColorBrush(ColorHelper.FromArgb(255, 0xEA, 0xEC, 0xF0)); //background: #EAECF0; //new SolidColorBrush(Colors.LightGreen);
            TxtStatus.Foreground = new SolidColorBrush(Colors.Gray); ;
            TxtStatusValue.Foreground = new SolidColorBrush(Colors.Gray);
        }
        else
        {
            StartButton.Background = new SolidColorBrush(ColorHelper.FromArgb(255, 0x44, 0xAB, 0x96)); //background: #D4F1EB;
            StartButton.Content = "Start";
            StartButton.IsEnabled = false;
            TxtStatusValue.Text = " N/A";
            PnlStatus.Background = new SolidColorBrush(ColorHelper.FromArgb(255, 0xEA, 0xEC, 0xF0)); //background: #EAECF0; //new SolidColorBrush(Colors.LightGreen);
            TxtStatus.Foreground = new SolidColorBrush(Colors.Gray); ;
            TxtStatusValue.Foreground = new SolidColorBrush(Colors.Gray);
        }
    }
}




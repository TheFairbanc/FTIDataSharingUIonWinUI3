using DataSubmission.Contracts.Services;
using DataSubmission.Models;
using DataSubmission.ViewModels;
using DataSubmission.Services;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using DataSubmission.Views;
using DataSubmissionApp.Helpers;

namespace FTIDataSharingUI.Views;

public sealed partial class MainMenuPage : Page
{
    public MainMenuViewModel ViewModel
    {
        get;
    }

    public MainMenuPage()
    {
        ViewModel = App.GetService<MainMenuViewModel>();
        InitializeComponent();
        ThemeHelper.ApplyTheme(this);
    }

    private MyParameterType _ParameterType = new MyParameterType();

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

    private void ButtonManual_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        try
        {
            var navigationService = App.GetService<INavigationService>();
            navigationService.NavigateTo(typeof(ManualProcessViewModel).FullName!, _ParameterType, true);
        }
        catch (Exception ex) 
        { 
            LogException(ex); 
        }
    }

    private async void ButtonAuto_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        try
        {
            DonwloadAndInstallService _donwloadAndInstallService = new DonwloadAndInstallService();
            var isServiceDownloaded = await _donwloadAndInstallService.IsServiceInstalled();
            if (!isServiceDownloaded)
            {
                await _donwloadAndInstallService.StartDownloading();
            }
            await Task.Delay(3000);
            var navigationService = App.GetService<INavigationService>();
            navigationService.NavigateTo(typeof(AutoProcessViewModel).FullName!, _ParameterType, true);
        }
        catch (Exception ex)
        {
            LogException(ex);
        }
    }

    private void ButtonLogs_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        try
        {
            var navigationService = App.GetService<INavigationService>();
            navigationService.NavigateTo(typeof(LogScreenViewModel).FullName!, _ParameterType, true);
            navigationService = null;

        }
        catch (Exception ex)
        {

            LogException(ex);  
        }

    }

    private async void LogException(Exception ex)
    {
        try
        {
            // Create a function to log exceptions
            string logFilePath = Path.Combine(@"C:\ProgramData\FairbancData", "error.log");
            // Append the exception message and stack trace to the log file
            File.AppendAllText(logFilePath, $"{DateTime.Now}: {ex.Message}\n{ex.StackTrace}\n\n");
        }
        catch (Exception logEx)
        {
            // Handle any exceptions related to logging itself (e.g., permission issues)
            ContentDialog errorDialog = new ContentDialog
            {
                XamlRoot = this.XamlRoot,
                Title = "Info Kesalahan",
                CloseButtonText = "OK",
                DefaultButton = ContentDialogButton.Close,
                Content = $"Terjadi error sebagai berikut:/n {logEx.Message}"
            };
            await errorDialog.ShowAsync();
        }
    }
}



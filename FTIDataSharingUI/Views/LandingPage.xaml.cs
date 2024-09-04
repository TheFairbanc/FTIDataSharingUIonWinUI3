using FTIDataSharingUI.Contracts.Services;
using FTIDataSharingUI.ViewModels;
using FTIDataSharingUI.Models;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage;
using Microsoft.UI.Xaml.Navigation;

namespace FTIDataSharingUI.Views;

public sealed partial class LandingPage : Page
{
    private enum IniFileMode
    {
        AutomaticUpload,ManualUpload
    }
    public LandingViewModel ViewModel
    {
        get;
    }

    public  LandingPage()
    {
        ViewModel = App.GetService<LandingViewModel>();
        InitializeComponent();
    }

    private MyParameterType _ParameterType =  new();

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (await CheckInitialFileExist( IniFileMode.AutomaticUpload) || await CheckInitialFileExist(IniFileMode.ManualUpload))
        {
            var navigationService = App.GetService<INavigationService>();
            navigationService.NavigateTo(typeof(MainMenuViewModel).FullName!, _ParameterType, false);
        }

    }
    private void agreeCheckBox_Checked(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        ButtonAgree.IsEnabled = true;
    }

    private void agreeCheckBox_Unchecked(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        this.ButtonAgree.IsEnabled = false;
    }

    private void ButtonAgree_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var navigationService = App.GetService<INavigationService>();
        navigationService.NavigateTo(typeof(LoginViewModel).FullName!, null, true);
    }

    private async Task<bool> CheckInitialFileExist(IniFileMode param)
    {
        try
        {
            var configFolder = @"C:\ProgramData\FairbancData";
            var filePath = "";
            filePath = param == IniFileMode.AutomaticUpload ? Path.Combine(configFolder, "DateTimeInfo.ini") : Path.Combine(configFolder, "ManualUploadInfo.ini");


            if (File.Exists(filePath))
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string line;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        if (line.Trim() == "[DTID]")
                        {
                            _ParameterType.Property1  = await reader.ReadLineAsync();
                        }
                        else if (line.Trim() == "[DTNAME]")
                        {
                            _ParameterType.Property2 = await reader.ReadLineAsync();
                        }
                    }
                }
                if (_ParameterType.Property1 == null || _ParameterType.Property2 == null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }
        catch (Exception ex)
        {
            return false;
        }
    }

}


using FTIDataSharingUI.Contracts.Services;
using FTIDataSharingUI.Models;
using FTIDataSharingUI.ViewModels;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

namespace FTIDataSharingUI.Views;

public sealed partial class AutoProcessPage : Page
{
    public AutoProcessViewModel ViewModel
    {
        get;
    }

    public AutoProcessPage()
    {
        ViewModel = App.GetService<AutoProcessViewModel>();
        InitializeComponent();
    }

    private int _StartButclickCount = 0;
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

    private async void StartButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var readconfig = await ReadDateTimeFromFileAsync();
            if (!readconfig)
            {
                ContentDialog infoDialog = new ContentDialog();
                infoDialog.XamlRoot = this.XamlRoot;
                infoDialog.Title = "Info";
                infoDialog.CloseButtonText = "OK";
                infoDialog.DefaultButton = ContentDialogButton.Close;
                infoDialog.Content = "Konfigurasi belum tersedia.\nMohon melakukan konfigurasi terlebih dahulu !";
                await infoDialog.ShowAsync();
                return;
            }
            _StartButclickCount++;

            if (_StartButclickCount % 2 == 1)
            {
                StartButton.Background = new SolidColorBrush(Colors.LightGray);
                StartButton.Content = "Stop";
                TxtStatusValue.Text = " Start";
                PnlStatus.Background = new SolidColorBrush(Colors.LightGreen);
                TxtStatus.Foreground = new SolidColorBrush(Colors.Green);
                TxtStatusValue.Foreground = new SolidColorBrush(Colors.Green);
            }
            else
            {
                StartButton.Background = new SolidColorBrush(Colors.ForestGreen);
                StartButton.Content = "Start";
                TxtStatusValue.Text = " Stop";
                PnlStatus.Background = new SolidColorBrush(Colors.LightGray);
                TxtStatus.Foreground = new SolidColorBrush(Colors.Gray);
                TxtStatusValue.Foreground = new SolidColorBrush(Colors.Gray);
            }
        }
        catch (Exception)
        {

            throw;
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
                infoDialog.Title = "Info";
                infoDialog.CloseButtonText = "OK";
                infoDialog.DefaultButton = ContentDialogButton.Close;
                infoDialog.Content = "Konfigurasi belum tersedia.\nSistem akan membuat konfigurasi baru !";
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
            var ConfogFolder = @"c:\temp";
            var filePath = Path.Combine(ConfogFolder, "DateTimeInfo.ini");

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
        catch (Exception ex)
        {
            return false;
        }
    }
}




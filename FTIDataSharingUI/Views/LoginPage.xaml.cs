using System.Text.Json;
using DataSubmission.Models;
using Microsoft.UI.Xaml.Controls;
using DataSubmission.ViewModels;
using DataSubmission.Contracts.Services;
using DataSubmission.Views;

namespace FTIDataSharingUI.Views;

public sealed partial class LoginPage : Page
{
    public LoginViewModel ViewModel
    {
        get;
    }

    public LoginPage()
    {
        ViewModel = App.GetService<LoginViewModel>();
        InitializeComponent();
        ThemeHelper.ApplyTheme(this);
    }
    private string dtIDandName
    {
        get;
        set;
    } = "";

    private void DTIDTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (DTIDTextBox.Text != "")
        {
            ButtonLogin.IsEnabled = true;
        }
        else
        {
            ButtonLogin.IsEnabled = false;
        }
    }

    private async void ButtonLogin_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        try
        {
#if (DEBUG)
#else
            if (TextBox_Password.Password.Trim() != "123")
            {
                ContentDialog infoDialog = new ContentDialog();
                infoDialog.XamlRoot = this.XamlRoot;
                infoDialog.Title = "Info";
                infoDialog.CloseButtonText = "OK";
                infoDialog.DefaultButton = ContentDialogButton.Close;
                infoDialog.Content = "Password salah.";
                await infoDialog.ShowAsync();
                return;
            }
#endif
            var value = await GetDistributorNameAsync();
            if (value == "")
            {
                // No DT Id Entered.
                ContentDialog infoDialog = new ContentDialog();
                infoDialog.XamlRoot = this.XamlRoot;
                infoDialog.Title = "Info";
                infoDialog.CloseButtonText = "OK";
                infoDialog.DefaultButton = ContentDialogButton.Close;
                infoDialog.Content = "Nomer registrasi DT tidak di temukan di basis data Fairbanc !";
                await infoDialog.ShowAsync();
                return;
            }

            var parameter = new MyParameterType { Property1 = DTIDTextBox.Text.Trim(), Property2 = dtIDandName };
            var navigationService = App.GetService<INavigationService>();
            navigationService.NavigateTo(typeof(MainMenuViewModel).FullName!, parameter, true);
        }
        catch (Exception ex)
        {

            throw ex;
        }

    }

    public async Task<string> GetDistributorNameAsync()
    {
        try
        {
            if (DTIDTextBox.Text.Trim() == "")
            {
                return "";
            }
            string apiUrl = "https://dashboard.fairbanc.app/api/distributors/" + DTIDTextBox.Text.Trim() + "?api_token=2S0VtpYzETxDrL6WClmxXXnOcCkNbR5nUCCLak6EHmbPbSSsJiTFTPNZrXKk2S0VtpYzETxDrL6WClmx";

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string responseData = await response.Content.ReadAsStringAsync();

                    // Parse the JSON response
                    using (JsonDocument doc = JsonDocument.Parse(responseData))
                    {
                        JsonElement root = doc.RootElement;
                        dtIDandName = root.GetProperty("name").GetString();
                        return dtIDandName;
                    }
                }
                else
                {
                    // Handle error response here
                    return "";
                }
            }
        }
        catch (Exception)
        {
            return "";
        }

    }
}


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
//#if (DEBUG)
            //var parameter = new MyParameterType { Property1 = DTIDTextBox.Text.Trim(), Property2 = dtIDandName };
            //var navigationService = App.GetService<INavigationService>();
            //navigationService.NavigateTo(typeof(MainMenuViewModel).FullName!, parameter, true);
//#else
            if (TextBox_Password.Password.Trim() == "")
            {
                // No DT Id Entered.
                ContentDialog infoDialog = new ContentDialog();
                infoDialog.XamlRoot = this.XamlRoot;
                infoDialog.Title = "Info";
                infoDialog.CloseButtonText = "OK";
                infoDialog.DefaultButton = ContentDialogButton.Close;
                infoDialog.Content = "Mohon masukan Password !";
                await infoDialog.ShowAsync();
                return;
            }

            var password = GeneratePassword(DTIDTextBox.Text.Trim(), dtIDandName);
            if (password == TextBox_Password.Password.Trim())
            {
                var parameter = new MyParameterType { Property1 = DTIDTextBox.Text.Trim(), Property2 = dtIDandName };
                var navigationService = App.GetService<INavigationService>();
                navigationService.NavigateTo(typeof(MainMenuViewModel).FullName!, parameter, true);
            }
            else
            {
                ContentDialog infoDialog = new ContentDialog();
                infoDialog.XamlRoot = this.XamlRoot;
                infoDialog.Title = "Password";
                infoDialog.CloseButtonText = "OK";
                infoDialog.DefaultButton = ContentDialogButton.Close;
                infoDialog.Content = "Password salah !";
                await infoDialog.ShowAsync();
            }


//#endif
        }
        catch (Exception)
        {

            ContentDialog infoDialog = new ContentDialog();
            infoDialog.XamlRoot = this.XamlRoot;
            infoDialog.Title = "Informasi";
            infoDialog.CloseButtonText = "OK";
            infoDialog.DefaultButton = ContentDialogButton.Close;
            infoDialog.Content = "Maaf, telah terjadi kesalahan.";
            await infoDialog.ShowAsync();
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
            string apiUrl = "https://dashboard.fairbanc.app/api/distributors/" + DTIDTextBox.Text.Trim() + "?api_token=HSpanHVAijRrrqZpWgLHxhXpvSBxJSutLPpLbbYqjXoxrscxPsbmCLZMeMKRHFzsFcnieMvqpKadiDLx";

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

    private string GeneratePassword(string number, string text)
    {
        //GSheet Formulat >> =CONCATENATE(LEFT(A2, 2), MID(B2, 4, 4), LEN(B2), REPT("*", 3), RIGHT(A2, 1))

        var leftPart = "";
        if (number.Length < 2)
        {
            leftPart = number;
        }
        else
        {
            leftPart = number.Substring(0, 2); // First 2 characters of A2
        }
        string midPart = text.Substring(3, Math.Min(4, text.Length - 3)); // 4 characters starting from the 4th char in B2
        int lengthOfText = text.Length; // Length of B2
        string separator = new string('*', 3); // Separator
        string rightPart = number.Substring(number.Length - 1, 1); // Last character of A2

        return leftPart + midPart + lengthOfText + separator + rightPart;
    }

}


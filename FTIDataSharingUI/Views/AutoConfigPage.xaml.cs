using System.Diagnostics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.Storage.Pickers;
using Microsoft.UI.Xaml.Navigation;
using DataSubmission.ViewModels;
using DataSubmission.Contracts.Services;
using DataSubmission.Models;
using DataSubmission.Views;

namespace FTIDataSharingUI.Views;

public sealed partial class AutoConfigPage : Page
{
    public AutoConfigViewModel ViewModel
    {
        get;
    }

    public AutoConfigPage()
    {
        ViewModel = App.GetService<AutoConfigViewModel>();
        InitializeComponent();
        JamMenit_Picker.Time = DateTime.Now.TimeOfDay;
        _ = ReadDateTimeFromFileAsync();
        ThemeHelper.ApplyTheme(this);
    }


    private MyParameterType _ParameterType;

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

    private async void BtnSearch_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var hwnd = App.MainWindow.GetWindowHandle();
        FolderPicker openPicker = new Windows.Storage.Pickers.FolderPicker();
        openPicker.SuggestedStartLocation = PickerLocationId.Desktop;
        openPicker.FileTypeFilter.Add("*");

        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);

        WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

        var folder = await openPicker.PickSingleFolderAsync();
        if (folder != null)
        {
            FolderSelected.Text = @folder.Path;
        }

    }

    private async void btnCancel_Click(object sender, RoutedEventArgs e)
    {
        ContentDialog dialog = new ContentDialog();

        // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
        dialog.XamlRoot = this.XamlRoot;
        dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
        dialog.Title = "Konfirmasi";
        dialog.Content = "Apakah anda ingin mengbaikan perubahan konfigurasi ?";
        dialog.PrimaryButtonText = "Ya";
        dialog.CloseButtonText = "Tidak";
        dialog.DefaultButton = ContentDialogButton.Secondary;

        var result = await dialog.ShowAsync();
        var navigationService = App.GetService<INavigationService>();
        _ = navigationService.GoBack();
    }


    private void Button_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        AnimatedIcon.SetState(this.SearchAnimatedIcon, "PointerOver");
    }

    private void Button_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        AnimatedIcon.SetState(this.SearchAnimatedIcon, "Normal");
    }

    private void NumberBoxSpinButton_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        if (sender.Value.ToString() == "NaN")
        {
            sender.Value = 1;
        }
    }

    private async void bntSave_Click(object sender, RoutedEventArgs e)
    {
        //Prepare Error message dialog box
        ContentDialog errorDialog = new ContentDialog();
        errorDialog.XamlRoot = this.XamlRoot;
        errorDialog.Title = "Peringatan";
        errorDialog.CloseButtonText = "Kembali";
        errorDialog.DefaultButton = ContentDialogButton.Close;

        //Start the UI component validation
        if (
            (NumberBox_RunDate1.Value > NumberBox_RunDate2.Value) ||
            (NumberBox_RunDate2.Value > NumberBox_RunDate3.Value) ||
            (NumberBox_RunDate3.Value < NumberBox_RunDate1.Value))
        {

            errorDialog.Content = "Mohon di cek kembali settingan semua tanggal.";
            NumberBox_RunDate1.Focus(FocusState.Programmatic);
            await errorDialog.ShowAsync();
            return;
        }

        if (FolderSelected.Text.Trim() == "")
        {
            errorDialog.Content = "Mohon di cek kembali pilihan folder.";
            Button_Pointer.Focus(FocusState.Programmatic);
            await errorDialog.ShowAsync();
            return;
        }

        if (TextBox_NamaFileSales.Text.Trim() == "")
        {
            errorDialog.Content = "Mohon di cek nama/kata untuk file 'Penjualan'.";
            TextBox_NamaFileSales.Focus(FocusState.Programmatic);
            await errorDialog.ShowAsync();
            return;
        }

        TextBox_NamaFileSales.Text = TextBox_NamaFileSales.Text.ToLower();
        TextBox_NamaFileAR.Text = TextBox_NamaFileAR.Text.ToLower();
        TextBox_NamaFileOutlet.Text = TextBox_NamaFileOutlet.Text.ToLower();
        var blnSuccess = WriteConfigToFileAsync();

        if (blnSuccess.Result)
        {
            errorDialog.Title = "Informasi";
            errorDialog.CloseButtonText = "OK";
            errorDialog.Content = "Konfigurasi berhasil di simpan.";
            await errorDialog.ShowAsync();
        }
        else
        {
            errorDialog.Title = "Informasi";
            errorDialog.CloseButtonText = "OK";
            errorDialog.Content = "Konfigurasi gagal di simpan!";
            await errorDialog.ShowAsync();
        }



        var navigationService = App.GetService<INavigationService>();
        _ = navigationService.GoBack();
    }

    async Task<bool> WriteConfigToFileAsync()
    {
        try
        {
            var folder = @"C:\ProgramData\FairbancData";
            var filePath = Path.Combine(folder, "DateTimeInfo.ini");

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                await writer.WriteLineAsync("[DATE#1]");
                await writer.WriteLineAsync(NumberBox_RunDate1.Value.ToString());
                await writer.WriteLineAsync("[DATE#2]");
                await writer.WriteLineAsync(NumberBox_RunDate2.Value.ToString());
                await writer.WriteLineAsync("[DATE#3]");
                await writer.WriteLineAsync(NumberBox_RunDate3.Value.ToString());
                await writer.WriteLineAsync("[TIME]");
                await writer.WriteLineAsync(JamMenit_Picker.Time.ToString());
                await writer.WriteLineAsync("[SALES]");
                await writer.WriteLineAsync(TextBox_NamaFileSales.Text.Trim());
                await writer.WriteLineAsync("[REPAYMENT]");
                await writer.WriteLineAsync(TextBox_NamaFileAR.Text.Trim());
                await writer.WriteLineAsync("[OUTLET]");
                await writer.WriteLineAsync(TextBox_NamaFileOutlet.Text.Trim());
                await writer.WriteLineAsync("[FOLDER]");
                await writer.WriteLineAsync(FolderSelected.Text.Trim());
                await writer.WriteLineAsync("[DTID]");
                await writer.WriteLineAsync(_ParameterType.Property1.Trim());
                await writer.WriteLineAsync("[DTNAME]");
                await writer.WriteLineAsync(_ParameterType.Property2.Trim());
            }
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception: {ex.Message}");
            return false;
        }
    }

    async Task<bool> ReadDateTimeFromFileAsync()
    {
        var folder = @"C:\ProgramData\FairbancData";
        var filePath = Path.Combine(folder, "DateTimeInfo.ini");
        try
        {
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
                    string dtidLabel = await reader.ReadLineAsync(); // [DTID]
                    string dtid = await reader.ReadLineAsync();
                    string dtnamelabel = await reader.ReadLineAsync(); // [DTNAME]
                    string dtname = await reader.ReadLineAsync();

                    // Process the variables as needed
                    NumberBox_RunDate1.Value = Convert.ToDouble(runDate1);
                    NumberBox_RunDate2.Value = Convert.ToDouble(runDate2);
                    NumberBox_RunDate3.Value = Convert.ToDouble(runDate3);
                    JamMenit_Picker.Time = TimeSpan.FromMinutes(Convert.ToInt32(time.Substring(0, 2).Substring(0, 2)) * 60 + Convert.ToInt32(time.Substring(3, 2)));
                    TextBox_NamaFileSales.Text = sales;
                    TextBox_NamaFileAR.Text = ar;
                    TextBox_NamaFileOutlet.Text = outlet;
                    FolderSelected.Text = foldername;
                }
                return true;
            }
            else
            {
                return false;
            }
        }
        catch (Exception )
        {
            //Debug.WriteLine($"Exception: {ex.Message}");

            ContentDialog errorDialog = new ContentDialog();
            errorDialog.XamlRoot = this.XamlRoot;
            errorDialog.Title = "Info";
            errorDialog.CloseButtonText = "Cancel";
            errorDialog.DefaultButton = ContentDialogButton.Close;
            errorDialog.Content = $"Gagal membaca file konfigurasi di folder {folder}.";
            await errorDialog.ShowAsync();
            return false;
        }
    }

}


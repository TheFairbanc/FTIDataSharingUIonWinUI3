using System.IO;
using System.Xml.Linq;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Reflection.Metadata.Ecma335;
using DataSubmission.Views;
using DataSubmission.Models;
using DataSubmission.Helpers;
using DataSubmission.Services;
using DataSubmission.ViewModels;
using DataSubmission.Contracts.Services;
using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Navigation;
using WinUIEx;
using Serilog;
using Windows.Storage;
using OfficeOpenXml.Style;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage.Pickers;
using System.Linq.Expressions;
using System;
using DataSubmissionApp.Helpers;


namespace FTIDataSharingUI.Views;

public sealed partial class ManualProcessPage : Page
{
    System.Globalization.CultureInfo indonesianCulture = new System.Globalization.CultureInfo("id-ID");
    private ObservableCollection<string> cbitem = new ObservableCollection<string>();
    private MyParameterType _ParameterType = new();
    //private const int MaxFiles = 1;
    private List<StorageFile> droppedFilesSales = new List<StorageFile>();
    private List<StorageFile> droppedFilesAR = new List<StorageFile>();
    private List<StorageFile> droppedFilesOutlet = new List<StorageFile>();
    private static int indexOfComboBoxDataPeriod = -1;

    private const string DEFAULT_FOLDER = @"C:\ProgramData\FairbancData";

    private readonly ILogger _logger;

    private UploadProcessAsync? _uploadProcess;

    public ManualProcessViewModel ViewModel
    {
        get;
    }

    public ManualProcessPage()
    {
        ViewModel = App.GetService<ManualProcessViewModel>();
        InitializeComponent();
        ThemeHelper.ApplyTheme(this);

        // Overide theme helper function - which is reseting all color setup in page XAML to standart color
        btnProcess.Foreground = new SolidColorBrush(Colors.White);

        _logger = Log.Logger;

        cbitem.Add(DateTime.Now.AddMonths(-1).ToString("MMMM yyyy", indonesianCulture));
        cbitem.Add(DateTime.Now.AddMonths(-2).ToString("MMMM yyyy", indonesianCulture));
        cbitem.Add(DateTime.Now.AddMonths(-3).ToString("MMMM yyyy", indonesianCulture));

        if (PresistentFiles.hasValue())
        {
            if (PresistentFiles.droppedFilesSales.Count > 0)
            {
                droppedFilesSales = PresistentFiles.droppedFilesSales;
                this.UpdateMessageTextBlock(Drop01, droppedFilesSales.First().Name);
            }
            else { btnPreview01.Visibility = Visibility.Collapsed; }
            if (PresistentFiles.droppedFilesAR.Count > 0)
            {
                droppedFilesAR = PresistentFiles.droppedFilesAR;
                this.UpdateMessageTextBlock(Drop02, droppedFilesAR.First().Name);
            }
            else { btnPreview02.Visibility = Visibility.Collapsed; }
            if (PresistentFiles.droppedFilesOutlet.Count > 0)
            {
                droppedFilesOutlet = PresistentFiles.droppedFilesOutlet;
                this.UpdateMessageTextBlock(Drop03, droppedFilesOutlet.First().Name);
            }
            else { btnPreview03.Visibility = Visibility.Collapsed; }

        }

        if (indexOfComboBoxDataPeriod >= 0) { DataPeriod.SelectedIndex = indexOfComboBoxDataPeriod; }
        if (isWindows10())
        {
            if (btnPreview01.Visibility == Visibility.Collapsed)
            {
                btnPilihPenjualan.Visibility = Visibility.Visible;
                MessageTextBlock01.Text = "File Invoice Penjualan";
            }
            if (btnPreview02.Visibility == Visibility.Collapsed)
            {
                btnPilihPembayaran.Visibility = Visibility.Visible;
                MessageTextBlock02.Text = "File Penerimaan Pembayaran Invoice";
            }
            if (btnPreview01.Visibility == Visibility.Collapsed)
            {
                btnPilihOutlet.Visibility = Visibility.Visible;
                MessageTextBlock03.Text = "File Data Customer/Outlet";
            }

        }
    }

    protected async override void OnNavigatedTo(NavigationEventArgs e)
    {
        try
        {
            base.OnNavigatedTo(e);

            // Use the parameter
            if (e.Parameter is MyParameterType parameter)
            {
                if ((parameter.Property1 is null) || (parameter.Property2 is null))
                {
                    await CheckInitialFileExist();
                }
                else
                {
                    _ParameterType = parameter;
                    if (!await CheckInitialFileExist())
                    {
                        await WriteConfigToFileAsync();
                    }
                }
                UserGreetings01.Text = "Hai, " + _ParameterType.Property2;
            }
        }
        catch (Exception ex)
        {
            ContentDialog errorDialog = new ContentDialog
            {
                XamlRoot = this.XamlRoot,
                Title = "Informasi Kesalahan",
                CloseButtonText = "OK",
                DefaultButton = ContentDialogButton.Close,
                Content = $"Terjadi kesalahan.\nInfo kesalahan : {ex.Message}."
            };
        }
    }

    private void OnDragOver(object sender, DragEventArgs e)
    {
        e.AcceptedOperation = DataPackageOperation.Copy;
        e.DragUIOverride.IsCaptionVisible = false;
        e.DragUIOverride.IsGlyphVisible = false;
    }

    private async void OnDropSales(object sender, DragEventArgs e)
    {
        if (e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            var items = await e.DataView.GetStorageItemsAsync();
            foreach (var item in items)
            {
                if (item is StorageFile file && IsExcelFile(file))
                {
                    if (droppedFilesSales.Count > 0)
                    {
                        droppedFilesSales.RemoveAll(x => IsExcelFile(file));
                    }
                    droppedFilesSales.Add(file);
                    PresistentFiles.droppedFilesSales = droppedFilesSales;
                    UpdateMessageTextBlock(sender, file.Name);
                }
                else
                {
                    UpdateMessageTextBlock(sender, "Only Excel files (.xls, .xlsx, or .xlsm) are allowed.");
                }
            }
        }
    }

    private async void OnDropAR(object sender, DragEventArgs e)
    {
        if (e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            var items = await e.DataView.GetStorageItemsAsync();
            foreach (var item in items)
            {
                if (item is StorageFile file && IsExcelFile(file))
                {
                    if (droppedFilesAR.Count > 0)
                    {
                        droppedFilesAR.RemoveAll(x => IsExcelFile(file));
                    }
                    droppedFilesAR.Add(file);
                    PresistentFiles.droppedFilesAR = droppedFilesAR;
                    UpdateMessageTextBlock(sender, file.Name);
                }
                else
                {
                    UpdateMessageTextBlock(sender, "Only Excel files (.xls, .xlsx, or .xlsm) are allowed.");
                }
            }
        }
    }

    private async void OnDropOutlet(object sender, DragEventArgs e)
    {
        if (e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            var items = await e.DataView.GetStorageItemsAsync();
            foreach (var item in items)
            {
                if (item is StorageFile file && IsExcelFile(file))
                {
                    if (droppedFilesOutlet.Count > 0)
                    {
                        droppedFilesOutlet.RemoveAll(x => IsExcelFile(file));
                    }
                    droppedFilesOutlet.Add(file);
                    PresistentFiles.droppedFilesOutlet = droppedFilesOutlet;
                    UpdateMessageTextBlock(sender, file.Name);
                }
                else
                {
                    UpdateMessageTextBlock(sender, "Only Excel files (.xls, .xlsx, or .xlsm) are allowed.");
                }
            }
        }
    }

    private bool IsExcelFile(StorageFile file)
    {
        return file.FileType == ".xls" || file.FileType == ".xlsx" || file.FileType == ".xlsm";
    }

    private void UpdateMessageTextBlock(object sender, string message)
    {
        if (sender.GetType() == typeof(Grid))
        {
            var senderGrid = sender as Grid;
            var textBlock = FindDescendant<TextBlock>(senderGrid);
            if (textBlock != null)
            {
                textBlock.Text = message;
            }
            if (!message.StartsWith("Only"))
            {
                AddFileIcon(sender);
            }
        }
    }

    private T FindDescendant<T>(DependencyObject parent) where T : DependencyObject
    {
        int childCount = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < childCount; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T)
            {
                return (T)child;
            }
            var descendant = FindDescendant<T>(child);
            if (descendant != null)
            {
                return descendant;
            }
        }
        return null;
    }

    private void AddFileIcon(object sender)
    {
        var icon = new FontIcon
        {
            //Glyph = "\uE8A5", // This is the Unicode for a document icon
            Glyph = "\uf000",
            FontSize = 20,
            Margin = new Thickness(5),
            FontFamily = new FontFamily("Segoe MDL2 Assets"),
            Foreground = new SolidColorBrush(Colors.ForestGreen)
        };
        var senderGrid = sender as Grid;
        if (senderGrid.Name == "Drop01")
        {
            IconsPanel01.Children.Clear();
            IconsPanel01.Children.Add(icon);
            //btnRemove01.Visibility = Visibility.Visible;
            btnPreview01.Visibility = Visibility.Visible;
        }
        if (senderGrid.Name == "Drop02")
        {
            IconsPanel02.Children.Clear();
            IconsPanel02.Children.Add(icon);
            //btnRemove02.Visibility = Visibility.Visible;
            btnPreview02.Visibility = Visibility.Visible;
        }
        if (senderGrid.Name == "Drop03")
        {
            IconsPanel03.Children.Clear();
            IconsPanel03.Children.Add(icon);
            //btnRemove03.Visibility = Visibility.Visible;
            btnPreview03.Visibility = Visibility.Visible;
        }
    }

    private void btnBack_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        //Clear droppedFiles static class
        droppedFilesSales.Clear();
        droppedFilesAR.Clear();
        droppedFilesOutlet.Clear();
        indexOfComboBoxDataPeriod = -1;

        var navigationService = App.GetService<INavigationService>();
        navigationService.NavigateTo(typeof(MainMenuViewModel).FullName!, _ParameterType, true);
    }

    private void btnRemove_Click(object sender, RoutedEventArgs e)
    {
        var senderButton = sender as Button;
        if (senderButton.Name == "btnRemove01" || senderButton .Name == "btnPreview01") 
        {
            //btnRemove01.Visibility = Visibility.Collapsed;
            btnPreview01.Visibility = Visibility.Collapsed;
            IconsPanel01.Children.Clear();
            MessageTextBlock01.Text = "Drag dan drop file Invoice Penjualan (Excel) di sini !";
            droppedFilesSales.Clear();
            PresistentFiles.droppedFilesSales.Clear();
            if (isWindows10())
            {
                btnPilihPenjualan.Visibility = Visibility.Visible;
                MessageTextBlock01.Text = "File Invoice Penjualan";
            }
        }
        if (senderButton.Name == "btnRemove02" || senderButton.Name == "btnPreview02")
        {
            //btnRemove02.Visibility = Visibility.Collapsed;
            btnPreview02.Visibility = Visibility.Collapsed;
            IconsPanel02.Children.Clear();
            MessageTextBlock02.Text = "Drag dan drop file Pembayaran Invoice (Excel) di sini !";
            droppedFilesAR.Clear();
            PresistentFiles.droppedFilesAR.Clear();
            if (isWindows10())
            {
                btnPilihPembayaran.Visibility = Visibility.Visible;
                MessageTextBlock02.Text = "File Penerimaan Pembayaran Invoice";
            }
        }
        if (senderButton.Name == "btnRemove03" || senderButton.Name == "btnPreview03")
        {
            //btnRemove03.Visibility = Visibility.Collapsed;
            btnPreview03.Visibility = Visibility.Collapsed;
            IconsPanel03.Children.Clear();
            MessageTextBlock03.Text = "Drag dan drop file data Customer (Excel) di sini !";
            droppedFilesOutlet.Clear();
            PresistentFiles.droppedFilesOutlet.Clear();
            if (isWindows10())
            {
                btnPilihOutlet.Visibility = Visibility.Visible;
                MessageTextBlock03.Text = "File Data Customer/Outlet";
            }
        }
    }

    private void btnPreview_Click(object sender, RoutedEventArgs e)
    {
        btnRemove_Click(sender, e);
        return;

        var navigationService = App.GetService<INavigationService>();
        var senderButton = sender as Button;
        if (senderButton.Name == "btnPreview01")
        {
            if (CheckDropFileInFolder(droppedFilesSales))  navigationService.NavigateTo(typeof(FilePreviewViewModel).FullName!, droppedFilesSales, false);
        }
        if (senderButton.Name == "btnPreview02")
        {
            if (CheckDropFileInFolder(droppedFilesAR))  navigationService.NavigateTo(typeof(FilePreviewViewModel).FullName!, droppedFilesAR, false); 
        }
        if (senderButton.Name == "btnPreview03")
        {
            if (CheckDropFileInFolder(droppedFilesOutlet)) navigationService.NavigateTo(typeof(FilePreviewViewModel).FullName!, droppedFilesOutlet, false);
        }
    }

    private bool CheckDropFileInFolder(List<StorageFile> DroppedFiles)
    {
        if (DroppedFiles.Count > 0)
        {
            bool fileExists = (File.Exists(DroppedFiles.First().Path));

            if (!fileExists)
            {
                ContentDialog resultDialog = new ContentDialog
                {
                    XamlRoot = this.XamlRoot,
                    Title = "Info Kesalahan",
                    CloseButtonText = "OK",
                    DefaultButton = ContentDialogButton.Close,
                    Content = $"File excel\n{ DroppedFiles.First().DisplayName }\ntidak di temukan di dalam folder."
                };
                resultDialog.ShowAsync();
                return false;
            }
            return true;
        }
        else
        {
            // if there is no file to check, then its ok - return true
            return false;
        }
    }

    private async void btnProcess_Click_old(object sender, RoutedEventArgs e)
    {
        if (DataPeriod.SelectedIndex < 0 || droppedFilesSales.Count == 0)
        {
            return;
        }


        var progressBar = new ProgressBar
        {
            IsIndeterminate = true,
            //Height = 10,
            //Margin = new Thickness(10), 
            VerticalAlignment = VerticalAlignment.Center,
            //Width = 140

        };

        var progressBarBorder = new Microsoft.UI.Xaml.Controls.Border
        {
            BorderBrush = new SolidColorBrush(Colors.Blue),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(5),
            Background = new SolidColorBrush(Colors.GhostWhite),
            Child = progressBar
        };

        var waitingTextBlock = new TextBlock
        {
            Text = "Mohon menunggu...",
            FontSize = 20,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Colors.DodgerBlue), // Set the desired text color
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        OverlayGrid.Children.Add(waitingTextBlock);
        //OverlayGrid.Children.Add(progressBarBorder);

        OverlayGrid.BorderBrush = new SolidColorBrush(Colors.DodgerBlue);
        OverlayGrid.BorderThickness = new Thickness(1);
        OverlayGrid.CornerRadius = new CornerRadius(0);
        OverlayGrid.Background = new SolidColorBrush(Colors.GhostWhite);

        OverlayGrid.Visibility = Visibility.Visible;

        var timer = new DispatcherTimer();
        timer.Interval = TimeSpan.FromSeconds(7);

        timer.Start();
        timer.Tick += (s, args) =>
        {
            ContentArea.Children.Remove(progressBar);
            OverlayGrid.Visibility = Visibility.Collapsed;
            timer.Stop();
        };

    }

    private async void btnProcess_Click(object sender, RoutedEventArgs e)
    {
        if (DataPeriod.SelectedIndex < 0 )
        {
            ContentDialog resultDialog = new ContentDialog
            {
                XamlRoot = this.XamlRoot,
                Title = "Info Kesalahan",
                CloseButtonText = "OK",
                DefaultButton = ContentDialogButton.Close,
                Content = $"Periode penkinian data belum di pilih."
            };
            await resultDialog.ShowAsync();
            return;
        }

        if (droppedFilesSales.Count == 0)
        {
            ContentDialog resultDialog = new ContentDialog
            {
                XamlRoot = this.XamlRoot,
                Title = "Info Kesalahan",
                CloseButtonText = "OK",
                DefaultButton = ContentDialogButton.Close,
                Content = $"File Excel belum di pilih."
            };
            await resultDialog.ShowAsync();
            return;
        }

        //= > call CheckDropFileInFolder
        if (!CheckDropFileInFolder(droppedFilesSales)) return;
        if (!CheckDropFileInFolder(droppedFilesAR)) return;
        if (!CheckDropFileInFolder(droppedFilesOutlet)) return;

        var progressBar = new ProgressBar
        {
            IsIndeterminate = true,
            VerticalAlignment = VerticalAlignment.Center,
        };

        var waitingTextBlock = new TextBlock
        {
            Text = "Mohon menunggu...",
            FontSize = 20,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Colors.DodgerBlue),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        OverlayGrid.BorderBrush = new SolidColorBrush(Colors.IndianRed);
        OverlayGrid.BorderThickness = new Thickness(1);
        OverlayGrid.CornerRadius = new CornerRadius(0);
        OverlayGrid.Background = new SolidColorBrush(Colors.GhostWhite);
        OverlayGrid.Children.Add(waitingTextBlock);
        OverlayGrid.Visibility = Visibility.Visible;

        var timer = new DispatcherTimer();
        timer.Interval = TimeSpan.FromSeconds(7);

        timer.Start();

        // Call PerformUploadTask asynchronously
        bool uploadResult = await PerformUploadTask();

        timer.Stop();
        ContentArea.Children.Remove(progressBar);
        OverlayGrid.Visibility = Visibility.Collapsed;

        // Handle the upload result (e.g., show a message or take further action)
        if (uploadResult)
        {
            ContentDialog resultDialog = new ContentDialog
            {
                XamlRoot = this.XamlRoot,
                Title = "Informasi",
                CloseButtonText = "OK",
                DefaultButton = ContentDialogButton.Close,
                Content = $"Berhasil melakukan pengkinian data pada waktu {DateTimeOffset.Now}."
            };
            await resultDialog.ShowAsync();

            // On UI page, lock "Preview Buttons" and Upload "Process" button"
            btnProcess.IsEnabled = false;
            btnPreview01.IsEnabled = false;
            btnPreview02.IsEnabled = false;
            btnPreview03.IsEnabled = false;
        }
        else
        {
            ContentDialog resultDialog = new ContentDialog
            {
                XamlRoot = this.XamlRoot,
                Title = "Info Kesalahan",
                CloseButtonText = "OK",
                DefaultButton = ContentDialogButton.Close,
                Content = $"Gagal melakukan pengkinian data pada waktu: {DateTimeOffset.Now}."
            };
            await resultDialog.ShowAsync();
        }
    }

    public string DataFolder
    {
        get; private set;
    } = "";

    private async Task<bool> PerformUploadTask()
    {

        try
        {
            var AppWorkingFolder = DEFAULT_FOLDER;

            // Its default location for App Summission is Path.Combine(@"C:\ProgramData\FairbancData", "Datasharing-result")
            AppWorkingFolder = AppWorkingFolder + @"\Datasharing-result";


            //string SalesFile = "", RepaymentFile = "", OutletFile = "";
            string SalesFileABS = "", RepaymentFileABS = "", OutletFileABS = "";
            if (droppedFilesSales.Count == 0) { return false; }
            CheckandRefreshFolder(AppWorkingFolder);
            SalesFileABS = droppedFilesSales.First().Path;
            //SalesFile = droppedFilesSales.First().Name;
            //File.Copy(SalesFileABS, DataFolder + Path.DirectorySeparatorChar + SalesFile, true);
            if (droppedFilesAR.Count > 0)
            {
                RepaymentFileABS = droppedFilesAR.First().Path;
                //RepaymentFile = droppedFilesAR.First().Name;
                //File.Copy(RepaymentFileABS, DataFolder + Path.DirectorySeparatorChar + RepaymentFile, true);
            }
            if (droppedFilesAR.Count > 0)
            {
                OutletFileABS = droppedFilesOutlet.First().Path;
                //OutletFile = droppedFilesOutlet.First().Name;
                //File.Copy(OutletFileABS, DataFolder + Path.DirectorySeparatorChar + OutletFile, true);
            }

            string uploadPeriod = "yyyyMM";
            switch (indexOfComboBoxDataPeriod)
            {
                case 0:
                    uploadPeriod = DateTime.Now.AddMonths(-1).ToString("yyyyMM");
                    break;
                case 1:
                    uploadPeriod = DateTime.Now.AddMonths(-2).ToString("yyyyMM");
                    break;
                case 2:
                    uploadPeriod = DateTime.Now.AddMonths(-3).ToString("yyyyMM");
                    break;
                default:
                    uploadPeriod = DateTime.Now.AddMonths(0).ToString("yyyyMM");
                    break;
            }
            _uploadProcess = new UploadProcessAsync("N", "Y", SalesFileABS, RepaymentFileABS, OutletFileABS, "",
                _ParameterType.Property1, _ParameterType.Property2, AppWorkingFolder , _logger, uploadPeriod);

            _logger.Information(">> At {time} performing data upload by executing Data Sharing app at the specified time.", DateTimeOffset.Now);
            //_logger.LogInformation($">>>> [RESULT] File info value in sequence are {Date1} ,{Date2} ,{Date3} ,{Time} ,{Sales}, {Repayment}, {Outlet}, {DataFolder} {DTid} and {DistName} ...");

            if (_uploadProcess != null)
            {
                await _uploadProcess.ExecuteAsync();
                _logger.Information(">>>> [OUTPUT] UploadProcess execution completed.");

                return true;
            }
            else
            {
                _logger.Information("UploadProcess is not initialized. Cannot perform task.");
                return false;
            }

            //TODO: Done = > need to check result of logging after performing manual upload

        }
        catch (Exception )
        {
            ContentDialog errorDialog = new ContentDialog
            {
                XamlRoot = this.XamlRoot,
                Title = "Informasi Kesalahan",
                CloseButtonText = "OK",
                DefaultButton = ContentDialogButton.Close,
                Content = $"Gagal melakukan pengkinian data pada {DateTimeOffset.Now}."
            };
            await errorDialog.ShowAsync();
            return false;
        }
    }

    private void CheckandRefreshFolder(string location)
    {
        try
        {
            if (Directory.Exists(location))
            {
                return;
            }
            Directory.CreateDirectory(location);
        }
        catch (Exception)
        {
            throw;
        }
    }

    private void DataPeriod_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        indexOfComboBoxDataPeriod = DataPeriod.SelectedIndex;
    }

    private async Task<bool> CheckInitialFileExist()
    {
        try
        {
            var configFolder = @"C:\ProgramData\FairbancData";
            var filePath = "";
            filePath = Path.Combine(configFolder, "ManualUpload.ini");


            if (File.Exists(filePath))
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string line;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        if (line.Trim() == "[DTID]")
                        {
                            _ParameterType.Property1 = await reader.ReadLineAsync();
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
        catch (Exception)
        {
            return false;
        }
    }

    private void Drop01_DragEnter(object sender, DragEventArgs e)
    {

    }

    async Task<bool> WriteConfigToFileAsync()
    {
        try
        {
            var folder = @"C:\ProgramData\FairbancData";
            var filePath = Path.Combine(folder, "ManualUpload.ini");

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                await writer.WriteLineAsync("[FOLDER]");
                await writer.WriteLineAsync(@"C:\ProgramData\FairbancData");
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

    private async void OpenFileButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var filePicker = new FileOpenPicker();
            filePicker.FileTypeFilter.Add(".xlsx");
            filePicker.FileTypeFilter.Add(".xls");
            filePicker.FileTypeFilter.Add(".xlsm");
            filePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            filePicker.ViewMode = PickerViewMode.Thumbnail;

            var wind = App.MainWindow;
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(wind);
            WinRT.Interop.InitializeWithWindow.Initialize(filePicker, hwnd);

            var file = await filePicker.PickSingleFileAsync();
            if (file != null)
            {

                // Do something with the file stream depending on caller button
                if ((IsExcelFile(file)) && (sender is Button tb))
                {
                    switch (tb.Name)
                    {
                        case "btnPilihPenjualan":
                            if (droppedFilesSales.Count > 0)
                            {
                                droppedFilesSales.RemoveAll(x => IsExcelFile(file));
                            }
                            droppedFilesSales.Add(file);
                            PresistentFiles.droppedFilesSales = droppedFilesSales;
                            UpdateMessageTextBlock(Drop01, file.Name);
                            break;
                        case "btnPilihPembayaran":
                            if (droppedFilesAR.Count > 0)
                            {
                                droppedFilesAR.RemoveAll(x => IsExcelFile(file));
                            }
                            droppedFilesAR.Add(file);
                            PresistentFiles.droppedFilesAR = droppedFilesAR;
                            UpdateMessageTextBlock(Drop02, file.Name);
                            break;
                        case "btnPilihOutlet":
                            if (droppedFilesOutlet.Count > 0)
                            {
                                droppedFilesOutlet.RemoveAll(x => IsExcelFile(file));
                            }
                            droppedFilesOutlet.Add(file);
                            PresistentFiles.droppedFilesOutlet = droppedFilesOutlet;
                            UpdateMessageTextBlock(Drop03, file.Name);

                            break;
                        default:
                            break;
                    }
                    tb.Visibility = Visibility.Collapsed;

                }
                else
                {
                    UpdateMessageTextBlock(sender, "Hanya file Excel (.xls, .xlsx, or .xlsm) yang diperbolehkan.");
                }
            }
        }
        catch (Exception)
        {
            ContentDialog errorDialog = new ContentDialog
            {
                XamlRoot = this.XamlRoot,
                Title = "Informasi Kesalahan",
                CloseButtonText = "OK",
                DefaultButton = ContentDialogButton.Close,
                Content = $"Gagal mengexplor data file excel."
            };
            await errorDialog.ShowAsync();
        }   
    }

    private bool isWindows10()
    {
        try
        {
            OperatingSystem os = Environment.OSVersion;
            Version version = os.Version;

            if (os.Platform == PlatformID.Win32NT && version.Major == 10)
            {
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

    private async void Button_Click_1(object sender, RoutedEventArgs e)
    {
        try
        {
            ContentDialog dialog = new ContentDialog();

            // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
            dialog.XamlRoot = this.XamlRoot;
            dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            dialog.Title = "Konfirmasi";
            dialog.Content = "Apakah anda ingin membuka layar 'Data Upload Otomatis' ?";
            dialog.PrimaryButtonText = "Ya";
            dialog.CloseButtonText = "Tidak";
            dialog.DefaultButton = ContentDialogButton.Secondary;

            var result = await dialog.ShowAsync();

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

    private void Button_Click_2(object sender, RoutedEventArgs e)
    {
        // do nothing
    }

    private async void Button_Click_3(object sender, RoutedEventArgs e)
    {
        try
        {
            ContentDialog dialog = new ContentDialog();

            // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
            dialog.XamlRoot = this.XamlRoot;
            dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            dialog.Title = "Konfirmasi";
            dialog.Content = "Apakah anda ingin membuka layar 'Catatan sejarah kegiatan applikasi (Log)' ?";
            dialog.PrimaryButtonText = "Ya";
            dialog.CloseButtonText = "Tidak";
            dialog.DefaultButton = ContentDialogButton.Secondary;

            var result = await dialog.ShowAsync();

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
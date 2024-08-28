using CommunityToolkit.WinUI.UI.Controls;
using FTIDataSharingUI.ViewModels;
using FTIDataSharingUI.Models;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Collections.Generic;
using System.Data;
using Windows.Storage;
using FTIDataSharingUI.Contracts.Services;

namespace FTIDataSharingUI.Views
{
    public sealed partial class FilePreviewPage : Page
    {
        // Define a property to hold the dropped files
        public List<StorageFile> DroppedFiles
        {
            get; set;
        }
        public FilePreviewViewModel ViewModel
        {
            get;
        }

        private MyParameterType _ParameterType = new MyParameterType();

        public FilePreviewPage()
        {
            ViewModel = App.GetService<FilePreviewViewModel>();
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Retrieve the passed data (e.Parameter) and cast it to the correct type
            if (e.Parameter is List<StorageFile> files)
            {
                // Store the dropped files in your property
                DroppedFiles = files;

                // Now you can use the DroppedFiles list to read and display the Excel data
                LoadExcelData(LicenseContext.NonCommercial);
            }
        }

        private void LoadExcelData(LicenseContext licenseContext)
        {
            try
            {
                if (DroppedFiles.Count > 0)
                {
                    using (var package = new ExcelPackage(new FileInfo(DroppedFiles[0].Path)))
                    {
                        ExcelPackage.LicenseContext = licenseContext;
                        var worksheet = package.Workbook.Worksheets[0]; 

                        // Read data from the worksheet
                        var data = new List<MyDataItem>();
                        for (int row = 1; row <= worksheet.Dimension.Rows; row++)
                        {
                            var item = new MyDataItem();
                            item.Coloum1 = worksheet.Cells[row, 1].Text;
                            item.Coloum2 = worksheet.Cells[row, 2].Text;
                            item.Coloum3 = worksheet.Cells[row, 3].Text;
                            item.Coloum4 = worksheet.Cells[row, 4].Text;
                            item.Coloum5 = worksheet.Cells[row, 5].Text;
                            data.Add(item);
                        }

                        // Set the DataGrid's ItemsSource 
                        ContentGridView.ItemsSource = data;
                    }
                }
            }
            catch (Exception)
            {
                ContentDialog errorDialog = new ContentDialog
                {
                    XamlRoot = this.XamlRoot,
                    Title = "Info",
                    CloseButtonText = "OK",
                    DefaultButton = ContentDialogButton.Close,
                    Content = $"Error - Gagal membaca file excel di folder {DroppedFiles[0].Path}."
                };
                errorDialog.ShowAsync();
            }
        }

        private void btnBack_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            var navigationService = App.GetService<INavigationService>();
            checkDTName();
            navigationService.NavigateTo(typeof(ManualProcessViewModel).FullName!, _ParameterType , true);
        }

        private  bool checkDTName()
        {
            try
            {
                var configFolder = @"C:\ProgramData\FairbancData";
                var filePath = Path.Combine(configFolder, "DateTimeInfo.ini");
                if (File.Exists(filePath))
                {
                    using (StreamReader reader = new StreamReader(filePath))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (line.Trim() == "[DTID]")
                            {
                                _ParameterType.Property1 = reader.ReadLine();
                            }
                            if (line.Trim() == "[DTNAME]")
                            {
                                _ParameterType.Property2 = reader.ReadLine();
                            }
                        }
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
    }


}

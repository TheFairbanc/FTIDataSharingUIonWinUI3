using System;
using System.IO;
using System.Data;
using System.Linq;
using Windows.Storage;
using System.Collections.Generic;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using DataSubmission.Models;
using DataSubmission.ViewModels;
using DataSubmission.Contracts.Services;
using DataSubmission.Views;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

using CommunityToolkit.WinUI.UI.Controls;
using Page = Microsoft.UI.Xaml.Controls.Page;
using CommunityToolkit.Mvvm.DependencyInjection;


namespace FTIDataSharingUI.Views
{
    public sealed partial class FilePrevPage : Page
    {
        private MyParameterType _ParameterType = new MyParameterType();

        public FilePrevViewModel ViewModel { get; }

        public FilePrevPage()
        {
            ViewModel = App.GetService<FilePrevViewModel>();
            InitializeComponent();
        }

        // Define a property to hold the dropped files
        public List<StorageFile> DroppedFiles { get; set; }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Retrieve the passed data (e.Parameter) and cast it to the correct type
            if (e.Parameter is List<StorageFile> files)
            {
                // Store the dropped files in your property
                DroppedFiles = files;

                ReadExcelFileDOM(DroppedFiles[0].Path);
            }
        }

        private async void ReadExcelFileDOM(string fileNamePath)
        {
            try
            {
                if (DroppedFiles.Count > 0)
                {
                    var dataTable = ExcelHelper.ReadExcelSheet(fileNamePath);
                    ContentGridView.ItemsSource = dataTable.DefaultView;
                }
                else
                {
                    // Its not possible to preview no selected/dropped excel
                    ContentDialog errorDialog = new ContentDialog
                    {
                        XamlRoot = this.XamlRoot,
                        Title = "Info Kesalahan",
                        CloseButtonText = "OK",
                        DefaultButton = ContentDialogButton.Close,
                        Content = $"Gagal membaca file excel di folder {DroppedFiles[0].Path}."
                    };
                    await errorDialog.ShowAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception: {ex.Message}");
                ContentDialog errorDialog = new ContentDialog
                {
                    XamlRoot = this.XamlRoot,
                    Title = "Info Kesalahan",
                    CloseButtonText = "OK",
                    DefaultButton = ContentDialogButton.Close,
                    Content = $"Terjadi kesalahan pembacaan data Excel\n Info kesalahan: {ex.Message}."
                };
                await errorDialog.ShowAsync();
            }
        }

        private string GetCellValue(SpreadsheetDocument document, Cell cell)
        {
            SharedStringTablePart sstpart = document.WorkbookPart.SharedStringTablePart;
            string value = cell.CellValue?.InnerText ?? ".";

            // Check the data type of the cell
            if (cell.DataType != null)
            {
                string dataType = cell.DataType.Value.ToString();
                switch (dataType)
                {
                    case "s":
                        if (sstpart != null && int.TryParse(value, out int index))
                        {
                            return sstpart.SharedStringTable.ChildElements[index].InnerText;
                        }
                        break;

                    case "b":
                        return value == "1" ? "TRUE" : "FALSE";

                    case "n":
                    case "str":
                    case "inlineStr":
                        return value;

                    default:
                        return value;
                }
            }

            return value;
        }




        private void btnBack_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            var navigationService = App.GetService<INavigationService>();
            checkDTName();
            navigationService.NavigateTo(typeof(ManualProcessViewModel).FullName!, _ParameterType, false);
        }

        private bool checkDTName()
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

    public static class ExcelHelper
    {
        public static DataTable ReadExcelSheet(string fname, bool firstRowIsHeader = true)
        {
            List<string> Headers = new List<string>();
            var dt = new DataTable();
            using (SpreadsheetDocument doc = SpreadsheetDocument.Open(fname, false))
            {
                //Read the first Sheets 
                Sheet sheet = doc.WorkbookPart.Workbook.Sheets.GetFirstChild<Sheet>();
                Worksheet worksheet = (doc.WorkbookPart.GetPartById(sheet.Id.Value) as WorksheetPart).Worksheet;
                IEnumerable<Row> rows = worksheet.GetFirstChild<SheetData>().Descendants<Row>();
                int counter = 0;
                int seccounter = 0;
                foreach (Row row in rows)
                {
                    counter = counter + 1;
                    //Read the first row as header
                    if (counter > 10) { break; }
                    if (counter == 1)
                    {
                        var j = 1;
                        foreach (Cell cell in row.Descendants<Cell>())
                        {
                            if (seccounter > 10) { break;}

                            var colunmName = firstRowIsHeader ? GetCellValue(doc, cell) : "Field" + j++;
                            Console.WriteLine(colunmName);
                            Headers.Add(colunmName);
                            dt.Columns.Add(colunmName);

                            seccounter ++;
                        }
                    }
                    else
                    {
                        dt.Rows.Add();
                        int i = 0;
                        foreach (Cell cell in row.Descendants<Cell>())
                        {
                            if (seccounter > 10) { break; }

                            dt.Rows[dt.Rows.Count - 1][i] = GetCellValue(doc, cell);
                            i++;

                            seccounter++;
                        }
                    }
                }
            }
            dt.Rows.RemoveAt(0);
            return dt;
        }

        public static string GetCellValue(SpreadsheetDocument document, Cell cell)
        {
            SharedStringTablePart stringTablePart = document.WorkbookPart.SharedStringTablePart;
            string value = cell.CellValue?.InnerXml ?? "";

            if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
            {
                return stringTablePart.SharedStringTable.ChildElements[Int32.Parse(value)].InnerText;
            }
            else
            {

                return value;
            }
        }
    }
}

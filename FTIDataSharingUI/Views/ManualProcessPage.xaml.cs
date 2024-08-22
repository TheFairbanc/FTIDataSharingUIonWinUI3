using System.Collections.ObjectModel;
using FTIDataSharingUI.Contracts.Services;
using FTIDataSharingUI.Models;
using FTIDataSharingUI.ViewModels;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.ApplicationModel.Appointments.AppointmentsProvider;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using WinUIEx;

namespace FTIDataSharingUI.Views;

public sealed partial class ManualProcessPage : Page
{
    private MyParameterType _ParameterType = new();

    private ObservableCollection<string> cbitem = new ObservableCollection<string>();

    public ManualProcessViewModel ViewModel
    {
        get;
    }

    public ManualProcessPage()
    {
        ViewModel = App.GetService<ManualProcessViewModel>();
        InitializeComponent();
        cbitem.Add(DateTime.Now.AddMonths(-1).ToString("MMMM yyyy"));
        cbitem.Add(DateTime.Now.AddMonths(-2).ToString("MMMM yyyy"));
        cbitem.Add(DateTime.Now.AddMonths(-3).ToString("MMMM yyyy"));
    }

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

    //private const int MaxFiles = 1;
    private List<StorageFile> droppedFilesSales = new List<StorageFile>();
    private List<StorageFile> droppedFilesAR = new List<StorageFile>();
    private List<StorageFile> droppedFilesOutlet = new List<StorageFile>();

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
                    AddFileIcon(sender);
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
                    AddFileIcon(sender);
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
                    AddFileIcon(sender);
                    UpdateMessageTextBlock(sender, file.Name);
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
            Glyph = "\uE8A5", // This is the Unicode for a document icon
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
            btnRemove01.Visibility = Visibility.Visible;
        }
        if (senderGrid.Name == "Drop02")
        {
            IconsPanel02.Children.Clear();
            IconsPanel02.Children.Add(icon);
            btnRemove02.Visibility = Visibility.Visible;
        }
        if (senderGrid.Name == "Drop03")
        {
            IconsPanel03.Children.Clear();
            IconsPanel03.Children.Add(icon);
            btnRemove03.Visibility = Visibility.Visible;
        }
    }

    private void btnBack_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var navigationService = App.GetService<INavigationService>();
        navigationService.NavigateTo(typeof(MainMenuViewModel).FullName!, _ParameterType, true);
    }

    private void btnRemove_Click(object sender, RoutedEventArgs e)
    {
        var senderButton = sender as Button;
        if (senderButton.Name == "btnRemove01")
        {
            btnRemove01.Visibility = Visibility.Collapsed;
            IconsPanel01.Children.Clear();
            MessageTextBlock01.Text = "Drag dan drop file Invoice Penjualan (Excel) di sini !";
        }
        if (senderButton.Name == "btnRemove02")
        {
            btnRemove02.Visibility = Visibility.Collapsed;
            IconsPanel02.Children.Clear();
            MessageTextBlock02.Text = "Drag dan drop file Pembayaran Invoice (Excel) di sini !";
        }
        if (senderButton.Name == "btnRemove03")
        {
            btnRemove03.Visibility = Visibility.Collapsed;
            IconsPanel03.Children.Clear();
            MessageTextBlock03.Text = "Drag dan drop file data Customer (Excel) di sini !";
        }
    }
}


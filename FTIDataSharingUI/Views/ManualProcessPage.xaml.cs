using FTIDataSharingUI.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;

namespace FTIDataSharingUI.Views;

public sealed partial class ManualProcessPage : Page
{
    public ManualProcessViewModel ViewModel
    {
        get;
    }

    public ManualProcessPage()
    {
        ViewModel = App.GetService<ManualProcessViewModel>();
        InitializeComponent();
    }

    private const int MaxFiles = 3;
    private List<StorageFile> droppedFiles = new List<StorageFile>();

    private void OnDragOver(object sender, DragEventArgs e)
    {
        e.AcceptedOperation = DataPackageOperation.Copy;
        e.DragUIOverride.IsCaptionVisible = false;
        e.DragUIOverride.IsGlyphVisible = false;
    }

    private async void OnDrop(object sender, DragEventArgs e)
    {
        if (e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            var items = await e.DataView.GetStorageItemsAsync();
            foreach (var item in items)
            {
                if (item is StorageFile file && (file.FileType == ".xls" || file.FileType == ".xlsx"))
                {
                    if (droppedFiles.Count < MaxFiles)
                    {
                        droppedFiles.Add(file);
                        AddFileIcon();
                        MessageTextBlock.Text = $"File '{file.Name}' added. Total files: {droppedFiles.Count}";
                    }
                    else
                    {
                        MessageTextBlock.Text = "Maximum of 3 Picture (PNG) files can be dropped.";
                    }
                }
                else
                {
                    MessageTextBlock.Text = "Only Excel files (.png) are allowed.";
                }
            }
        }
    }

    private void AddFileIcon()
    {
        var icon = new FontIcon
        {
            Glyph = "\uE8A5", // This is the Unicode for a document icon
            FontSize = 24,
            Margin = new Thickness(5)
        };
        IconsPanel.Children.Add(icon);
    }
}


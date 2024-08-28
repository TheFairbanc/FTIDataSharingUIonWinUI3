using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace FTIDataSharingUI.Models;
public static class PresistentFiles 
{
    public static List<StorageFile>? droppedFilesSales { get; set; } = new List<StorageFile>();
    public static List<StorageFile>? droppedFilesAR { get; set; } = new List<StorageFile>();
    public static List<StorageFile>? droppedFilesOutlet { get; set; } = new List<StorageFile>();

    public static Boolean hasValue()
    {
        if (droppedFilesAR.Count > 0 || droppedFilesOutlet.Count > 0 || droppedFilesSales.Count > 0) 
        { 
            return true; 
        }
        else
        {
            return false;
        }
    }
}

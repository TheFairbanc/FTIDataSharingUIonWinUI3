using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace DataSubmission.Models;
// Models/LogEntry.cs
public class LogEntry
{
    public string Time
    {
        get; set;
    }
    public string Date
    {
        get; set;
    }
    public string Process
    {
        get; set;
    } = "";
    public string Warning 
    {
        get; set;
    } = "";
    public SolidColorBrush Color
    {
        get; set;
    }
}


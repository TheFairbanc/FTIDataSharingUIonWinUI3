using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml;
using Microsoft.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.UI.Xaml.Navigation;
using Windows.UI.ViewManagement;
using System.Diagnostics;
using Microsoft.UI;

namespace DataSubmission.Views
{
    public static class ThemeHelper
    {
        public static void ApplyTheme(FrameworkElement element)
        {
            var oriColor = (Color)element.Resources["SystemAccentColor"];
            var textColor = GetContrastingColor((Color)element.Resources["SystemAccentColor"]);
            UpdateTheme(element, textColor, oriColor);
        }

        private static void UpdateTheme(FrameworkElement rootElement, Color textColor, Color themeColor)
        {
            UpdateTextBlocks(rootElement, textColor, themeColor);
        }

        private static void UpdateTextBlocks(DependencyObject parent, Color textColor, Color themeColor)
        {
            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is TextBlock textBlock)
                {
                    //
                    var clrs = (SolidColorBrush)textBlock.Foreground;
                    if (clrs.Color == new SolidColorBrush(Microsoft.UI.Colors.White).Color)
                    {
                        Debug.WriteLine(textBlock.Name.ToString());
                        break;  }
                    if (GetTheme() == "Black")
                    {
                        textBlock.Foreground = new SolidColorBrush(Microsoft.UI.Colors.White);
                    }else
                    {
                        textBlock.Foreground = new SolidColorBrush(Microsoft.UI.Colors.DarkSlateGray);
                    }

                }
                else if (child is DependencyObject)
                {
                    UpdateTextBlocks(child, textColor, themeColor);
                }
            }
        }

        private static Color GetContrastingColor(Color color)
        {
            return (color.R + color.G + color.B) / 3 > 128 ? Colors.White : Colors.Black;
        }

        private static string GetTheme() 
        {
            var uiSettings = new UISettings();
            var accentColor = uiSettings.GetColorValue(UIColorType.Accent);
            var backgroundColor = uiSettings.GetColorValue(UIColorType.Background);
            bool isDarkTheme = backgroundColor.R < 128 && backgroundColor.G < 128 && backgroundColor.B < 128;

            if (isDarkTheme)
            {
                return "Dark";
            }
            else
            {
                return "Light";
            }

        }
    }


}

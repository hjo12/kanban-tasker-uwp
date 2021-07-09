using KanbanTasker.Models;
using System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace KanbanTasker.Converters.Text
{
    public class StringToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            
            string colorKey = value?.ToString() ?? "Medium";
            RevealBorderBrush revealBorderBrush = null;
            if (colorKey.Equals("Normal")) colorKey = "Medium";

            switch (colorKey)
            {
                case "Medium":
                    // Medium Priority - Reveal Brush
                    revealBorderBrush = new RevealBorderBrush
                    {
                        Color = Colors.Orange,
                        FallbackColor = Colors.Orange,
                        Opacity = 0.8,
                        TargetTheme = ApplicationTheme.Light
                    };

                    return revealBorderBrush;

                case "Low":
                    // Low Priority - Reveal Brush
                    revealBorderBrush = new RevealBorderBrush
                    {
                        Color = Colors.Green,
                        FallbackColor = Colors.Green,
                        Opacity = 0.8,
                        TargetTheme = ApplicationTheme.Light
                    };
                    return revealBorderBrush;

                case "High":
                    // High Priority - Reveal Brush
                    revealBorderBrush = new RevealBorderBrush
                    {
                        Color = Colors.Red,
                        FallbackColor = Colors.Red,
                        Opacity = 0.8,
                        TargetTheme = ApplicationTheme.Light
                    };
                    return revealBorderBrush;

                default:
                    return new SolidColorBrush(Colors.Transparent);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }
}
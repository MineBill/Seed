using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace Seed.Converters;

public class NullToDefaultImageConverter : IValueConverter
{
    public string DefaultImagePath { get; set; }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value ?? new Bitmap(AssetLoader.Open(new Uri((string)parameter)));
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
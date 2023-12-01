using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace Seed.Controls;

public class UserPreference : ContentControl
{
    public static readonly StyledProperty<string> OptionNameProperty = AvaloniaProperty.Register<UserPreference, string>(
        nameof(OptionName));

    public string OptionName
    {
        get => GetValue(OptionNameProperty);
        set => SetValue(OptionNameProperty, value);
    }
}
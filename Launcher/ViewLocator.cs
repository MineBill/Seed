using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Launcher.ViewModels;

namespace Launcher;

public class ViewLocator : IDataTemplate
{
    private static readonly Dictionary<Type, Func<Control>> Registration = new();

    public static void Register<TViewModel, TView>()
        where TView : Control, new()
    {
        Registration.Add(typeof(TViewModel), () => new TView());
    }

    public static void Register<TViewModel, TView>(Func<TView> factory)
        where TView : Control
    {
        Registration.Add(typeof(TViewModel), factory);
    }

    public Control Build(object? data)
    {
        if (data is null) return new TextBlock { Text = "`data` parameter was null" };

        var type = data.GetType();
        return Registration.TryGetValue(type, out var factory)
            ? factory()
            : new TextBlock { Text = "View Not Found: " + type };
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}
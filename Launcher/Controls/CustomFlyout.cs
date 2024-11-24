using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using NLog;

namespace Launcher.Controls;

public class CustomFlyout : Flyout
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private List<Button> mRegisteredButtons = [];

    protected override void OnOpened()
    {
        if (Content is Control c)
        {
            foreach (var button in c.GetVisualDescendants().OfType<Button>())
            {
                button.Click += OnClick;
                mRegisteredButtons.Add(button);
            }
        }

        base.OnOpened();
    }

    protected override void OnClosed()
    {
        mRegisteredButtons.ForEach(x => x.Click -= OnClick);
        base.OnClosed();
    }

    private async void OnClick(object? sender, RoutedEventArgs eventArgs)
    {
        try
        {
            await Task.Delay(100); // Update ViewModel（wait）
            Hide();
        }
        catch (Exception e)
        {
            Logger.Error(e);
        }
    }
}
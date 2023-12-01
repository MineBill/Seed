using System;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using ReactiveUI;
using Seed.ViewModels;

namespace Seed.Views;

public partial class EngineView : ReactiveUserControl<EngineViewModel>
{
    public EngineView()
    {
        InitializeComponent();
        this.WhenActivated(action => action(ViewModel!.OpenEditor.RegisterHandler(OpenEditorHandler)));
    }

    private async Task OpenEditorHandler(InteractionContext<EngineEditorViewModel, Unit> obj)
    {
        var editor = new EngineEditorView
        {
            DataContext = obj.Input
        };

        var ret = await editor.ShowDialog<Unit>(App.Current.MainWindow);
        obj.SetOutput(ret);
    }

    private void InputElement_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        ViewModel?.OnLostFocus(e);
    }

    private void InputElement_OnKeyDown(object? sender, KeyEventArgs e)
    {
        ViewModel?.OnKeyDown(e);
    }
}
using System;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.ReactiveUI;
using ReactiveUI;
using Seed.Models;
using Seed.ViewModels;
using Seed.Views.Dialogs;

namespace Seed.Views;

public partial class ProjectView : ReactiveUserControl<ProjectViewModel>
{
    public ProjectView()
    {
        InitializeComponent();
        this.WhenActivated(action =>
            action(ViewModel!.OpenCommandLineOptionsEditor.RegisterHandler(OpenCommandLineOptionsEditorHandler)));
        this.WhenActivated(action =>
            action(ViewModel!.OpenEnginePicker.RegisterHandler(OpenEnginePickerHandler)));
    }

    private async Task OpenCommandLineOptionsEditorHandler(InteractionContext<CommandLineOptionsViewModel, string> obj)
    {
        var editor = new CommandLineOptionsDialog()
        {
            DataContext = obj.Input
        };

        var ret = await editor.ShowDialog<string>(App.Current.MainWindow);
        obj.SetOutput(ret);
    }

    private async Task OpenEnginePickerHandler(InteractionContext<EngineVersionPickerViewModel, EngineVersion> obj)
    {
        var window = new EngineVersionPickerWindow()
        {
            DataContext = obj.Input
        };

        var ret = await window.ShowDialog<EngineVersion>(App.Current.MainWindow);
        obj.SetOutput(ret);
    }

    private void InputElement_OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is ProjectViewModel viewModel)
        {
            viewModel.OnDoubleTapped(sender, e);
        }
    }
}
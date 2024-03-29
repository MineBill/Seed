using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Microsoft.Extensions.DependencyInjection;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using ReactiveUI;
using Seed.Models;
using Seed.Services;

namespace Seed.ViewModels;

public class EngineViewModel : ViewModelBase
{
    private readonly Engine _engine;

    private string _name = string.Empty;

    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    public string Path => _engine.Path;
    public EngineVersion Version => _engine.Version;
    public static TextTrimming TrimmingMode => TextTrimming.PrefixCharacterEllipsis;

    private bool _isEditing;

    public bool IsEditing
    {
        get => _isEditing;
        set => this.RaiseAndSetIfChanged(ref _isEditing, value);
    }

    public ICommand DeleteCommand { get; }
    public ICommand EditNameCommand { get; }
    public ICommand OpenEditorCommand { get; }
    public ICommand OpenEngineFolderCommand { get; }

    public Interaction<EngineEditorViewModel, Unit> OpenEditor = new();

    public EngineViewModel(IEngineManager engineManager, Engine engine)
    {
        _engine = engine;
        Name = _engine.Name;
        DeleteCommand = ReactiveCommand.Create(async () =>
        {
            var confirmationBox = MessageBoxManager.GetMessageBoxStandard("Confirm", $"Are you sure you want to delete this engine: '{_engine.Name}' ?",
                ButtonEnum.YesNo, Icon.Stop);
            var result = await confirmationBox.ShowWindowDialogAsync(App.Current.MainWindow);

            if (result == ButtonResult.Yes)
                engineManager.DeleteEngine(_engine);
        });
        EditNameCommand = ReactiveCommand.Create(() => { IsEditing = true; });
        OpenEditorCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var engineEditorViewModel = new EngineEditorViewModel(_engine);
            await OpenEditor.Handle(engineEditorViewModel);
            Name = engine.Name;
            this.RaisePropertyChanged(nameof(Version));
            // TODO: Do the saving here?
            engineManager.Save();
        });

        OpenEngineFolderCommand = ReactiveCommand.Create(() =>
        {
            var filesService = App.Current.Services.GetService<IFilesService>();
            filesService?.OpenFolder(_engine.Path);
        });
    }

    public EngineViewModel()
    {
        _engine = new Engine
        {
            Name = "1.7",
            Path = "/home/minebill/.local/share/Seed/Installs/Flax_1.7",
            Version = new NormalVersion(new Version(1, 7, 6043, 0))
        };
        Name = _engine.Name;
        DeleteCommand = ReactiveCommand.Create(() => { });
        EditNameCommand = ReactiveCommand.Create(() => { IsEditing = true; });
    }

    public void OnLostFocus(RoutedEventArgs e)
    {
        SaveName();
    }

    public void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            SaveName();
        }
    }

    private void SaveName()
    {
        IsEditing = false;
        _engine.Name = Name;
        var engineManger = App.Current.Services.GetService<IEngineManager>()!;
        engineManger.Save();
    }
}
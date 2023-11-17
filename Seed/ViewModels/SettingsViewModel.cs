using System;
using System.ComponentModel;
using System.Reactive;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Media;
using DynamicData.Binding;
using Microsoft.Extensions.Logging;
using NLog;
using ReactiveUI;
using Seed.Models;
using Seed.Services;

namespace Seed.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private readonly IPreferencesSaver _preferencesSaver;

    private string? _newProjectLocation;

    public string? NewProjectLocation
    {
        get => _newProjectLocation;
        set
        {
            _preferencesSaver.Preferences.NewProjectLocation = value;
            this.RaiseAndSetIfChanged(ref _newProjectLocation, value);
        }
    }

    private string? _engineInstallLocation;

    public string? EngineInstallLocation
    {
        get => _engineInstallLocation;
        set
        {
            _preferencesSaver.Preferences.EngineInstallLocation = value;
            this.RaiseAndSetIfChanged(ref _engineInstallLocation, value);
        }
    }

    public ICommand SelectEngineFolderCommand { get; }
    public ICommand SelectNewProjectFolderCommand { get; }

    public SettingsViewModel(IPreferencesSaver preferencesSaver, IFilesService filesService)
    {
        _preferencesSaver = preferencesSaver;
        PropertyChanged += OnPropertyChanged;

        SelectEngineFolderCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var folder = await filesService.SelectFolderAsync();
            EngineInstallLocation = folder?.Path.AbsolutePath ?? null;
            _preferencesSaver.Save();
        });

        SelectNewProjectFolderCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var folder = await filesService.SelectFolderAsync();
            NewProjectLocation = folder?.Path.AbsolutePath ?? null;
            _preferencesSaver.Save();
        });

        _engineInstallLocation = _preferencesSaver.Preferences.EngineInstallLocation;
        _newProjectLocation = _preferencesSaver.Preferences.NewProjectLocation;
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        Logger.Info($"{e.PropertyName} setting was changed.");
    }
}
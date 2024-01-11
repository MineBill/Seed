using System;
using System.ComponentModel;
using System.Reactive;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
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

    private string _selectedThemeVariant;

    public string SelectedThemeVariant
    {
        get => _selectedThemeVariant;
        set
        {
            _preferencesSaver.Preferences.ColorTheme = value;
            this.RaiseAndSetIfChanged(ref _selectedThemeVariant, value);
        }
    }

    private Color? _accentColor;

    public Color? AccentColor
    {
        get => _accentColor;
        set
        {
            _preferencesSaver.Preferences.AccentColor = value;
            this.RaiseAndSetIfChanged(ref _accentColor, value);
        }
    }

    public bool AutoVariant => SelectedThemeVariant == "Auto";
    public bool DarkVariant => SelectedThemeVariant == "Dark";
    public bool LightVariant => SelectedThemeVariant == "Light";

    public ICommand SelectEngineFolderCommand { get; }
    public ICommand SelectNewProjectFolderCommand { get; }
    public ReactiveCommand<string, Unit> ChangeThemeVariant { get; }

    public SettingsViewModel(IPreferencesSaver preferencesSaver, IFilesService filesService)
    {
        _preferencesSaver = preferencesSaver;
        PropertyChanged += OnPropertyChanged;

        SelectEngineFolderCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var folder = await filesService.SelectFolderAsync();
            EngineInstallLocation = folder?.Path.LocalPath ?? null;
            _preferencesSaver.Save();
        });

        SelectNewProjectFolderCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var folder = await filesService.SelectFolderAsync();
            NewProjectLocation = folder?.Path.LocalPath ?? null;
            _preferencesSaver.Save();
        });

        ChangeThemeVariant = ReactiveCommand.Create((string variant) =>
        {
            Application.Current!.RequestedThemeVariant = variant switch
            {
                "Auto" => ThemeVariant.Default,
                "Dark" => ThemeVariant.Dark,
                "Light" => ThemeVariant.Light,
            };
            SelectedThemeVariant = variant;

            // Application.Current!.RequestedThemeVariant = ThemeVariant;
            _preferencesSaver.Save();
        });

        _engineInstallLocation = _preferencesSaver.Preferences.EngineInstallLocation;
        _newProjectLocation = _preferencesSaver.Preferences.NewProjectLocation;
        _selectedThemeVariant = _preferencesSaver.Preferences.ColorTheme;
        _accentColor = _preferencesSaver.Preferences.AccentColor;
    }

    public void SetAccentColor(Color color)
    {
        AccentColor = color;
        Application.Current!.Resources["SystemAccentColor"] = color;
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        Logger.Info($"{e.PropertyName} setting was changed.");
    }
}
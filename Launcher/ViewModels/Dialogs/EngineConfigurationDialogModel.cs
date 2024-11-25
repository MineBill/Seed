using System;
using System.Collections;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using Launcher.DataModels;
using Launcher.Services;
using Launcher.Services.Dummies;

namespace Launcher.ViewModels.Dialogs;

public partial class EngineConfigurationDialogModel(Engine engine, IEngineManager engineManager) : DialogModelBase<Unit>
{
    public string EngineName => engine.Name;

    public bool IsDebug => engine.PreferredConfiguration == Engine.Configuration.Debug;
    public bool IsDevelop => engine.PreferredConfiguration == Engine.Configuration.Development;
    public bool IsRelease => engine.PreferredConfiguration == Engine.Configuration.Release;

    public string CurrentConfigurationText => engine.PreferredConfiguration.ToString();
    public int Configuration => (int)engine.PreferredConfiguration;

    public EngineConfigurationDialogModel() : this(new Engine
        {
            Name = "1.6",
            Version = new NormalVersion(Version.Parse("1.6"))
        },
        new DummyEngineManager())
    {
    }

    [RelayCommand]
    private void SelectionChanged(IList items)
    {
        if (items.Count == 0)
            return;
        var cb = items[0] as ComboBoxItem;
        engine.PreferredConfiguration = (cb?.Content as string) switch
        {
            "Debug" => Engine.Configuration.Debug,
            "Development" => Engine.Configuration.Development,
            "Release" => Engine.Configuration.Release,
            _ => engine.PreferredConfiguration
        };
        engineManager.Save();
    }
}
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ReactiveUI;
using Seed.Models;

namespace Seed.ViewModels;

public class RemoteEngineViewModel : ViewModelBase
{
    public RemoteEngine RemoteEngine { get; }

    public List<PackageViewModel> Packages { get; } = new();

    public string Name => RemoteEngine.Name;

    public RemoteEngineViewModel(RemoteEngine remoteEngine)
    {
        RemoteEngine = remoteEngine;

        foreach (var package in RemoteEngine.SupportedPlatformTools)
        {
            Packages.Add(new PackageViewModel(package));
        }
    }
}

public class PackageViewModel : ViewModelBase
{
    public RemotePackage RemotePackage { get; }

    private bool _isChecked;

    public bool IsChecked
    {
        get => _isChecked;
        set => this.RaiseAndSetIfChanged(ref _isChecked, value);
    }

    public string Name => RemotePackage.Name;

    public bool IsEditorPackage => RemotePackage.IsEditorPackage;

    public bool IsCurrentPlatform
    {
        get
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return Name.Contains("Linux");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return Name.Contains("Windows");
            throw new ArgumentException($"Unsupported os platform: {Environment.OSVersion}");
        }
    }

    public PackageViewModel(RemotePackage remotePackage)
    {
        RemotePackage = remotePackage;
        IsChecked = false;
    }
}
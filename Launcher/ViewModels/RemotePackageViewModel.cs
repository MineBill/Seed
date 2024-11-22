using System;
using System.Runtime.InteropServices;
using CommunityToolkit.Mvvm.ComponentModel;
using Launcher.DataModels;

namespace Launcher.ViewModels;

public partial class RemotePackageViewModel(RemotePackage package) : ViewModelBase
{
    [ObservableProperty] private bool _isChecked;
    public string PackageName => package.Name ?? "asd";

    public RemotePackage Package => package;

    public bool IsCurrentPlatform
    {
        get
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return PackageName.Contains("Linux");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return PackageName.Contains("Windows");
            throw new ArgumentException($"Unsupported os platform: {Environment.OSVersion}");
        }
    }
}
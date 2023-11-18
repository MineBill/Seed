using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Seed.Models;
using Seed.ViewModels;

namespace Seed.Services.Implementations;

public class LocalEngineDownloaderService : IEngineDownloaderService
{
    private readonly Dictionary<string, string> _urlToLocalMapping = new()
    {
        {
            "https://vps2.flaxengine.com/store/builds/Package_1_07_06404/FlaxEditorLinux.zip",
            "/home/minebill/Downloads/FlaxPlatformTools/1.7/FlaxEditorLinux.zip"
        },
        {
            "https://vps2.flaxengine.com/store/builds/Package_1_06_06344/FlaxEditorLinux.zip",
            "/home/minebill/Downloads/FlaxPlatformTools/1.6/FlaxEditorLinux.zip"
        },
        {
            "https://vps2.flaxengine.com/store/builds/Package_1_07_06404/Windows.zip",
            "/home/minebill/Downloads/FlaxPlatformTools/1.7/Windows.zip"
        },
        {
            "https://vps2.flaxengine.com/store/builds/Package_1_07_06404/Linux.zip",
            "/home/minebill/Downloads/FlaxPlatformTools/1.7/Linux.zip"
        },
        {
            "https://vps2.flaxengine.com/store/builds/Package_1_06_06344/Windows.zip",
            "/home/minebill/Downloads/FlaxPlatformTools/1.6/Windows.zip"
        },
        {
            "https://vps2.flaxengine.com/store/builds/Package_1_06_06344/Linux.zip",
            "/home/minebill/Downloads/FlaxPlatformTools/1.6/Linux.zip"
        },
    };

    public event Action<string> ActionChanged;
    public event Action? DownloadStarted;
    public event Action? DownloadFinished;

    private Progress<float> _progress = new();
    public Progress<float> Progress => _progress;
    private string _currentAction;

    public string CurrentAction
    {
        get => _currentAction;
        private set
        {
            _currentAction = value;
            ActionChanged?.Invoke(value);
        }
    }

    public async Task<List<RemoteEngine>?> GetAvailableVersions()
    {
        var json = await File.ReadAllTextAsync("/home/minebill/git/Seed/Seed/Assets/api.json");
        try
        {
            var tree = JsonNode.Parse(json);
            if (tree is null)
                return null;

            var engines = tree["versions"].Deserialize<List<RemoteEngine>>();
            return engines;
        }
        catch (JsonException je)
        {
            var box = MessageBoxManager.GetMessageBoxStandard(
                "Exception",
                "An exception occured while deserializing information from the Flax API. It's possible the API changed. Please make an issue at <url-repo>.",
                icon: Icon.Error);
            await box.ShowWindowDialogAsync(App.Current.MainWindow);
            return null;
        }
    }

    public Task<List<Workflow>?> GetGithubWorkflows()
    {
        throw new NotImplementedException();
    }

    public async Task<Engine> DownloadVersion(RemoteEngine engine, List<RemotePackage> platformTools,
        string installFolderPath, CancellationToken cancellationToken = default)
    {
        var tempEditorFile = Path.GetTempFileName();

        File.Copy(_urlToLocalMapping[engine.GetEditorPackage().EditorUrl], tempEditorFile, overwrite: true);

        // create sub folder for this engine installation
        var editorInstallFolder = Path.Combine(installFolderPath, engine.Name);

        // TODO: Check for errors
        CurrentAction = "Extracting editor";
        await ZipHelpers.ExtractToDirectoryAsync(tempEditorFile, editorInstallFolder, _progress);

        var installedPackages = new List<Package>(platformTools.Count);
        foreach (var tools in platformTools)
        {
            CurrentAction = $"Downloading platform tools for {tools.Name}";
            var tmpFile = Path.GetTempFileName();
            File.Copy(_urlToLocalMapping[tools.Url], tmpFile, overwrite: true);

            var installFolder = Path.Combine(editorInstallFolder, tools.TargetPath);
            CurrentAction = $"Extracting {tools.Name}";
            await ZipHelpers.ExtractToDirectoryAsync(tmpFile, installFolder, _progress);

            installedPackages.Add(new Package(tools.Name, installFolder));
        }

        CurrentAction = "Done!";

        return new Engine
        {
            Name = engine.Name,
            Path = editorInstallFolder,
            Version = new NormalVersion(engine.Version),
            InstalledPackages = installedPackages
        };
    }

    public Task<Engine> DownloadFromWorkflow(Workflow artifact, List<Artifact> platformTools, string installFolderPath,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
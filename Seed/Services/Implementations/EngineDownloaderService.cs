using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Seed.Models;

namespace Seed.Services.Implementations;

public class EngineDownloaderService : IEngineDownloaderService
{
    public const string ApiUrl = "https://api.flaxengine.com/launcher/engine";

    public event Action<string> ActionChanged;

    private HttpClient _client = new();
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

    public EngineDownloaderService()
    {
    }

    public async Task<List<RemoteEngine>?> GetAvailableVersions()
    {
        _client.DefaultRequestHeaders.Accept.Clear();
        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _client.DefaultRequestHeaders.Add("User-Agent", "Seed Launcher for Flax");

        var json = await _client.GetStringAsync(ApiUrl);
        try
        {
            var tree = JsonNode.Parse(json);
            if (tree is null)
                return null;

            var engines = tree["version"].Deserialize<List<RemoteEngine>>();
            return engines;
        }
        catch (JsonException je)
        {
            var box = MessageBoxManager.GetMessageBoxStandard(
                "Exception",
                $"An exception occured while deserializing information from the Flax API. It's possible the API has changed. Please make an issue at {Globals.RepoUrl}.",
                icon: Icon.Error);
            await box.ShowWindowDialogAsync(App.Current.MainWindow);
            return null;
        }
    }

    public async Task<Engine> DownloadVersion(RemoteEngine engine, List<RemotePackage> platformTools,
        string installFolderPath)
    {
        var tempEditorFile = Path.GetTempFileName();
#if NODEBUG
        var editorUrl = "/home/minebill/Downloads/FlaxEditorLinux.zip";
        File.Move(editorUrl, tempEditorFile, true);
#else
        CurrentAction = $"Downloading {engine.Name}";
        var editorUrl = engine.GetEditorPackage().EditorUrl;
        await using (var file = new FileStream(tempEditorFile, FileMode.Create, FileAccess.Write, FileShare.None))
            await _client.DownloadDataAsync(editorUrl, file, _progress);
        // await DownloadFile(editorUrl, tempEditorFile);
#endif
        // create sub folder for this engine installation
        var editorInstallFolder = Path.Combine(installFolderPath, engine.Name);

        // TODO: Check for errors
        // ZipFile.ExtractToDirectory(tempEditorFile, editorInstallFolder);
        CurrentAction = "Extracting editor";
        await ZipHelpers.ExtractToDirectoryAsync(tempEditorFile, editorInstallFolder, _progress);

        var installedPackages = new List<Package>(platformTools.Count);
        foreach (var tools in platformTools)
        {
            CurrentAction = $"Downloading platform tools for {tools.Name}";
            var tmpFile = Path.GetTempFileName();
            await using (var file = new FileStream(tmpFile, FileMode.Create, FileAccess.Write, FileShare.None))
                await _client.DownloadDataAsync(tools.Url, file, _progress);

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
            Version = engine.Version,
            InstalledPackages = installedPackages
        };
    }
}
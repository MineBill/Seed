#define USE_JSON_FILE
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Seed.Models;

namespace Seed.Services;

public class EngineDownloaderService: IEngineDownloaderService
{
    public const string ApiUrl = "https://api.flaxengine.com/launcher/engine";
    
    private HttpClient _client = new();

    public EngineDownloaderService()
    {
    }
    
    public async Task<List<RemoteEngine>?> GetAvailableVersions()
    {
#if USE_JSON_FILE
        var json = await File.ReadAllTextAsync("/home/minebill/git/Seed/Seed/Assets/api.json");
#else
        _client.DefaultRequestHeaders.Accept.Clear();
        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _client.DefaultRequestHeaders.Add("User-Agent", "Seed Launcher for Flax");

        var json = await _client.GetStringAsync(ApiUrl);
#endif
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
}
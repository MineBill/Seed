using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace Launcher.Services;

public class UpdateService
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private const string Url = "https://api.github.com/repos/MineBill/Seed/releases";

    private readonly HttpClient _client = new();

    public async Task<Update?> CheckForUpdate()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, Url);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Add("User-Agent", "Seed Launcher for Flax");

        try
        {
            var response = await _client.SendAsync(request, CancellationToken.None);
            try
            {
                var json = await response.Content.ReadAsStringAsync();
                var tree = JsonNode.Parse(json);

                var updates = tree?.Deserialize(UpdateListGenerationContext.Default.ListUpdate);
                var latest =
                    updates?.Where(x => x is
                        { Draft: false, PreRelease: false }).FirstOrDefault(); // We only care about the latest update. This assumes GitHub will return the latest first.

                var tagName = latest?.TagName.Replace("v", "");
                if (!Version.TryParse(tagName, out var version)) return null;
                var currentVersion = Assembly.GetEntryAssembly()?.GetName().Version;

                return version > currentVersion ? latest : null;
            }
            catch (JsonException e)
            {
                Logger.Error(e);
                return null;
            }
        }
        catch (HttpRequestException e)
        {
            if (e.HttpRequestError == HttpRequestError.ConnectionError)
            {
                Logger.Info("Update check failed due to network issues.");
            }
            else
            {
                Logger.Error(e);
            }

            return null;
        }
    }
}

public struct Update
{
    [JsonPropertyName("html_url")]
    public string Url { get; set; }

    [JsonPropertyName("published_at")]
    public DateTime PublishedAt { get; set; }

    [JsonPropertyName("draft")]
    public bool Draft { get; set; }

    [JsonPropertyName("prerelease")]
    public bool PreRelease { get; set; }

    [JsonPropertyName("tag_name")]
    public string TagName { get; set; }

    [JsonPropertyName("body")]
    public string Body { get; set; }
}

[JsonSerializable(typeof(Update))]
internal partial class UpdateGenerationContext : JsonSerializerContext;

[JsonSerializable(typeof(List<Update>))]
internal partial class UpdateListGenerationContext : JsonSerializerContext;
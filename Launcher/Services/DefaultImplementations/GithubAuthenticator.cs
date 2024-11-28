using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace Seed.Services.Implementations;

public class GithubAuthenticator
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private const string ClientId = "Iv1.4582b571275a312a";

    private readonly HttpClient _client = new()
    {
        BaseAddress = new Uri("https://github.com"),
        DefaultRequestHeaders =
        {
            Accept = { MediaTypeWithQualityHeaderValue.Parse("application/json") }
        }
    };

    public async Task<AuthenticationResult> Authenticate(
        DeviceCodeResponse deviceCodeResponse, CancellationToken cancellationToken = default)
    {
        const float extraSafetyDelay = 0.2f;
        return await Task.Run(async () =>
        {
            do
            {
                Logger.Debug("Checking for github response...");
                var tokenResponse = RequestToken(deviceCodeResponse.DeviceCode);
                var message = await tokenResponse.Result.Content.ReadAsStringAsync(cancellationToken);
                var json = JsonNode.Parse(message);

                var error = json!["error"]?.ToString();
                if (error is null)
                {
                    // fucking finally
                    var token = json["access_token"]?.ToString()!;
                    return new AuthenticationResult(token,
                        AuthenticationError.Ok);
                }

                switch (error)
                {
                    case "slow_down":
                        // When you receive the slow_down error, 5 extra seconds are added to the minimum interval.
                        await Task.Delay(TimeSpan.FromSeconds(deviceCodeResponse.Interval + extraSafetyDelay + 5.5f),
                            cancellationToken);
                        break;
                    case "authorization_pending":
                        // This error occurs when the authorization request is pending and the user hasn't entered the user code yet.
                        // Keep polling.
                        await Task.Delay(TimeSpan.FromSeconds(deviceCodeResponse.Interval + extraSafetyDelay),
                            cancellationToken);
                        break;
                    case "expired_token":
                        // If the device code expired, then you will see the token_expired error.
                        // You must make a new request for a device code.
                        return new AuthenticationResult(null,
                            AuthenticationError.Expired);
                    case "access_denied":
                        return new AuthenticationResult(null,
                            AuthenticationError.AccessDenied);
                    case "unsupported_grant_type":
                        return new AuthenticationResult(null,
                            AuthenticationError.UnsupportedGrantType);
                    case "incorrect_client_credentials":
                        return new AuthenticationResult(null,
                            AuthenticationError.IncorrectCredentials);
                    case "incorrect_device_code":
                        return new AuthenticationResult(null,
                            AuthenticationError.IncorrectDeviceCode);
                    case "device_flow_disabled":
                        return new AuthenticationResult(null,
                            AuthenticationError.DeviceFlowDisabled);
                }
            } while (true);
        }, cancellationToken);
    }

    private async Task<HttpResponseMessage> RequestToken(string deviceCode)
    {
        using var content = new StringContent(
            JsonSerializer.Serialize(new
            {
                client_id = ClientId,
                device_code = deviceCode,
                grant_type = "urn:ietf:params:oauth:grant-type:device_code"
            }), Encoding.UTF8, "application/json"
        );
        return await _client.PostAsync("login/oauth/access_token", content);
    }

    public async Task<DeviceCodeResponse> RequestDeviceCode()
    {
        using var content = new StringContent(
            JsonSerializer.Serialize(new
            {
                client_id = ClientId
            }), Encoding.UTF8, "application/json"
        );

        var response = await _client.PostAsync("/login/device/code", content);
        return await response.Content.ReadFromJsonAsync(DeviceCodeResponseGenerationContext.Default
            .DeviceCodeResponse);
    }
}

public record AuthenticationResult(string? AccessToken, AuthenticationError Error);

public enum AuthenticationError
{
    Ok,
    Expired,
    AccessDenied,
    Pending,
    IncorrectCredentials,
    IncorrectDeviceCode,
    UnsupportedGrantType,
    DeviceFlowDisabled,
    Failed,
}

public struct DeviceCodeResponse
{
    /// <summary>
    /// The device code, which is needed to generate an access token.
    /// </summary>
    [JsonPropertyName("device_code")]
    public string DeviceCode { get; set; }

    /// <summary>
    /// The device code for this users' device.
    /// </summary>
    [JsonPropertyName("user_code")]
    public string UserCode { get; set; }

    /// <summary>
    /// The uri where the user needs to authenticate.
    /// </summary>
    [JsonPropertyName("verification_uri")]
    public Uri VerificationUri { get; set; }

    /// <summary>
    /// How many seconds until the token expires.
    /// </summary>
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    /// <summary>
    /// Delay in seconds between checking for auth status.
    /// </summary>
    [JsonPropertyName("interval")]
    public int Interval { get; set; }
}

[JsonSerializable(typeof(DeviceCodeResponse))]
internal partial class DeviceCodeResponseGenerationContext : JsonSerializerContext;
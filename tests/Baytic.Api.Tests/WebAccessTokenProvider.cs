using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Baytic.Api.Tests;

/// <summary>
/// Provides OAuth2 access tokens for authenticated test requests against Keycloak.
/// Implements token caching with expiration checks to avoid unnecessary token requests.
/// </summary>
public class WebAccessTokenProvider
{
    private const string DefaultUsername = "test-user";
    private const string DefaultPassword = "password";
    private const int ExpirationBufferSeconds = 10; // Request new token 10s before expiry

    private readonly ILogger<WebAccessTokenProvider> _logger;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly Dictionary<string, AccessTokenItem> _tokenCache = new();

    private class AccessTokenItem
    {
        public string AccessToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }

        public bool IsExpired => DateTime.UtcNow.AddSeconds(ExpirationBufferSeconds) >= ExpiresAt;
    }

    public WebAccessTokenProvider(
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        ILoggerFactory loggerFactory)
    {
        _configuration = configuration;
        _httpClient = httpClientFactory.CreateClient();
        _logger = loggerFactory.CreateLogger<WebAccessTokenProvider>();
    }

    /// <summary>
    /// Gets or requests a new API token for the specified client.
    /// Caches valid tokens to avoid unnecessary token server requests.
    /// </summary>
    /// <param name="clientId">The OAuth2 client identifier</param>
    /// <param name="scope">The requested OAuth2 scope</param>
    /// <param name="secret">The client secret for authentication</param>
    /// <returns>A valid access token</returns>
    /// <exception cref="InvalidOperationException">Thrown if token acquisition fails</exception>
    public async Task<string> GetApiToken(string clientId, string scope, string secret)
    {
        var cacheKey = $"{clientId}:{scope}";

        // Check if we have a cached token that hasn't expired
        if (_tokenCache.TryGetValue(cacheKey, out var cachedToken) && !cachedToken.IsExpired)
        {
            _logger.LogDebug("Using cached token for {ClientId} (expires in {ExpiresIn}s)", 
                clientId, (cachedToken.ExpiresAt - DateTime.UtcNow).TotalSeconds);
            return cachedToken.AccessToken;
        }

        _logger.LogDebug("Requesting new token from Keycloak for client {ClientId}", clientId);

        var username = _configuration["Keycloak:Username"] ?? DefaultUsername;
        var password = _configuration["Keycloak:Password"] ?? DefaultPassword;

        var newToken = await GetInternalApiToken(clientId, scope, secret, username, password);

        // Cache the token
        _tokenCache[cacheKey] = newToken;

        return newToken.AccessToken;
    }

    private async Task<AccessTokenItem> GetInternalApiToken(string clientId, string scope, string secret, string username, string password)
    {
        try
        {
            var authority = _configuration["Keycloak:Authority"]
                ?? throw new InvalidOperationException("Keycloak:Authority configuration is missing");

            _logger.LogDebug("Discovering token endpoint from {Authority}/.well-known/openid-configuration", authority);

            var discoveryUrl = $"{authority}/.well-known/openid-configuration";
            var discoveryDocumentResponse = await _httpClient.GetAsync(discoveryUrl);

            discoveryDocumentResponse.EnsureSuccessStatusCode();

            var discoveryDocument = await discoveryDocumentResponse.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            
            if (discoveryDocument is null || !discoveryDocument.TryGetValue("token_endpoint", out var endpoint))
            {
                throw new InvalidOperationException(
                    $"Keycloak discovery document from {discoveryUrl} does not contain a token_endpoint.");
            }

            _logger.LogDebug("Using token endpoint: {Endpoint}", endpoint);

            using var tokenRequestContent = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["client_id"] = clientId,
                ["client_secret"] = secret,
                ["scope"] = scope,
                ["username"] = username,
                ["password"] = password,
            });

            var tokenResponse = await _httpClient.PostAsync(endpoint, tokenRequestContent);

            if (!tokenResponse.IsSuccessStatusCode)
            {
                var errorContent = await tokenResponse.Content.ReadAsStringAsync();
                _logger.LogError(
                    "Token request failed with status {StatusCode}: {ErrorContent}",
                    tokenResponse.StatusCode, errorContent);
                tokenResponse.EnsureSuccessStatusCode();
            }

            var tokenPayload = await tokenResponse.Content.ReadFromJsonAsync<AccessTokenResponse>();

            if (tokenPayload is null || string.IsNullOrWhiteSpace(tokenPayload.AccessToken))
            {
                throw new InvalidOperationException("Keycloak returned an empty access token.");
            }

            var expiresAt = DateTime.UtcNow.AddSeconds(tokenPayload.ExpiresIn);
            _logger.LogDebug("Token acquired, expires at {ExpiresAt} ({ExpiresIn}s)", expiresAt, tokenPayload.ExpiresIn);

            return new AccessTokenItem
            {
                ExpiresAt = expiresAt,
                AccessToken = tokenPayload.AccessToken,
            };
        }
        catch (HttpRequestException e)
        {
            _logger.LogError(e, 
                "HTTP error while acquiring access token for client {ClientId} with scope {Scope}", 
                clientId, scope);
            throw new InvalidOperationException($"Failed to acquire access token from Keycloak: {e.Message}", e);
        }
        catch (Exception e)
        {
            _logger.LogError(e, 
                "Unexpected error while acquiring access token for client {ClientId}", 
                clientId);
            throw new InvalidOperationException("Failed to acquire access token from Keycloak.", e);
        }
    }
}

internal sealed class AccessTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
}
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Baytic.Api.Tests;

/// <summary>
/// Base class for API integration tests using XUnit.
/// Provides shared HttpClient and factory with Keycloak and database containers.
/// 
/// Usage:
/// <code>
/// public class UserApiTests(BaseFactory factory) : BaseIntegrationTest(factory)
/// {
///     [Fact]
///     public async Task CreateUser_WithValidData_ReturnsCreated()
///     {
///         // Arrange
///         var client = await Factory.CreateAuthenticatedClientAsync(HttpMethod.Post);
///         var request = new CreateUserRequest { Email = "test@example.com" };
///
///         // Act
///         var response = await client.PostAsJsonAsync("/api/users", request);
///
///         // Assert
///         Assert.Equal(HttpStatusCode.Created, response.StatusCode);
///     }
/// }
/// </code>
/// </summary>
[Trait("Category", "Integration")]
public class BaseIntegrationTest : IAsyncLifetime
{
    /// <summary>HTTP client configured for the test API</summary>
    protected HttpClient Client { get; }

    /// <summary>WebApplicationFactory for creating test clients and accessing DI container</summary>
    protected BaseFactory Factory { get; }

    public BaseIntegrationTest(BaseFactory factory)
    {
        Factory = factory;

        Client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });
    }

    /// <summary>
    /// Creates an HTTP client with Bearer token authentication.
    /// Token is retrieved from Keycloak test user credentials.
    /// </summary>
    /// <returns>HttpClient with Authorization header set</returns>
    protected async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var tokenProvider = Factory.Services.GetRequiredService<WebAccessTokenProvider>();
        var token = await tokenProvider.GetApiToken(
            BaseFactory.ClientId,
            "openid",
            BaseFactory.ClientSecret);

        var client = Factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });

        return client.WithAuthentication(token);
    }

    /// <summary>
    /// Initialize async resources (data seeding, etc).
    /// Override in derived classes to seed test-specific data.
    /// </summary>
    public virtual ValueTask InitializeAsync()
    {
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Clean up async resources.
    /// Override in derived classes to clean up test-specific data.
    /// </summary>
    public virtual ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }
}

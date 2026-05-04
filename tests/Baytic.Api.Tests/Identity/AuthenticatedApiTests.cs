using System.Net;
using System.Net.Http.Json;

namespace Baytic.Api.Tests.Identity;

/// <summary>
/// Example integration tests for authenticated endpoints.
/// Demonstrates:
/// - Using CreateAuthenticatedClientAsync() for Bearer token
/// - Testing with valid credentials
/// - Testing error scenarios (unauthorized)
/// - Using [Theory] and [InlineData] for data-driven tests
/// </summary>
public class AuthenticatedApiTests(BaseFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task GetProfile_WithAuthentication_ReturnsOk()
    {
        // Arrange
        var client = await CreateAuthenticatedClientAsync();

        // Act
        var response = await client.GetAsync("/api/profile");

        // Assert
        Assert.True(response.IsSuccessStatusCode, $"Expected success, got {response.StatusCode}");
    }

    [Fact]
    public async Task GetProfile_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        // (no auth setup - using unauthenticated client)

        // Act
        var response = await Client.GetAsync("/api/profile");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("/api/profile")]
    [InlineData("/api/settings")]
    [InlineData("/api/data")]
    public async Task ProtectedEndpoints_WithoutAuth_ReturnUnauthorized(string endpoint)
    {
        // Arrange
        // (no auth setup - using unauthenticated client)

        // Act
        var response = await Client.GetAsync(endpoint);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("/api/profile")]
    [InlineData("/api/settings")]
    [InlineData("/api/data")]
    public async Task ProtectedEndpoints_WithAuth_ReturnSuccess(string endpoint)
    {
        // Arrange
        var client = await CreateAuthenticatedClientAsync();

        // Act
        var response = await client.GetAsync(endpoint);

        // Assert
        Assert.True(
            response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotFound,
            $"Expected success or NotFound for {endpoint}, got {response.StatusCode}");
    }
}

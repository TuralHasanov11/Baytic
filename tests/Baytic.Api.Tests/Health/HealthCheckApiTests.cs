using System.Net;

namespace Baytic.Api.Tests.Health;

/// <summary>
/// Example integration tests for the health check endpoint.
/// Demonstrates:
/// - Basic HTTP requests without authentication
/// - Simple assertions with XUnit [Fact]
/// - Using BaseIntegrationTest for shared setup
/// </summary>
public class HealthCheckApiTests(BaseFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task HealthCheck_WhenCalled_ReturnsOk()
    {
        // Arrange
        // (no setup needed for this test)

        // Act
        var response = await Client.GetAsync("/api/health", TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.IsSuccessStatusCode, $"Expected success, got {response.StatusCode}");
    }

    [Fact]
    public async Task HealthCheck_ReturnsHealthyStatus()
    {
        // Arrange
        // (no setup needed for this test)

        // Act
        var response = await Client.GetAsync("/api/health", TestContext.Current.CancellationToken);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.NotEmpty(content);
        Assert.Contains("healthy", content, StringComparison.OrdinalIgnoreCase);
    }
}

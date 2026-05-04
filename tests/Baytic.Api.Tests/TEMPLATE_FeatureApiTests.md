## TEMPLATE: Feature API Tests

```cs
using System.Net;
using System.Net.Http.Json;

namespace Baytic.Api.Tests.Features;

/// <summary>
/// Integration tests for [Feature] API endpoints.
/// 
/// Test Classes Follow XUnit Conventions:
/// - Inherit from BaseIntegrationTest(factory) for dependency injection
/// - Use [Fact] for simple tests, [Theory] for parameterized tests
/// - Follow Arrange-Act-Assert pattern
/// - Name tests: Method_Scenario_ExpectedBehavior
/// 
/// Authentication:
/// - Use Client for unauthenticated requests
/// - Use await CreateAuthenticatedClientAsync() for protected endpoints
/// - Tokens are automatically cached (no manual management needed)
/// 
/// See: README.md for quick reference, KEYCLOAK_SETUP.md for detailed guide
/// </summary>
public class FeatureApiTests(BaseFactory factory) : BaseIntegrationTest(factory)
{
    /// <summary>
    /// Example: Test successful operation without authentication
    /// </summary>
    [Fact]
    public async Task GetPublicData_WithoutAuth_ReturnsOk()
    {
        // Arrange
        // (no setup required for unauthenticated endpoint)

        // Act
        var response = await Client.GetAsync("/api/features/public");

        // Assert
        Assert.NotNull(response);
        Assert.True(response.IsSuccessStatusCode, $"Expected success, got {response.StatusCode}");
    }

    /// <summary>
    /// Example: Test successful operation with authentication
    /// </summary>
    [Fact]
    public async Task GetUserFeatures_WithAuthentication_ReturnsOk()
    {
        // Arrange
        var client = await CreateAuthenticatedClientAsync();

        // Act
        var response = await client.GetAsync("/api/features/user");

        // Assert
        Assert.True(response.IsSuccessStatusCode, $"Expected success, got {response.StatusCode}");
    }

    /// <summary>
    /// Example: Test data-driven scenarios with multiple inputs
    /// Use [Theory] + [InlineData] to avoid code duplication
    /// </summary>
    [Theory]
    [InlineData("scenario1", true)]
    [InlineData("scenario2", false)]
    [InlineData("scenario3", true)]
    public async Task CheckFeature_WithVariousScenarios_ReturnsExpected(string scenario, bool expected)
    {
        // Arrange
        var client = await CreateAuthenticatedClientAsync();

        // Act
        var response = await client.GetAsync($"/api/features/check?scenario={scenario}");

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        // TODO: Replace with your actual API response type
        // var result = await response.Content.ReadAsAsync<bool>();
        // Assert.Equal(expected, result);
    }

    /// <summary>
    /// Example: Test POST request with JSON body
    /// </summary>
    [Fact]
    public async Task CreateFeature_WithValidRequest_ReturnsCreated()
    {
        // Arrange
        var client = await CreateAuthenticatedClientAsync();
        // TODO: Replace with your actual request type
        // var request = new CreateFeatureRequest
        // {
        //     Name = "Test Feature",
        //     Description = "Test Description",
        // };

        // Act
        // var response = await client.PostAsJsonAsync("/api/features", request);

        // Assert
        // Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        // var result = await response.Content.ReadAsAsync<FeatureDto>();
        // Assert.NotNull(result.Id);
        // Assert.Equal(request.Name, result.Name);
    }

    /// <summary>
    /// Example: Test error handling - missing required field
    /// </summary>
    [Fact]
    public async Task CreateFeature_WithoutName_ReturnsBadRequest()
    {
        // Arrange
        var client = await CreateAuthenticatedClientAsync();
        // TODO: Replace with your actual request type
        // var request = new CreateFeatureRequest
        // {
        //     // Missing Name
        //     Description = "Test Description",
        // };

        // Act
        // var response = await client.PostAsJsonAsync("/api/features", request);

        // Assert
        // Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>
    /// Example: Test authorization - endpoint requires specific role
    /// </summary>
    [Fact]
    public async Task DeleteFeature_WithoutAdminRole_ReturnsForbidden()
    {
        // Arrange
        var client = await CreateAuthenticatedClientAsync();

        // Act
        var response = await client.DeleteAsync("/api/features/123");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    /// <summary>
    /// Example: Test with database seeding
    /// Override InitializeAsync to prepare test data
    /// </summary>
    [Fact]
    public async Task GetFeature_WithExistingFeature_ReturnsFeature()
    {
        // Note: Run this test with override of InitializeAsync below

        // Arrange
        var client = await CreateAuthenticatedClientAsync();

        // Act
        var response = await client.GetAsync("/api/features/pre-seeded-id");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>
    /// Override InitializeAsync to seed test data before each test
    /// Optional: Only implement if your tests need specific data setup
    /// </summary>
    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();

        // Example: Seed database with test data
        // var dbContext = Factory.Services.GetRequiredService<ApplicationDbContext>();
        // dbContext.Features.Add(new Feature
        // {
        //     Id = "pre-seeded-id",
        //     Name = "Pre-seeded Feature",
        // });
        // await dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Override DisposeAsync to clean up test-specific data
    /// Optional: Only implement if InitializeAsync creates test data
    /// </summary>
    public override async ValueTask DisposeAsync()
    {
        // Example: Clean up test data
        // var dbContext = Factory.Services.GetRequiredService<ApplicationDbContext>();
        // var testData = await dbContext.Features
        //     .Where(f => f.Id == "pre-seeded-id")
        //     .ToListAsync();
        // dbContext.Features.RemoveRange(testData);
        // await dbContext.SaveChangesAsync();

        GC.SuppressFinalize(this);
        await base.DisposeAsync();
    }
}

// TODO: Define your request/response DTOs
// Keep them near the tests or in a shared folder
// Example:
// public record CreateFeatureRequest(string Name, string Description);
// public record FeatureDto(string Id, string Name, string Description);


```
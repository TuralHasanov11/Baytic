using System.Net;
using System.Net.Http.Json;
using Baytic.Application.Blog;
using Baytic.Domain.Blog;
using Baytic.Domain.Identity;
using Baytic.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Baytic.Api.Tests.Blog;

public class BlogPostApiTests(BaseFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task GetBlogPosts_WhenPostsExist_ReturnsOk()
    {
        var seeded = await SeedBlogPostAsync("Public title", "public-title");

        var response = await Client.GetAsync("/api/v1/blog");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var blogPosts = await response.Content.ReadFromJsonAsync<List<BlogPostResponse>>();

        Assert.NotNull(blogPosts);
        Assert.Contains(blogPosts!, post => post.Id == seeded.Id && post.Title == "Public title");
    }

    [Fact]
    public async Task GetBlogPostById_WhenPostExists_ReturnsOk()
    {
        var seeded = await SeedBlogPostAsync("Get title", "get-title");

        var response = await Client.GetAsync($"/api/v1/blog/{seeded.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var blogPost = await response.Content.ReadFromJsonAsync<BlogPostResponse>();

        Assert.NotNull(blogPost);
        Assert.Equal(seeded.Id, blogPost!.Id);
        Assert.Equal("Get title", blogPost.Title);
    }

    [Fact]
    public async Task CreateBlogPost_WithAuthentication_ReturnsCreated()
    {
        var client = await CreateAuthenticatedClientAsync();
        var request = new CreateBlogPostRequest("Created title", "created-title", "Created body");

        var response = await client.PostAsJsonAsync("/api/v1/blog", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<BlogPostResponse>();

        Assert.NotNull(created);
        Assert.Equal("Created title", created!.Title);
        Assert.Equal("created-title", created.Slug);
        Assert.Equal(BlogPostStatus.Draft, created.Status);
    }

    [Fact]
    public async Task UpdateBlogPost_WithAuthentication_ReturnsOk()
    {
        var seeded = await SeedBlogPostAsync("Original title", "original-title");
        var client = await CreateAuthenticatedClientAsync();
        var request = new UpdateBlogPostRequest("Updated title", "updated-title", "Updated body");

        var response = await client.PutAsJsonAsync($"/api/v1/blog/{seeded.Id}", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updated = await response.Content.ReadFromJsonAsync<BlogPostResponse>();

        Assert.NotNull(updated);
        Assert.Equal("Updated title", updated!.Title);
        Assert.Equal("updated-title", updated.Slug);
    }

    [Fact]
    public async Task PublishBlogPost_WithAuthentication_ReturnsOk()
    {
        var seeded = await SeedBlogPostAsync("Publish title", "publish-title");
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.PostAsync($"/api/v1/blog/{seeded.Id}/publish", content: null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var published = await response.Content.ReadFromJsonAsync<BlogPostResponse>();

        Assert.NotNull(published);
        Assert.Equal(BlogPostStatus.Published, published!.Status);
        Assert.NotNull(published.PublishedAt);
    }

    [Fact]
    public async Task ArchiveBlogPost_WithAuthentication_ReturnsOk()
    {
        var seeded = await SeedBlogPostAsync("Archive title", "archive-title");
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.PostAsync($"/api/v1/blog/{seeded.Id}/archive", content: null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var archived = await response.Content.ReadFromJsonAsync<BlogPostResponse>();

        Assert.NotNull(archived);
        Assert.Equal(BlogPostStatus.Archived, archived!.Status);
    }

    private async Task<BlogPost> SeedBlogPostAsync(string title, string slug)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var blogPost = BlogPost.CreateDraft(new UserId(Guid.NewGuid()), title, slug, "Body content");

        dbContext.BlogPosts.Add(blogPost);
        await dbContext.SaveChangesAsync();

        return blogPost;
    }
}
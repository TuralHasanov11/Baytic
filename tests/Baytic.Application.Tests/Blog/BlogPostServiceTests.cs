using Baytic.Application.Blog;
using Baytic.Domain.Blog;
using Baytic.Domain.Identity;
using Baytic.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Baytic.Application.Tests.Blog;

public class BlogPostServiceTests
{
    [Fact]
    public async Task CreateDraftAsync_WhenRequestIsValid_PersistsBlogPost()
    {
        await using var dbContext = CreateDbContext();
        var service = new BlogPostService(dbContext);
        var authorId = new UserId(Guid.NewGuid());
        var request = new CreateBlogPostRequest("Draft title", "draft-title", "Draft body");

        var response = await service.CreateDraftAsync(authorId, request);

        Assert.Equal("Draft title", response.Title);
        Assert.Equal(BlogPostStatus.Draft, response.Status);
        Assert.Single(dbContext.BlogPosts);
    }

    [Fact]
    public async Task GetByIdAsync_WhenBlogPostExists_ReturnsResponse()
    {
        await using var dbContext = CreateDbContext();
        var service = new BlogPostService(dbContext);
        var blogPost = BlogPost.CreateDraft(new UserId(Guid.NewGuid()), "Lookup title", "lookup-title", "Lookup body");

        dbContext.BlogPosts.Add(blogPost);
        await dbContext.SaveChangesAsync();

        var response = await service.GetByIdAsync(blogPost.Id);

        Assert.NotNull(response);
        Assert.Equal(blogPost.Id, response!.Id);
        Assert.Equal("Lookup title", response.Title);
    }

    [Fact]
    public async Task ListAsync_WhenStatusIsProvided_ReturnsMatchingPosts()
    {
        await using var dbContext = CreateDbContext();
        var service = new BlogPostService(dbContext);

        var draft = BlogPost.CreateDraft(new UserId(Guid.NewGuid()), "Draft title", "draft-title", "Draft body");
        var published = BlogPost.CreateDraft(new UserId(Guid.NewGuid()), "Published title", "published-title", "Published body");
        published.Publish(new DateTime(2026, 5, 4, 12, 0, 0, DateTimeKind.Utc));

        dbContext.BlogPosts.AddRange(draft, published);
        await dbContext.SaveChangesAsync();

        var response = await service.ListAsync(BlogPostStatus.Published);

        Assert.Single(response);
        Assert.Equal("Published title", response.Single().Title);
    }

    [Fact]
    public async Task UpdateDraftAsync_WhenBlogPostExists_UpdatesBlogPost()
    {
        await using var dbContext = CreateDbContext();
        var service = new BlogPostService(dbContext);
        var blogPost = BlogPost.CreateDraft(new UserId(Guid.NewGuid()), "Original title", "original-title", "Original body");

        dbContext.BlogPosts.Add(blogPost);
        await dbContext.SaveChangesAsync();

        var request = new UpdateBlogPostRequest("Updated title", "updated-title", "Updated body");
        var response = await service.UpdateDraftAsync(blogPost.Id, request);

        Assert.NotNull(response);
        Assert.Equal("Updated title", response!.Title);
        Assert.Equal("updated-title", response.Slug);
    }

    [Fact]
    public async Task PublishAsync_WhenBlogPostExists_SetsPublishedState()
    {
        await using var dbContext = CreateDbContext();
        var service = new BlogPostService(dbContext);
        var blogPost = BlogPost.CreateDraft(new UserId(Guid.NewGuid()), "Publish title", "publish-title", "Publish body");

        dbContext.BlogPosts.Add(blogPost);
        await dbContext.SaveChangesAsync();

        var publishedAt = new DateTime(2026, 5, 4, 13, 0, 0, DateTimeKind.Utc);
        var response = await service.PublishAsync(blogPost.Id, publishedAt);

        Assert.NotNull(response);
        Assert.Equal(BlogPostStatus.Published, response!.Status);
        Assert.Equal(publishedAt, response.PublishedAt);
    }

    [Fact]
    public async Task ArchiveAsync_WhenBlogPostExists_SetsArchivedState()
    {
        await using var dbContext = CreateDbContext();
        var service = new BlogPostService(dbContext);
        var blogPost = BlogPost.CreateDraft(new UserId(Guid.NewGuid()), "Archive title", "archive-title", "Archive body");

        dbContext.BlogPosts.Add(blogPost);
        await dbContext.SaveChangesAsync();

        var response = await service.ArchiveAsync(blogPost.Id);

        Assert.NotNull(response);
        Assert.Equal(BlogPostStatus.Archived, response!.Status);
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}
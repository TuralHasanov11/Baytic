using Baytic.Domain.Blog;
using Baytic.Domain.Identity;

namespace Baytic.Domain.Tests;

public class BlogPostTests
{
    [Fact]
    public void CreateDraft_ShouldInitializeDraftAggregate()
    {
        var authorId = new UserId(Guid.NewGuid());

        var post = BlogPost.CreateDraft(authorId, "  Vet blog  ", "Vet-blog", "  Body text  ");

        Assert.Equal(authorId, post.AuthorId);
        Assert.Equal("Vet blog", post.Title);
        Assert.Equal("vet-blog", post.Slug);
        Assert.Equal("Body text", post.Body);
        Assert.Equal(BlogPostStatus.Draft, post.Status);
    }

    [Fact]
    public void UpdateDraft_ShouldUpdateDraftFields()
    {
        var post = BlogPost.CreateDraft(new UserId(Guid.NewGuid()), "Vet blog", "vet-blog", "Body text");

        post.UpdateDraft("Updated title", "Updated-slug", "Updated body");

        Assert.Equal("Updated title", post.Title);
        Assert.Equal("updated-slug", post.Slug);
        Assert.Equal("Updated body", post.Body);
    }

    [Fact]
    public void Publish_ShouldChangeStateAndRaiseEvent()
    {
        var post = BlogPost.CreateDraft(new UserId(Guid.NewGuid()), "Vet blog", "vet-blog", "Body text");
        var publishedAt = new DateTime(2026, 5, 4, 12, 30, 0, DateTimeKind.Utc);

        post.Publish(publishedAt);

        Assert.Equal(BlogPostStatus.Published, post.Status);
        Assert.Equal(publishedAt, post.PublishedAt);
        Assert.Single(post.DomainEvents);
        var domainEvent = Assert.IsType<BlogPostPublishedDomainEvent>(post.DomainEvents.Single());
        Assert.Equal(post.Id, domainEvent.BlogPostId);
        Assert.Equal(post.AuthorId, domainEvent.AuthorId);
        Assert.Equal(publishedAt, domainEvent.OccurredOn);
    }

    [Fact]
    public void Archive_ShouldBeIdempotent_WhenCalledMultipleTimes()
    {
        var post = BlogPost.CreateDraft(new UserId(Guid.NewGuid()), "Vet blog", "vet-blog", "Body text");

        post.Archive();
        post.Archive();

        Assert.Equal(BlogPostStatus.Archived, post.Status);
    }
}
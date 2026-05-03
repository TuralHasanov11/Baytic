using Baytic.Domain.Identity;
using SharedKernel.Domain;

namespace Baytic.Domain.Blog;

public sealed class BlogPost : BaseEntity
{
    public UserId AuthorId { get; }

    public string Title { get; private set; }

    public string Slug { get; private set; }

    public string Body { get; private set; }

    public BlogPostStatus Status { get; private set; }

    public DateTime? PublishedAt { get; private set; }

    public static BlogPost CreateDraft(UserId authorId, string title, string slug, string body)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);
        ArgumentException.ThrowIfNullOrWhiteSpace(body);

        return new BlogPost(authorId, title.Trim(), NormalizeSlug(slug), body.Trim());
    }

    public void UpdateDraft(string title, string slug, string body)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);
        ArgumentException.ThrowIfNullOrWhiteSpace(body);

        Title = title.Trim();
        Slug = NormalizeSlug(slug);
        Body = body.Trim();
        Touch();
    }

    public void Publish(DateTime publishedAtUtc)
    {
        Status = BlogPostStatus.Published;
        PublishedAt = publishedAtUtc;
        Touch();

        AddDomainEvent(new BlogPostPublishedDomainEvent(Id, AuthorId, PublishedAt.Value));
    }

    public void Archive()
    {
        if (Status == BlogPostStatus.Archived)
        {
            return;
        }

        Status = BlogPostStatus.Archived;
        Touch();
    }

    private BlogPost(UserId authorId, string title, string slug, string body)
    {
        AuthorId = authorId;
        Title = title;
        Slug = slug;
        Body = body;
        Status = BlogPostStatus.Draft;
    }

    private static string NormalizeSlug(string slug)
    {
        return slug.Trim().ToLowerInvariant();
    }

    private void Touch()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}

public enum BlogPostStatus
{
    Draft,
    Scheduled,
    Published,
    Archived,
}

public sealed record BlogPostPublishedDomainEvent(Guid BlogPostId, UserId AuthorId, DateTime OccurredOn)
    : DomainEvent(OccurredOn);
using SharedKernel.Domain;

namespace Baytic.Domain.Announcements;

public sealed class Announcement : BaseEntity
{
    public string Title { get; private set; } = string.Empty;

    public string Slug { get; private set; } = string.Empty;

    public string Body { get; private set; } = string.Empty;

    public string Audience { get; private set; } = string.Empty;

    public AnnouncementStatus Status { get; private set; }

    public DateTime? PublishAtUtc { get; private set; }

    public DateTime? ExpiresAtUtc { get; private set; }

    public static Announcement CreateDraft(string title, string slug, string body, string audience, DateTime? publishAtUtc = null, DateTime? expiresAtUtc = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);
        ArgumentException.ThrowIfNullOrWhiteSpace(body);
        ArgumentException.ThrowIfNullOrWhiteSpace(audience);

        return new Announcement(title.Trim(), NormalizeSlug(slug), body.Trim(), audience.Trim(), publishAtUtc, expiresAtUtc);
    }

    public void UpdateDraft(string title, string body, string audience, DateTime? publishAtUtc = null, DateTime? expiresAtUtc = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(body);
        ArgumentException.ThrowIfNullOrWhiteSpace(audience);

        Title = title.Trim();
        Body = body.Trim();
        Audience = audience.Trim();
        PublishAtUtc = publishAtUtc;
        ExpiresAtUtc = expiresAtUtc;
        Touch();
    }

    public void Publish()
    {
        Status = AnnouncementStatus.Published;
        PublishAtUtc ??= DateTime.UtcNow;
        Touch();
    }

    public void Archive()
    {
        Status = AnnouncementStatus.Archived;
        Touch();
    }

    private Announcement(string title, string slug, string body, string audience, DateTime? publishAtUtc, DateTime? expiresAtUtc)
    {
        Title = title;
        Slug = slug;
        Body = body;
        Audience = audience;
        PublishAtUtc = publishAtUtc;
        ExpiresAtUtc = expiresAtUtc;
        Status = AnnouncementStatus.Draft;
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

public enum AnnouncementStatus
{
    Draft,
    Published,
    Archived,
}
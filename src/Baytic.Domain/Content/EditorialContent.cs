using SharedKernel.Domain;

namespace Baytic.Domain.Content;

public sealed class EditorialContent : BaseEntity
{
    private readonly List<string> _tags = [];

    public EditorialContentType ContentType { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string Slug { get; private set; } = string.Empty;

    public string Summary { get; private set; } = string.Empty;

    public string Body { get; private set; } = string.Empty;

    public string? Specialty { get; private set; }

    public string? Species { get; private set; }

    public ContentLifecycleStatus Status { get; private set; }

    public DateTime? PublishedAtUtc { get; private set; }

    public DateTime? ReviewedAtUtc { get; private set; }

    public string? ReviewedBy { get; private set; }

    public IReadOnlyCollection<string> Tags => _tags.AsReadOnly();

    public static EditorialContent CreateResource(string title, string slug, string summary, string body, string? specialty = null, string? species = null)
    {
        return Create(EditorialContentType.Resource, title, slug, summary, body, specialty, species);
    }

    public static EditorialContent CreatePublication(string title, string slug, string summary, string body, string? specialty = null, string? species = null)
    {
        return Create(EditorialContentType.Publication, title, slug, summary, body, specialty, species);
    }

    public void UpdateDraft(string title, string slug, string summary, string body, string? specialty = null, string? species = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);
        ArgumentException.ThrowIfNullOrWhiteSpace(summary);
        ArgumentException.ThrowIfNullOrWhiteSpace(body);

        Title = title.Trim();
        Slug = NormalizeSlug(slug);
        Summary = summary.Trim();
        Body = body.Trim();
        Specialty = string.IsNullOrWhiteSpace(specialty) ? null : specialty.Trim();
        Species = string.IsNullOrWhiteSpace(species) ? null : species.Trim();
        Touch();
    }

    public void AddTag(string tag)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tag);

        var normalizedTag = NormalizeTag(tag);
        if (_tags.Contains(normalizedTag))
        {
            return;
        }

        _tags.Add(normalizedTag);
        Touch();
    }

    public void RemoveTag(string tag)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tag);

        if (_tags.Remove(NormalizeTag(tag)))
        {
            Touch();
        }
    }

    public void SubmitForReview(string reviewer)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reviewer);

        Status = ContentLifecycleStatus.InReview;
        ReviewedBy = reviewer.Trim();
        ReviewedAtUtc = DateTime.UtcNow;
        Touch();
    }

    public void Publish()
    {
        if (Status == ContentLifecycleStatus.Published)
        {
            return;
        }

        Status = ContentLifecycleStatus.Published;
        PublishedAtUtc = DateTime.UtcNow;
        Touch();

        AddDomainEvent(new EditorialContentPublishedDomainEvent(Id, ContentType, PublishedAtUtc.Value));
    }

    public void Archive()
    {
        if (Status == ContentLifecycleStatus.Archived)
        {
            return;
        }

        Status = ContentLifecycleStatus.Archived;
        Touch();

        AddDomainEvent(new EditorialContentArchivedDomainEvent(Id, ContentType, UpdatedAt));
    }

    private EditorialContent(EditorialContentType contentType, string title, string slug, string summary, string body, string? specialty, string? species)
    {
        ContentType = contentType;
        Title = title;
        Slug = slug;
        Summary = summary;
        Body = body;
        Specialty = specialty;
        Species = species;
        Status = ContentLifecycleStatus.Draft;
    }

    private static EditorialContent Create(EditorialContentType contentType, string title, string slug, string summary, string body, string? specialty, string? species)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);
        ArgumentException.ThrowIfNullOrWhiteSpace(summary);
        ArgumentException.ThrowIfNullOrWhiteSpace(body);

        return new EditorialContent(contentType, title.Trim(), NormalizeSlug(slug), summary.Trim(), body.Trim(), specialty?.Trim(), species?.Trim());
    }

    private static string NormalizeSlug(string slug)
    {
        return slug.Trim().ToLowerInvariant();
    }

    private static string NormalizeTag(string tag)
    {
        return tag.Trim().ToLowerInvariant();
    }

    private void Touch()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}

public enum EditorialContentType
{
    Resource,
    Publication,
}

public enum ContentLifecycleStatus
{
    Draft,
    InReview,
    Published,
    Archived,
}

public sealed record EditorialContentPublishedDomainEvent(Guid ContentId, EditorialContentType ContentType, DateTime OccurredOn)
    : DomainEvent(OccurredOn);

public sealed record EditorialContentArchivedDomainEvent(Guid ContentId, EditorialContentType ContentType, DateTime OccurredOn)
    : DomainEvent(OccurredOn);
using SharedKernel.Domain;

namespace Baytic.Domain.Portal;

public sealed class MemberProfile : BaseEntity
{
    private readonly List<SavedResource> _savedResources = [];
    private readonly List<SavedBlogPost> _savedBlogPosts = [];

    public string MemberId { get; private set; } = string.Empty;

    public string DisplayName { get; private set; } = string.Empty;

    public string? Bio { get; private set; }

    public string? City { get; private set; }

    public string? Country { get; private set; }

    public MemberProfileStatus Status { get; private set; }

    public IReadOnlyCollection<SavedResource> SavedResources => _savedResources.AsReadOnly();

    public IReadOnlyCollection<SavedBlogPost> SavedBlogPosts => _savedBlogPosts.AsReadOnly();

    public static MemberProfile Create(string memberId, string displayName, string? bio = null, string? city = null, string? country = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(memberId);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);

        var profile = new MemberProfile(memberId.Trim(), displayName.Trim(), bio?.Trim(), city?.Trim(), country?.Trim());
        profile.AddDomainEvent(new MemberProfileCreatedDomainEvent(profile.Id, profile.MemberId, DateTime.UtcNow));
        return profile;
    }

    public void UpdateProfile(string displayName, string? bio = null, string? city = null, string? country = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);

        DisplayName = displayName.Trim();
        Bio = string.IsNullOrWhiteSpace(bio) ? null : bio.Trim();
        City = string.IsNullOrWhiteSpace(city) ? null : city.Trim();
        Country = string.IsNullOrWhiteSpace(country) ? null : country.Trim();
        Touch();

        AddDomainEvent(new MemberProfileUpdatedDomainEvent(Id, MemberId, UpdatedAt));
    }

    public void SaveResource(Guid resourceId, string title)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        if (_savedResources.Any(savedResource => savedResource.ResourceId == resourceId))
        {
            return;
        }

        _savedResources.Add(new SavedResource(resourceId, title.Trim(), DateTime.UtcNow));
        Touch();

        AddDomainEvent(new MemberSavedResourceDomainEvent(Id, MemberId, resourceId, UpdatedAt));
    }

    public void RemoveSavedResource(Guid resourceId)
    {
        if (_savedResources.RemoveAll(savedResource => savedResource.ResourceId == resourceId) == 0)
        {
            return;
        }

        Touch();
    }

    public void SaveBlogPost(Guid blogPostId, string title)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        if (_savedBlogPosts.Any(savedBlogPost => savedBlogPost.BlogPostId == blogPostId))
        {
            return;
        }

        _savedBlogPosts.Add(new SavedBlogPost(blogPostId, title.Trim(), DateTime.UtcNow));
        Touch();
    }

    public void RemoveSavedBlogPost(Guid blogPostId)
    {
        if (_savedBlogPosts.RemoveAll(savedBlogPost => savedBlogPost.BlogPostId == blogPostId) == 0)
        {
            return;
        }

        Touch();
    }

    public void Suspend()
    {
        if (Status == MemberProfileStatus.Suspended)
        {
            return;
        }

        Status = MemberProfileStatus.Suspended;
        Touch();
    }

    public void Reinstate()
    {
        if (Status == MemberProfileStatus.Active)
        {
            return;
        }

        Status = MemberProfileStatus.Active;
        Touch();
    }

    private MemberProfile(string memberId, string displayName, string? bio, string? city, string? country)
    {
        MemberId = memberId;
        DisplayName = displayName;
        Bio = bio;
        City = city;
        Country = country;
        Status = MemberProfileStatus.Active;
    }

    private void Touch()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}

public enum MemberProfileStatus
{
    Active,
    Suspended,
}

public sealed record SavedResource(Guid ResourceId, string Title, DateTime SavedAtUtc);

public sealed record SavedBlogPost(Guid BlogPostId, string Title, DateTime SavedAtUtc);

public sealed record MemberProfileCreatedDomainEvent(Guid MemberProfileId, string MemberId, DateTime OccurredOn)
    : DomainEvent(OccurredOn);

public sealed record MemberProfileUpdatedDomainEvent(Guid MemberProfileId, string MemberId, DateTime OccurredOn)
    : DomainEvent(OccurredOn);

public sealed record MemberSavedResourceDomainEvent(Guid MemberProfileId, string MemberId, Guid ResourceId, DateTime OccurredOn)
    : DomainEvent(OccurredOn);
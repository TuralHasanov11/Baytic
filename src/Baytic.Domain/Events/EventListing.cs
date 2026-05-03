using SharedKernel.Domain;

namespace Baytic.Domain.Events;

public sealed class EventListing : BaseEntity
{
    private readonly List<EventRegistration> _registrations = [];

    public string Title { get; private set; } = string.Empty;

    public string Slug { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public DateTimeOffset StartsAt { get; private set; }

    public DateTimeOffset EndsAt { get; private set; }

    public string Location { get; private set; } = string.Empty;

    public bool IsVirtual { get; private set; }

    public EventListingStatus Status { get; private set; }

    public IReadOnlyCollection<EventRegistration> Registrations => _registrations.AsReadOnly();

    public static EventListing Create(string title, string slug, string description, DateTimeOffset startsAt, DateTimeOffset endsAt, string location, bool isVirtual)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        ArgumentException.ThrowIfNullOrWhiteSpace(location);

        if (endsAt <= startsAt)
        {
            throw new ArgumentException("The end date must be after the start date.", nameof(endsAt));
        }

        return new EventListing(title.Trim(), NormalizeSlug(slug), description.Trim(), startsAt, endsAt, location.Trim(), isVirtual);
    }

    public void UpdateDetails(string title, string description, DateTimeOffset startsAt, DateTimeOffset endsAt, string location, bool isVirtual)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        ArgumentException.ThrowIfNullOrWhiteSpace(location);

        if (endsAt <= startsAt)
        {
            throw new ArgumentException("The end date must be after the start date.", nameof(endsAt));
        }

        Title = title.Trim();
        Description = description.Trim();
        StartsAt = startsAt;
        EndsAt = endsAt;
        Location = location.Trim();
        IsVirtual = isVirtual;
        Touch();
    }

    public void Publish()
    {
        if (Status == EventListingStatus.Published)
        {
            return;
        }

        Status = EventListingStatus.Published;
        Touch();
    }

    public void OpenRegistration()
    {
        Status = EventListingStatus.OpenForRegistration;
        Touch();
    }

    public Guid Register(string memberId, string memberDisplayName, string? memberEmail = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(memberId);
        ArgumentException.ThrowIfNullOrWhiteSpace(memberDisplayName);

        if (_registrations.Any(registration => registration.MemberId == memberId.Trim()))
        {
            throw new InvalidOperationException("Member is already registered for this event.");
        }

        var registration = new EventRegistration(Guid.NewGuid(), memberId.Trim(), memberDisplayName.Trim(), string.IsNullOrWhiteSpace(memberEmail) ? null : memberEmail.Trim(), DateTime.UtcNow, EventRegistrationStatus.Confirmed);
        _registrations.Add(registration);
        Touch();
        return registration.RegistrationId;
    }

    public void CancelRegistration(Guid registrationId)
    {
        var registrationIndex = _registrations.FindIndex(registration => registration.RegistrationId == registrationId);
        if (registrationIndex < 0)
        {
            return;
        }

        var registration = _registrations[registrationIndex];
        _registrations[registrationIndex] = registration with { Status = EventRegistrationStatus.Cancelled };
        Touch();
    }

    public void CloseRegistration()
    {
        if (Status == EventListingStatus.Closed)
        {
            return;
        }

        Status = EventListingStatus.Closed;
        Touch();
    }

    private EventListing(string title, string slug, string description, DateTimeOffset startsAt, DateTimeOffset endsAt, string location, bool isVirtual)
    {
        Title = title;
        Slug = slug;
        Description = description;
        StartsAt = startsAt;
        EndsAt = endsAt;
        Location = location;
        IsVirtual = isVirtual;
        Status = EventListingStatus.Draft;
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

public enum EventListingStatus
{
    Draft,
    OpenForRegistration,
    Published,
    Closed,
}

public enum EventRegistrationStatus
{
    Confirmed,
    Cancelled,
}

public sealed record EventRegistration(Guid RegistrationId, string MemberId, string MemberDisplayName, string? MemberEmail, DateTime RegisteredAtUtc, EventRegistrationStatus Status);
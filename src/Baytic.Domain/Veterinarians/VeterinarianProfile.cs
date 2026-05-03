using Baytic.Domain.Identity;
using SharedKernel.Domain;

namespace Baytic.Domain.Veterinarians;

public sealed class VeterinarianProfile : BaseEntity
{
    private readonly List<VeterinarianCredential> _credentials = [];
    private readonly List<string> _expertiseTags = [];
    private readonly List<VeterinarianProfileLink> _profileLinks = [];

    public UserId UserId { get; }

    public string DisplayName { get; private set; }

    public string? Biography { get; private set; }

    public string Location { get; private set; }

    public int YearsOfExperience { get; private set; }

    public VeterinarianVerificationStatus VerificationStatus { get; private set; }

    public bool IsFeatured { get; private set; }

    public IReadOnlyCollection<VeterinarianCredential> Credentials => _credentials.AsReadOnly();

    public IReadOnlyCollection<string> ExpertiseTags => _expertiseTags.AsReadOnly();

    public IReadOnlyCollection<VeterinarianProfileLink> ProfileLinks => _profileLinks.AsReadOnly();

    public static VeterinarianProfile Register(UserId userId, string displayName, string location, int yearsOfExperience, string? biography = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        ArgumentException.ThrowIfNullOrWhiteSpace(location);

        if (yearsOfExperience < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(yearsOfExperience), yearsOfExperience, "Years of experience cannot be negative.");
        }

        return new VeterinarianProfile(userId, displayName.Trim(), location.Trim(), yearsOfExperience, biography?.Trim());
    }

    public void UpdateProfile(string displayName, string location, int yearsOfExperience, string? biography = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        ArgumentException.ThrowIfNullOrWhiteSpace(location);

        if (yearsOfExperience < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(yearsOfExperience), yearsOfExperience, "Years of experience cannot be negative.");
        }

        DisplayName = displayName.Trim();
        Location = location.Trim();
        YearsOfExperience = yearsOfExperience;
        Biography = string.IsNullOrWhiteSpace(biography) ? null : biography.Trim();
        Touch();
    }

    public Guid AddCredential(string credentialName, string issuer, DateOnly issuedOn, DateOnly? expiresOn = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(credentialName);
        ArgumentException.ThrowIfNullOrWhiteSpace(issuer);

        var credential = new VeterinarianCredential(Guid.NewGuid(), credentialName.Trim(), issuer.Trim(), issuedOn, expiresOn, false, null, null);
        _credentials.Add(credential);
        Touch();
        return credential.CredentialId;
    }

    public void VerifyCredential(Guid credentialId, string verifiedBy)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(verifiedBy);

        var credentialIndex = _credentials.FindIndex(credential => credential.CredentialId == credentialId);
        if (credentialIndex < 0)
        {
            return;
        }

        var credential = _credentials[credentialIndex];
        _credentials[credentialIndex] = credential with
        {
            IsVerified = true,
            VerifiedBy = verifiedBy.Trim(),
            VerifiedAtUtc = DateTime.UtcNow,
        };

        VerificationStatus = VeterinarianVerificationStatus.Verified;
        Touch();

        AddDomainEvent(new VeterinarianCredentialVerifiedDomainEvent(Id, UserId, credentialId, UpdatedAt));
    }

    public void AddExpertiseTag(string tag)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tag);

        var normalizedTag = tag.Trim().ToLowerInvariant();
        if (_expertiseTags.Contains(normalizedTag))
        {
            return;
        }

        _expertiseTags.Add(normalizedTag);
        Touch();
    }

    public void RemoveExpertiseTag(string tag)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tag);

        if (_expertiseTags.Remove(tag.Trim().ToLowerInvariant()))
        {
            Touch();
        }
    }

    public Guid AddProfileLink(string label, string url)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(label);
        ArgumentException.ThrowIfNullOrWhiteSpace(url);

        var link = new VeterinarianProfileLink(Guid.NewGuid(), label.Trim(), url.Trim());
        _profileLinks.Add(link);
        Touch();
        return link.LinkId;
    }

    public void Feature()
    {
        if (IsFeatured)
        {
            return;
        }

        IsFeatured = true;
        Touch();
    }

    public void Unfeature()
    {
        if (!IsFeatured)
        {
            return;
        }

        IsFeatured = false;
        Touch();
    }

    public void Suspend()
    {
        VerificationStatus = VeterinarianVerificationStatus.Suspended;
        Touch();
    }

    private VeterinarianProfile(UserId userId, string displayName, string location, int yearsOfExperience, string? biography)
    {
        UserId = userId;
        DisplayName = displayName;
        Location = location;
        YearsOfExperience = yearsOfExperience;
        Biography = biography;
        VerificationStatus = VeterinarianVerificationStatus.Pending;
    }

    private void Touch()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}

public enum VeterinarianVerificationStatus
{
    Pending,
    Verified,
    Suspended,
}

public sealed record VeterinarianCredential(Guid CredentialId, string CredentialName, string Issuer, DateOnly IssuedOn, DateOnly? ExpiresOn, bool IsVerified, string? VerifiedBy, DateTime? VerifiedAtUtc);

public sealed record VeterinarianProfileLink(Guid LinkId, string Label, string Url);

public sealed record VeterinarianCredentialVerifiedDomainEvent(
    Guid VeterinarianProfileId, 
    UserId UserId, 
    Guid CredentialId, 
    DateTime OccurredOn)
    : DomainEvent(OccurredOn);
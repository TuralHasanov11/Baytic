using SharedKernel.Domain;

namespace Baytic.Domain.Cases;

public sealed class CaseStudy : BaseEntity
{
    private readonly List<CaseReview> _reviews = [];
    private readonly List<CaseComment> _comments = [];

    public string ContributorId { get; private set; } = string.Empty;

    public string Title { get; private set; } = string.Empty;

    public string Slug { get; private set; } = string.Empty;

    public string Summary { get; private set; } = string.Empty;

    public string Specialty { get; private set; } = string.Empty;

    public string? Species { get; private set; }

    public CaseStudyStatus Status { get; private set; }

    public IReadOnlyCollection<CaseReview> Reviews => _reviews.AsReadOnly();

    public IReadOnlyCollection<CaseComment> Comments => _comments.AsReadOnly();

    public static CaseStudy CreateDraft(string contributorId, string title, string slug, string summary, string specialty, string? species = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contributorId);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);
        ArgumentException.ThrowIfNullOrWhiteSpace(summary);
        ArgumentException.ThrowIfNullOrWhiteSpace(specialty);

        return new CaseStudy(contributorId.Trim(), title.Trim(), NormalizeSlug(slug), summary.Trim(), specialty.Trim(), species?.Trim());
    }

    public void SubmitForReview()
    {
        if (Status == CaseStudyStatus.Submitted)
        {
            return;
        }

        Status = CaseStudyStatus.Submitted;
        Touch();
    }

    public void Publish()
    {
        Status = CaseStudyStatus.Published;
        Touch();

        AddDomainEvent(new CaseStudyPublishedDomainEvent(Id, ContributorId, UpdatedAt));
    }

    public Guid AddReview(string reviewerId, string reviewBody, CaseReviewDecision decision)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reviewerId);
        ArgumentException.ThrowIfNullOrWhiteSpace(reviewBody);

        var review = new CaseReview(Guid.NewGuid(), reviewerId.Trim(), reviewBody.Trim(), decision, DateTime.UtcNow);
        _reviews.Add(review);
        Touch();
        return review.ReviewId;
    }

    public Guid AddComment(string memberId, string memberDisplayName, string body)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(memberId);
        ArgumentException.ThrowIfNullOrWhiteSpace(memberDisplayName);
        ArgumentException.ThrowIfNullOrWhiteSpace(body);

        var comment = new CaseComment(Guid.NewGuid(), memberId.Trim(), memberDisplayName.Trim(), body.Trim(), DateTime.UtcNow);
        _comments.Add(comment);
        Touch();
        return comment.CommentId;
    }

    public void Archive()
    {
        Status = CaseStudyStatus.Archived;
        Touch();
    }

    private CaseStudy(string contributorId, string title, string slug, string summary, string specialty, string? species)
    {
        ContributorId = contributorId;
        Title = title;
        Slug = slug;
        Summary = summary;
        Specialty = specialty;
        Species = species;
        Status = CaseStudyStatus.Draft;
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

public enum CaseStudyStatus
{
    Draft,
    Submitted,
    Published,
    Archived,
}

public enum CaseReviewDecision
{
    Pending,
    Approved,
    NeedsChanges,
    Rejected,
}

public sealed record CaseReview(Guid ReviewId, string ReviewerId, string Body, CaseReviewDecision Decision, DateTime ReviewedAtUtc);

public sealed record CaseComment(Guid CommentId, string MemberId, string MemberDisplayName, string Body, DateTime CreatedAtUtc);

public sealed record CaseStudyPublishedDomainEvent(Guid CaseStudyId, string ContributorId, DateTime OccurredOn)
    : DomainEvent(OccurredOn);
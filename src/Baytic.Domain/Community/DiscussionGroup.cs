using SharedKernel.Domain;

namespace Baytic.Domain.Community;

public sealed class DiscussionGroup : BaseEntity
{
    private readonly List<GroupMember> _members = [];
    private readonly List<GroupMessage> _messages = [];

    public string Title { get; private set; } = string.Empty;

    public string Slug { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public string Topic { get; private set; } = string.Empty;

    public GroupAccessLevel AccessLevel { get; private set; }

    public IReadOnlyCollection<GroupMember> Members => _members.AsReadOnly();

    public IReadOnlyCollection<GroupMessage> Messages => _messages.AsReadOnly();

    public static DiscussionGroup Create(string title, string slug, string description, string topic, GroupAccessLevel accessLevel)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        ArgumentException.ThrowIfNullOrWhiteSpace(topic);

        return new DiscussionGroup(title.Trim(), NormalizeSlug(slug), description.Trim(), topic.Trim(), accessLevel);
    }

    public void Update(string title, string description, string topic, GroupAccessLevel accessLevel)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        ArgumentException.ThrowIfNullOrWhiteSpace(topic);

        Title = title.Trim();
        Description = description.Trim();
        Topic = topic.Trim();
        AccessLevel = accessLevel;
        Touch();
    }

    public void Join(string memberId, string memberDisplayName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(memberId);
        ArgumentException.ThrowIfNullOrWhiteSpace(memberDisplayName);

        if (_members.Any(member => member.MemberId == memberId))
        {
            return;
        }

        _members.Add(new GroupMember(Guid.NewGuid(), memberId.Trim(), memberDisplayName.Trim(), DateTime.UtcNow));
        Touch();
    }

    public void Leave(string memberId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(memberId);

        if (_members.RemoveAll(member => member.MemberId == memberId.Trim()) == 0)
        {
            return;
        }

        Touch();
    }

    public Guid PostMessage(string memberId, string body)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(memberId);
        ArgumentException.ThrowIfNullOrWhiteSpace(body);

        if (_members.All(member => member.MemberId != memberId.Trim()))
        {
            throw new InvalidOperationException("Only members can post to the group.");
        }

        var message = new GroupMessage(Guid.NewGuid(), memberId.Trim(), body.Trim(), DateTime.UtcNow, false);
        _messages.Add(message);
        Touch();
        return message.MessageId;
    }

    public void ModerateMessage(Guid messageId)
    {
        var messageIndex = _messages.FindIndex(message => message.MessageId == messageId);
        if (messageIndex < 0)
        {
            return;
        }

        var message = _messages[messageIndex];
        _messages[messageIndex] = message with { IsModerated = true };
        Touch();
    }

    private DiscussionGroup(string title, string slug, string description, string topic, GroupAccessLevel accessLevel)
    {
        Title = title;
        Slug = slug;
        Description = description;
        Topic = topic;
        AccessLevel = accessLevel;
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

public enum GroupAccessLevel
{
    Public,
    MembersOnly,
    InviteOnly,
}

public sealed record GroupMember(Guid MemberIdValue, string MemberId, string MemberDisplayName, DateTime JoinedAtUtc);

public sealed record GroupMessage(Guid MessageId, string MemberId, string Body, DateTime CreatedAtUtc, bool IsModerated);
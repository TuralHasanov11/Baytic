using Baytic.Domain.Portal;

namespace Baytic.Domain.Tests;

public class MemberProfileTests
{
    [Fact]
    public void Create_ShouldInitializeActiveProfileAndRaiseCreatedEvent()
    {
        var profile = MemberProfile.Create("member-001", "Dr. Ada Lovelace", "Veterinary surgeon", "London", "UK");

        Assert.Equal("member-001", profile.MemberId);
        Assert.Equal(MemberProfileStatus.Active, profile.Status);
        Assert.Single(profile.DomainEvents);
        Assert.IsType<MemberProfileCreatedDomainEvent>(profile.DomainEvents.Single());
    }

    [Fact]
    public void SaveResource_ShouldAddResourceOnce()
    {
        var profile = MemberProfile.Create("member-002", "Dr. Grace Hopper");
        var resourceId = Guid.NewGuid();

        profile.SaveResource(resourceId, "Clinical protocol");
        profile.SaveResource(resourceId, "Clinical protocol");

        Assert.Single(profile.SavedResources);
        Assert.Equal(resourceId, profile.SavedResources.Single().ResourceId);
    }
}
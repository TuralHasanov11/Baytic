using Baytic.Domain.Events;

namespace Baytic.Domain.Tests;

public class EventListingTests
{
    [Fact]
    public void Register_ShouldPreventDuplicateRegistrations()
    {
        var eventListing = EventListing.Create(
            "Annual conference",
            "annual-conference",
            "Conference description",
            DateTimeOffset.UtcNow.AddDays(10),
            DateTimeOffset.UtcNow.AddDays(10).AddHours(2),
            "Virtual",
            true);

        eventListing.OpenRegistration();
        eventListing.Register("member-1", "Member One");

        Assert.Throws<InvalidOperationException>(() => eventListing.Register("member-1", "Member One"));
        Assert.Single(eventListing.Registrations);
    }
}
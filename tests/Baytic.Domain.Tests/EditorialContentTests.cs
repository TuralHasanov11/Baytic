using Baytic.Domain.Content;

namespace Baytic.Domain.Tests;

public class EditorialContentTests
{
    [Fact]
    public void CreateResource_ShouldNormalizeSlugAndStartAsDraft()
    {
        var content = EditorialContent.CreateResource("Small Animal Protocols", " Small-Animal-Protocols ", "Guidance", "Body text");

        Assert.Equal("small-animal-protocols", content.Slug);
        Assert.Equal(ContentLifecycleStatus.Draft, content.Status);
        Assert.Equal(EditorialContentType.Resource, content.ContentType);
    }

    [Fact]
    public void Publish_ShouldSetPublishedStateAndRaiseEvent()
    {
        var content = EditorialContent.CreatePublication("Annual Review", "annual-review", "Summary", "Body text");

        content.Publish();

        Assert.Equal(ContentLifecycleStatus.Published, content.Status);
        Assert.Single(content.DomainEvents);
        Assert.IsType<EditorialContentPublishedDomainEvent>(content.DomainEvents.Single());
    }
}
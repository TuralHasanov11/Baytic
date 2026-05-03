using Baytic.Domain.Blog;

namespace Baytic.Domain.Tests;

public class BlogPostTests
{
    [Fact]
    public void AddComment_ShouldAppendCommentToAggregate()
    {
        var post = BlogPost.CreateDraft("author-1", "Dr. Author", "Vet blog", "vet-blog", "Excerpt", "Body text", 4);

        var commentId = post.AddComment("member-1", "Member One", "Helpful article");

        Assert.Single(post.Comments);
        Assert.Equal(commentId, post.Comments.Single().CommentId);
    }

    [Fact]
    public void Publish_ShouldChangeStateAndRaiseEvent()
    {
        var post = BlogPost.CreateDraft("author-2", "Dr. Writer", "Vet blog 2", "vet-blog-2", "Excerpt", "Body text", 5);

        post.Publish(DateTime.UtcNow);

        Assert.Equal(BlogPostStatus.Published, post.Status);
        Assert.Single(post.DomainEvents);
        Assert.IsType<BlogPostPublishedDomainEvent>(post.DomainEvents.Single());
    }
}
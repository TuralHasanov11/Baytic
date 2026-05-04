using Baytic.Domain.Blog;

namespace Baytic.Application.Blog;

public sealed record BlogPostResponse(
    Guid Id,
    Guid AuthorId,
    string Title,
    string Slug,
    string Body,
    BlogPostStatus Status,
    DateTime? PublishedAt,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    public static BlogPostResponse From(BlogPost blogPost)
    {
        return new BlogPostResponse(
            blogPost.Id,
            blogPost.AuthorId.Value,
            blogPost.Title,
            blogPost.Slug,
            blogPost.Body,
            blogPost.Status,
            blogPost.PublishedAt,
            blogPost.CreatedAt,
            blogPost.UpdatedAt);
    }
}

public sealed record CreateBlogPostRequest(string Title, string Slug, string Body);

public sealed record UpdateBlogPostRequest(string Title, string Slug, string Body);
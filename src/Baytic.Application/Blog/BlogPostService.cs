using Baytic.Domain.Blog;
using Baytic.Domain.Identity;
using Baytic.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Baytic.Application.Blog;

public sealed class BlogPostService(ApplicationDbContext dbContext)
{
    public async Task<IReadOnlyList<BlogPostResponse>> ListAsync(BlogPostStatus? status = null, CancellationToken cancellationToken = default)
    {
        IQueryable<BlogPost> query = dbContext.BlogPosts.AsNoTracking();

        if (status is not null)
        {
            query = query.Where(blogPost => blogPost.Status == status);
        }

        var blogPosts = await query
            .OrderByDescending(blogPost => blogPost.PublishedAt ?? blogPost.CreatedAt)
            .ThenByDescending(blogPost => blogPost.CreatedAt)
            .ToListAsync(cancellationToken);

        return blogPosts.Select(BlogPostResponse.From).ToList();
    }

    public async Task<BlogPostResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var blogPost = await dbContext.BlogPosts
            .AsNoTracking()
            .FirstOrDefaultAsync(blogPost => blogPost.Id == id, cancellationToken);

        return blogPost is null ? null : BlogPostResponse.From(blogPost);
    }

    public async Task<BlogPostResponse> CreateDraftAsync(UserId authorId, CreateBlogPostRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var blogPost = BlogPost.CreateDraft(authorId, request.Title, request.Slug, request.Body);

        dbContext.BlogPosts.Add(blogPost);
        await dbContext.SaveChangesAsync(cancellationToken);

        return BlogPostResponse.From(blogPost);
    }

    public async Task<BlogPostResponse?> UpdateDraftAsync(Guid id, UpdateBlogPostRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var blogPost = await dbContext.BlogPosts.FirstOrDefaultAsync(blogPost => blogPost.Id == id, cancellationToken);
        if (blogPost is null)
        {
            return null;
        }

        blogPost.UpdateDraft(request.Title, request.Slug, request.Body);
        await dbContext.SaveChangesAsync(cancellationToken);

        return BlogPostResponse.From(blogPost);
    }

    public async Task<BlogPostResponse?> PublishAsync(Guid id, DateTime? publishedAtUtc = null, CancellationToken cancellationToken = default)
    {
        var blogPost = await dbContext.BlogPosts.FirstOrDefaultAsync(blogPost => blogPost.Id == id, cancellationToken);
        if (blogPost is null)
        {
            return null;
        }

        blogPost.Publish(publishedAtUtc ?? DateTime.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken);

        return BlogPostResponse.From(blogPost);
    }

    public async Task<BlogPostResponse?> ArchiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var blogPost = await dbContext.BlogPosts.FirstOrDefaultAsync(blogPost => blogPost.Id == id, cancellationToken);
        if (blogPost is null)
        {
            return null;
        }

        blogPost.Archive();
        await dbContext.SaveChangesAsync(cancellationToken);

        return BlogPostResponse.From(blogPost);
    }
}
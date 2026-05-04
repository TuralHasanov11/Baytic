using Baytic.Application.Blog;
using Baytic.Domain.Blog;
using Baytic.Infrastructure.Identity;

namespace Baytic.Api.Blog;

public static class BlogPostEndpoints
{
    public static IEndpointRouteBuilder MapBlogEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/blog")
            .WithTags("Blog");

        group.MapGet("", async (BlogPostService blogPostService, BlogPostStatus? status, CancellationToken cancellationToken) =>
            Results.Ok(await blogPostService.ListAsync(status, cancellationToken)))
            .AllowAnonymous();

        group.MapGet("{id:guid}", async (Guid id, BlogPostService blogPostService, CancellationToken cancellationToken) =>
            await blogPostService.GetByIdAsync(id, cancellationToken) is { } blogPost
                ? Results.Ok(blogPost)
                : Results.NotFound())
            .AllowAnonymous();

        group.MapPost("", async (HttpContext httpContext, CreateBlogPostRequest request, BlogPostService blogPostService, CancellationToken cancellationToken) =>
        {
            var author = httpContext.User.FromClaimsPrincipal();
            var created = await blogPostService.CreateDraftAsync(author.UserId, request, cancellationToken);

            return Results.Created($"/api/v1/blog/{created.Id}", created);
        });

        group.MapPut("{id:guid}", async (Guid id, UpdateBlogPostRequest request, BlogPostService blogPostService, CancellationToken cancellationToken) =>
            await blogPostService.UpdateDraftAsync(id, request, cancellationToken) is { } updated
                ? Results.Ok(updated)
                : Results.NotFound());

        group.MapPost("{id:guid}/publish", async (Guid id, BlogPostService blogPostService, CancellationToken cancellationToken) =>
            await blogPostService.PublishAsync(id, cancellationToken: cancellationToken) is { } published
                ? Results.Ok(published)
                : Results.NotFound());

        group.MapPost("{id:guid}/archive", async (Guid id, BlogPostService blogPostService, CancellationToken cancellationToken) =>
            await blogPostService.ArchiveAsync(id, cancellationToken) is { } archived
                ? Results.Ok(archived)
                : Results.NotFound());

        return app;
    }
}
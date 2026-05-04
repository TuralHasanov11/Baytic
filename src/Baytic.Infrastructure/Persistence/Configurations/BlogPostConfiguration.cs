using Baytic.Domain.Blog;
using Baytic.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Baytic.Infrastructure.Persistence.Configurations;

public sealed class BlogPostConfiguration : IEntityTypeConfiguration<BlogPost>
{
    public void Configure(EntityTypeBuilder<BlogPost> builder)
    {
        builder.ToTable("blog_posts");

        builder.HasKey(blogPost => blogPost.Id);
        builder.Property(blogPost => blogPost.Id).ValueGeneratedNever();

        builder.Property(blogPost => blogPost.AuthorId)
            .HasConversion(authorId => authorId.Value, value => new UserId(value))
            .HasColumnName("author_id")
            .IsRequired();

        builder.Property(blogPost => blogPost.Title)
            .HasColumnName("title")
            .HasMaxLength(500)
            .IsRequired();

        builder.HasIndex(blogPost => blogPost.Slug).IsUnique();

        builder.Property(blogPost => blogPost.Slug)
            .HasColumnName("slug")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(blogPost => blogPost.Body)
            .HasColumnName("body")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(blogPost => blogPost.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(blogPost => blogPost.PublishedAt)
            .HasColumnName("published_at")
            .HasColumnType("timestamp with time zone");

        builder.Property(blogPost => blogPost.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone");

        builder.Property(blogPost => blogPost.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone");
    }
}
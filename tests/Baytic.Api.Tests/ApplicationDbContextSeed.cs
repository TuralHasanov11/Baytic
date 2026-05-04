using Baytic.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;

namespace Baytic.Api.Tests;

internal class ApplicationDbContextSeed
{
    internal static void SeedAsync(ApplicationDbContext db, ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger<ApplicationDbContextSeed>();

        try
        {
            if (!db.BlogPosts.Any())
            {
                logger.LogInformation("Seeding database with test blog posts...");

                db.BlogPosts.AddRange(
                // TODO: Add realistic test data here
                );

                db.SaveChanges();
                logger.LogInformation("Database seeding completed.");
            }
            else
            {
                logger.LogInformation("Database already contains blog posts. Skipping seeding.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database: {Message}", ex.Message);
            throw; // Rethrow to ensure test setup fails if seeding fails
        }
    }
}
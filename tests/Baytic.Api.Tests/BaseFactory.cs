using Baytic.Api.Tests;
using Baytic.Infrastructure.Persistence;
using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Testcontainers.Keycloak;
using Testcontainers.PostgreSql;

[assembly: AssemblyFixture(typeof(BaseFactory))]

namespace Baytic.Api.Tests;

public class BaseFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string RealmName = "Baytic";

    public const string ClientId = "web-api";

    public const string ClientSecret = "your-client-secret-for-web-api";

    public const string RedirectUri = "http://localhost:5103/signin-oidc";

    public const string TokenUserName = "test-user";

    public const string TokenPassword = "password";

    private readonly KeycloakContainer _keycloakContainer = new KeycloakBuilder("quay.io/keycloak/keycloak:21.1")
        .WithRealm(Path.Combine(AppContext.BaseDirectory, "Baytic-realm.json"))
        .WithUsername("admin")
        .WithPassword("admin")
        .Build();

    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgres:latest")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithDatabase("catalog")
        .WithWaitStrategy(Wait.ForUnixContainer())
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Keycloak:Authority"] = new Uri(new Uri(_keycloakContainer.GetBaseAddress()), $"realms/{RealmName}").ToString(),
                ["Keycloak:ClientId"] = ClientId,
                ["Keycloak:ClientSecret"] = ClientSecret,
                ["Keycloak:RedirectUri"] = RedirectUri,
                ["Keycloak:Username"] = TokenUserName,
                ["Keycloak:Password"] = TokenPassword,
            });
        });

        builder.ConfigureServices(services =>
        {
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IDbContextOptionsConfiguration<ApplicationDbContext>));

            if (dbContextDescriptor != null)
            {
                services.Remove(dbContextDescriptor);
            }

            services.AddDbContextPool<ApplicationDbContext>((sp, options) =>
            {
                options.UseNpgsql(
                    _dbContainer.GetConnectionString(),
                    npgsqlOptionsAction => npgsqlOptionsAction.MigrationsHistoryTable(
                        HistoryRepository.DefaultTableName));
            });

            // Build the service provider.
            var sp = services.BuildServiceProvider();

            // Create a scope to obtain a reference to the database
            // context (ApplicationDbContext).
            using (var scope = sp.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<ApplicationDbContext>();
                var loggerFactory = scopedServices.GetRequiredService<ILoggerFactory>();

                var logger = scopedServices
                    .GetRequiredService<ILogger<BaseFactory>>();

                db.Database.Migrate();

                try
                {
                    // Seed the database with test data.
                    ApplicationDbContextSeed.SeedAsync(db, loggerFactory);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred seeding the database with test messages. Error: {Message}", ex.Message);
                }
            }
        });

        builder.UseEnvironment("Development");
    }

    public async ValueTask InitializeAsync()
    {
        try
        {
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<BaseFactory>();

            logger.LogInformation("Starting Keycloak container...");
            await _keycloakContainer.StartAsync();
            logger.LogInformation("Keycloak started at {BaseAddress}", _keycloakContainer.GetBaseAddress());

            logger.LogInformation("Starting PostgreSQL container...");
            await _dbContainer.StartAsync();
            logger.LogInformation("PostgreSQL connection: {ConnectionString}", _dbContainer.GetConnectionString());
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to initialize test containers", ex);
        }
    }

    public new async Task DisposeAsync()
    {
        try
        {
            if (_keycloakContainer is not null)
            {
                await _keycloakContainer.StopAsync();
                await _keycloakContainer.DisposeAsync();
            }

            if (_dbContainer is not null)
            {
                await _dbContainer.StopAsync();
                await _dbContainer.DisposeAsync();
            }

            await base.DisposeAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error during test container cleanup", ex);
        }
    }
}

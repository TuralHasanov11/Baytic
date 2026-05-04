using Baytic.Api.Blog;
using Baytic.Api;
using Baytic.Api.Identity;
using Baytic.Application.Blog;
using Baytic.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseDefaultServiceProvider(config => config.ValidateOnBuild = true);
builder.WebHost.UseKestrel(options => options.AddServerHeader = false);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

var connectionString = builder.Configuration.GetConnectionString("Baytic")
    ?? throw new InvalidOperationException("Connection string 'Baytic' was not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddScoped<BlogPostService>();

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();

    var keycloakOptions = app.Services.GetRequiredService<IOptions<KeycloakOptions>>().Value;

    app.MapScalarApiReference(
        options => options
            .AddPreferredSecuritySchemes("Keycloak", "KeycloakSelf")
            .AddHttpAuthentication("Keycloak", flow =>
            {
                flow.Description = "Keycloak Authentication";
                flow.Token = "";
            })
            .AddAuthorizationCodeFlow("KeycloakSelf", flow =>
            {
                flow.ClientId = keycloakOptions.ClientId;
                flow.ClientSecret = keycloakOptions.ClientSecret;
                flow.RedirectUri = keycloakOptions.RedirectUri;
                flow.Pkce = Pkce.Sha256;
            }))
        .AllowAnonymous();
}

app.UseHttpsRedirection();

app.MapBlogEndpoints();
app.MapDefaultEndpoints();

await app.RunAsync();

using Baytic.Api;
using Baytic.Api.Identity;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseDefaultServiceProvider(config => config.ValidateOnBuild = true);
builder.WebHost.UseKestrel(options => options.AddServerHeader = false);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

var app = builder.Build();

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

app.MapDefaultEndpoints();

await app.RunAsync();

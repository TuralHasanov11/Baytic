using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

namespace Baytic.Api.OpenApi;

public class InfoTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(
        OpenApiDocument document, 
        OpenApiDocumentTransformerContext context, 
        CancellationToken cancellationToken)
    {
        var openApiInfo = context.ApplicationServices.GetRequiredService<IOptions<OpenApiInfo>>();

        document.Info = openApiInfo.Value;

        return Task.CompletedTask;
    }
}
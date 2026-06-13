using Scalar.AspNetCore;

namespace FinanceTracker.Api.Extensions;

public static class OpenApiExtensions
{
    public static IServiceCollection AddOpenApiWithJwtSecurity(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, _, _) =>
            {
                var info = document.Info ?? new Microsoft.OpenApi.OpenApiInfo();
                info.Title = "Finance Tracker Api";
                info.Description = "This is a finance tracker portfolio project";
                info.Contact = new Microsoft.OpenApi.OpenApiContact
                {
                    Name = "Valentyn Matvieienko",
                    Email = "xanaramus@gmail.com",
                    Url = new Uri("https://www.linkedin.com/in/valentyn-matveenko/")
                };
                info.License = new Microsoft.OpenApi.OpenApiLicense
                {
                    Name = "MIT License",
                    Url = new Uri("https://opensource.org/licenses/MIT")
                };
                document.Info = info;
                document.ExternalDocs = new Microsoft.OpenApi.OpenApiExternalDocs
                {
                    Description = "GitHub Repository",
                    Url = new Uri("https://github.com/vailshnast/finance-tracker-portfolio")
                };

                var components = document.Components ?? new Microsoft.OpenApi.OpenApiComponents();
                components.SecuritySchemes ??= new Dictionary<string, Microsoft.OpenApi.IOpenApiSecurityScheme>();
                components.SecuritySchemes["Bearer"] = new Microsoft.OpenApi.OpenApiSecurityScheme
                {
                    Type = Microsoft.OpenApi.SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "Enter your JWT token"
                };

                document.Components = components;

                var schemeReference = new Microsoft.OpenApi.OpenApiSecuritySchemeReference("Bearer");
                var securityRequirement = new Microsoft.OpenApi.OpenApiSecurityRequirement
                {
                    [schemeReference] = new List<string>()
                };

                document.Security ??= [];
                document.Security.Add(securityRequirement);
                return Task.CompletedTask;
            });
        });

        return services;
    }

    public static WebApplication MapOpenApiEndpoints(this WebApplication app)
    {
        app.MapOpenApi();
        app.MapScalarApiReference(options =>
        {
            options.WithTitle("Finance Tracker Api");
            options.WithTheme(ScalarTheme.BluePlanet);
            options.WithDefaultHttpClient(ScalarTarget.Shell, ScalarClient.Curl);
        });

        return app;
    }
}

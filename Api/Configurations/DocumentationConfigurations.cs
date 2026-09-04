using Microsoft.OpenApi.Models;

namespace Api.Configurations;

public static class DocumentationConfigurations
{
    public static IServiceCollection AddDocumentationConfiguration(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "High Performance Orders API",
                Version = "v1",
                Description = "API de estudo focada em performance de queries SQL Server, execution plan e otimização com EF Core."
            });

            var xmlFile = $"{typeof(Program).Assembly.GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

            if (File.Exists(xmlPath))
                options.IncludeXmlComments(xmlPath);
        });

        return services;
    }

    public static WebApplication UseDocumentationConfiguration(this WebApplication app)
    {
        app.UseSwagger();

        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "High Performance Orders API v1");
            options.DefaultModelsExpandDepth(-1);
            options.DisplayRequestDuration();
            options.EnableFilter();
        });

        return app;
    }
}

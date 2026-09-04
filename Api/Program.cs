using Api.Configurations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDocumentationConfiguration();
builder.Services.AddDependencyInjectionConfiguration(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDocumentationConfiguration();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

// Necessário para o WebApplicationFactory<Program> em testes de integração acessar este tipo
// (top-level statements geram uma classe Program interna por padrão).
public partial class Program { }

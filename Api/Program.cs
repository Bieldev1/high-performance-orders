using Api.Configurations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDocumentationConfiguration();
builder.Services.AddDependencyInjectionConfiguration(builder.Configuration);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDocumentationConfiguration();
}

// Sem UseHttpsRedirection: a API roda atrás de um proxy/ingress (Docker) que faz a
// terminação TLS. Redirect de HTTPS aqui só gera o warning "Failed to determine the
// https port" e não agrega em container.

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");

app.Run();

// Necessário para o WebApplicationFactory<Program> em testes de integração acessar este tipo
// (top-level statements geram uma classe Program interna por padrão).
public partial class Program { }

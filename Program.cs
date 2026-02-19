using Microsoft.VisualBasic;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(configure =>
{
  configure.DocumentName = "TodoApi";
  configure.Title = "Todo API v1";
  configure.Version = "v1";
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
  app.UseOpenApi();
  app.UseSwaggerUi();
}

app.MapGet("/", () => "Hello World!");

app.Run();

using SmartDocs.Api.IServices;
using SmartDocs.Api.Services;
using SmartDocs.Api.Services.SmartDocs.Api.Services;
using SQLitePCL;

var builder = WebApplication.CreateBuilder(args);
Batteries.Init();
// Permitir requisições do Angular
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Adiciona controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<PdfTextExtractorService>();
builder.Services.AddSingleton<AIService>();
builder.Services.AddScoped<TextChunkerService>();
builder.Services.AddSingleton<IVectorStore, SqliteVectorStore>();

var app = builder.Build();

// Middleware de dev
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseAuthorization();

app.MapControllers();

await app.RunAsync();

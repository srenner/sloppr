using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using sloppr.AI.Extraction;
using sloppr.DataAccess;
using sloppr.Mappers;
using sloppr.Settings;
using sloppr.Services;

var client = new OllamaModelClient("granite4.1:3b");
var evaluator = new ExtractionEvaluator(client);
await evaluator.RunAsync();


var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ProviderTypeSettings>(
    builder.Configuration.GetSection("ProviderTypeSettings"));

builder.Services.Configure<AISettings>(
    builder.Configuration.GetSection("AI"));

builder.Services.AddScoped<ChatService>();

builder.Services.AddControllers();

builder.Services.AddScoped<IKeyIngredientService, KeyIngredientService>();
builder.Services.AddScoped<KeyIngredientMapper>();

builder.Services.AddScoped<IAiProviderService, AiProviderService>();
builder.Services.AddScoped<AiProviderMapper>();

builder.Services.AddScoped<IAiModelService, AiModelService>();
builder.Services.AddScoped<AiModelMapper>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

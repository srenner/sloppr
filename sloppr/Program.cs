using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using sloppr.AI.Extraction;
using sloppr.DataAccess;
using sloppr.Mappers;
using sloppr.Settings;
using sloppr.Services;
using sloppr.AI;
using Microsoft.AspNetCore.Http.Json;
using System.Text.Json.Serialization;

var client = new OllamaModelClient("granite4.1:3b");
var evaluator = new ExtractionEvaluator(client);
await evaluator.RunAsync();


var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddHttpClient();
builder.Services.AddHttpClient("health", client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.Configure<ProviderTypeSettings>(
    builder.Configuration.GetSection("ProviderTypeSettings"));

builder.Services.Configure<AISettings>(
    builder.Configuration.GetSection("AI"));

builder.Services.AddControllers();

builder.Services.AddScoped<ChatService>();

builder.Services.AddSingleton<IChatClientFactory, ChatClientFactory>();

builder.Services.AddScoped<IModelDiscoveryService, ModelDiscoveryService>();

builder.Services.AddScoped<IKeyIngredientService, KeyIngredientService>();
builder.Services.AddScoped<KeyIngredientMapper>();

builder.Services.AddScoped<IAiProviderService, AiProviderService>();
builder.Services.AddScoped<AiProviderMapper>();

builder.Services.AddScoped<IAiModelService, AiModelService>();
builder.Services.AddScoped<AiModelMapper>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowDevFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200") // Angular dev
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});


builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.UseDeveloperExceptionPage();
    app.UseCors("AllowDevFrontend");
}
else
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.MapControllers();

app.Run();

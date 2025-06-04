using System.Text.Json.Serialization;
using HevySync.Configuration;
using HevySync.Endpoints.Average2Savage;
using HevySync.Endpoints.Average2Savage.Converters;
using HevySync.Endpoints.Hevy;
using HevySync.Identity;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthorization()
    .AddIdentityServices()
    .AddDataServices(builder.Configuration)
    .AddEndpointsApiExplorer()
    .AddHevyApiClient(builder.Configuration)
    .AddSwagger()
    .AddCorsWithPolicy();

// Add custom JSON options with converters
builder.Services.Configure<JsonOptions>(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); // Enum converter
    options.JsonSerializerOptions.Converters.Add(new ExerciseDetailsRequestConverter()); // Custom converter
    options.JsonSerializerOptions.Converters.Add(new ExerciseDetailDtoConverter()); // Custom converter
});
var app = builder.Build();

app.UseCors();

app.MapIdentityApi<ApplicationUser>();

if (app.Environment.IsDevelopment()) app.UseSwagger().UseSwaggerUI();

app.MapGroup("/hevy").MapHevy();
app.MapGroup("/average2savage").MapAverage2Savage();
app.UseHttpsRedirection();
await app.RunPendingMigrations();
app.Run();

public partial class Program
{
}
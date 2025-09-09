using FluentValidation.AspNetCore;
using HevySync;
using HevySync.Configuration;
using HevySync.Endpoints.Average2Savage;
using HevySync.Endpoints.Hevy;
using HevySync.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthorization()
    .AddIdentityServices()
    .AddDomainServices()
    .AddDataServices(builder.Configuration)
    .AddHevyApiClient(builder.Configuration)
    .AddSwagger()
    .AddCorsWithPolicy()
    .AddEndpointsApiExplorer()
    .AddControllers().AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<AssemblyMarker>());

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
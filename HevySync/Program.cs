using FluentValidation.AspNetCore;
using HevySync;
using HevySync.Application;
using HevySync.Configuration;
using HevySync.Endpoints.Average2Savage;
using HevySync.Endpoints.Hevy;
using HevySync.Infrastructure;
using HevySync.Infrastructure.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthorization()
    .AddIdentityApiEndpoints<ApplicationUser>()
    .AddEntityFrameworkStores<HevySync.Infrastructure.Persistence.HevySyncDbContext>();

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddDomainServices()
    .AddHevyApiClient(builder.Configuration)
    .AddOptions(builder.Configuration)
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
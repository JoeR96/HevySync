using HevySync.Configuration;
using HevySync.Endpoints;
using HevySync.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthorization()
    .AddIdentityServices()
    .AddDataServices(builder.Configuration)
    .AddEndpointsApiExplorer()
    .AddHevyApiClient(builder.Configuration)
    .AddSwagger();

var app = builder.Build();

app.MapIdentityApi<ApplicationUser>();

if (app.Environment.IsDevelopment()) app.UseSwagger().UseSwaggerUI();

app.MapGroup("/hevy").MapHevy();

app.UseHttpsRedirection();

app.Run();
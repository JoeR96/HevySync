using HevySync.Configuration;
using HevySync.Endpoints;
using HevySync.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthorization();
builder.Services.AddIdentityServices();
builder.Services.AddDataServices(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHevyApiClient(builder.Configuration);
builder.Services.AddSwagger();

var app = builder.Build();

app.MapIdentityApi<ApplicationUser>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGroup("/hevy").MapHevy();

app.UseHttpsRedirection();

app.Run();
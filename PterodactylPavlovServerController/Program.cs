using PterodactylPavlovServerController.Middleware;
using PterodactylPavlovServerController.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<GoogleSheetService>();
builder.Services.AddScoped<PterodactylService>();
builder.Services.AddScoped<ServerControlService>();
builder.Services.AddScoped<PavlovRconService>();
builder.Services.AddSingleton<SteamWorkshopService>();
builder.Services.AddSingleton<SteamService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.UseMiddleware<BasicApiKeyMiddleware>();
}

app.Run();

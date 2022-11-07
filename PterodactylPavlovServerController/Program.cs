using Fluxor;
using PavlovStatsReader;
using PterodactylPavlovServerController.Middleware;
using PterodactylPavlovServerController.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<PavlovRconService>();
builder.Services.AddSingleton<GoogleSheetService>();
builder.Services.AddSingleton<PterodactylService>();
builder.Services.AddSingleton<PavlovServerService>();
builder.Services.AddSingleton<SteamWorkshopService>();
builder.Services.AddSingleton<SteamService>();
builder.Services.AddSingleton<StatsContext>();
builder.Services.AddSingleton<StatsCalculator>();
builder.Services.AddSingleton<PavlovStatisticsService>();

builder.Services.AddFluxor(options =>
{
    options.ScanAssemblies(typeof(Program).Assembly);
    options.UseReduxDevTools();
    options.UseRouting();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

//app.Services.GetRequiredService<PavlovStatisticsService>().RunStatsReader();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

if (!app.Environment.IsDevelopment())
{
    app.UseMiddleware<BasicApiKeyMiddleware>();
}

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();

//app.Services.GetRequiredService<PavlovStatisticsService>().StopStatsReader();
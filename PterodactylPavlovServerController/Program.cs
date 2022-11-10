using Blazored.Toast;
using Fluxor;
using PavlovStatsReader;
using PterodactylPavlovServerController.Contexts;
using PterodactylPavlovServerController.Middleware;
using PterodactylPavlovServerController.Models;
using PterodactylPavlovServerController.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<PavlovServerContext>();
builder.Services.AddScoped<PterodactylContext>();
builder.Services.AddSingleton<PavlovRconService>();
builder.Services.AddSingleton<GoogleSheetService>();
builder.Services.AddSingleton<PterodactylService>();
builder.Services.AddSingleton<PavlovServerService>();
builder.Services.AddSingleton<SteamWorkshopService>();
builder.Services.AddSingleton<SteamService>();
builder.Services.AddSingleton<StatsContext>();
builder.Services.AddSingleton<StatsCalculator>();
builder.Services.AddSingleton<PavlovStatisticsService>();
builder.Services.AddSingleton<PavlovRconConnectionService>();

builder.Services.AddFluxor(options =>
{
    options.ScanAssemblies(typeof(Program).Assembly);
});

builder.Services.AddBlazoredToast();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//builder.Services.AddAuthentication().AddApplicationCookie();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.Services.GetRequiredService<PavlovStatisticsService>().Run();
}

app.UseHttpsRedirection();

app.MapControllers();

if (!app.Environment.IsDevelopment())
{
    app.UseMiddleware<BasicApiKeyMiddleware>();
}

app.UseStaticFiles();

app.UseRouting();

//app.UseAuthentication();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

using (PterodactylContext context = new PterodactylContext(app.Services.GetRequiredService<IConfiguration>()))
{
    foreach (PterodactylUserModel user in context.Users)
    {
        Console.WriteLine(user.Username);
    }
}

using (IServiceScope scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<PavlovServerContext>().Database.EnsureCreated();
}

app.Services.GetRequiredService<PavlovRconConnectionService>().Run();

app.Run();

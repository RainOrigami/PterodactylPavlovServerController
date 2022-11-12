using Blazored.Toast;
using Fluxor;
using Microsoft.AspNetCore.HttpOverrides;
using PavlovStatsReader;
using PterodactylPavlovServerController.Areas.Identity.Data;
using PterodactylPavlovServerController.Contexts;
using PterodactylPavlovServerController.Data;
using PterodactylPavlovServerController.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<PterodactylPavlovServerControllerContext>();

builder.Services.AddDefaultIdentity<PterodactylPavlovServerControllerUser>()
    .AddEntityFrameworkStores<PterodactylPavlovServerControllerContext>();

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

app.Use(async (context, next) =>
{
    context.Request.PathBase = context.RequestServices.GetRequiredService<IConfiguration>()["basePath"];
    await next.Invoke();
});

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.All
});

app.UseHttpsRedirection();

app.MapControllers();

app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

using (IServiceScope scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<PterodactylPavlovServerControllerContext>().Database.EnsureCreated();
    scope.ServiceProvider.GetRequiredService<PavlovServerContext>().Database.EnsureCreated();
}

app.Services.GetRequiredService<PavlovRconConnectionService>().Run();


app.Run();

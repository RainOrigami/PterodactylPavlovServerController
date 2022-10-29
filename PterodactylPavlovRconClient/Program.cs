using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NReco.Logging.File;
using PterodactylPavlovRconClient.Services;

namespace PterodactylPavlovRconClient
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            try
            {
                string? basepath = Properties.Settings.Default.ppsc_basepath;
                string? apikey = Properties.Settings.Default.ppsc_apikey;
                string? pterodactylkey = Properties.Settings.Default.ppsc_pterodactyl_key;

                if (basepath == null || apikey == null || pterodactylkey == null)
                {
                    throw new Exception();
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Please add all configuration values to the configuration file!", "Application not configured", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (!Directory.Exists("logs"))
            {
                Directory.CreateDirectory("logs");
            }

            IServiceCollection services = new ServiceCollection();

            services.AddScoped<PavlovRcon>();

            services.AddSingleton<PterodactylAPIService>();

            services.AddLogging(configure =>
            {
                configure.AddFile("logs/pavlovRcon_{0:yyyy}-{0:MM}-{0:dd}.log", opts =>
                {
                    opts.Append = true;
                    opts.MinLevel = LogLevel.Trace;
                    opts.FormatLogFileName = fName => String.Format(fName, DateTime.Now);
                });
            });

            using (ServiceProvider serviceProvider = services.BuildServiceProvider())
            {
                services.AddSingleton<ILogger>(serviceProvider.GetRequiredService<ILogger<FileLogger>>());
            }

            using (ServiceProvider serviceProvider = services.BuildServiceProvider())
            {
                PavlovRcon pavlovRcon = serviceProvider.GetRequiredService<PavlovRcon>();
                Application.Run(pavlovRcon);
            }
        }
    }
}
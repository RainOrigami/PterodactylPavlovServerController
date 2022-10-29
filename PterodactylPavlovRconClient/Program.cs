using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
            services.AddSingleton<ILogger, FileLogger>();

            using (ServiceProvider serviceProvider = services.BuildServiceProvider())
            {
                PavlovRcon pavlovRcon = serviceProvider.GetRequiredService<PavlovRcon>();
                Application.Run(pavlovRcon);
            }
        }
    }

    class FileLogger : ILogger, IDisposable
    {
        public LogLevel MinimumLevel { get; set; } = LogLevel.Error;
        private static String lockMe = String.Empty;
        public IDisposable BeginScope<TState>(TState state) => this;
        public void Dispose() { }
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (logLevel < MinimumLevel)
            {
                return;
            }

            lock (lockMe)
            {
                try
                {
                    File.AppendAllText(String.Format("logs/pavlovRcon_{0:yyyy}-{0:MM}-{0:dd}.log", DateTime.Now), String.Format("[{0:HH}:{0:mm}:{0:ss}][{1}] {2}{3}", DateTime.Now, logLevel.ToString(), state!.ToString(), Environment.NewLine));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
    }
}
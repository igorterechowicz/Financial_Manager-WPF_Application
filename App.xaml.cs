using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using wpf_projekt.Data;
using wpf_projekt.Services;
using wpf_projekt.Views;

namespace wpf_projekt
{
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }

        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            var services = new ServiceCollection();
            services.AddSingleton<AppDbContext>();
            services.AddScoped<IEventLogService, EventLogService>();
            ServiceProvider = services.BuildServiceProvider();

            // Wymusza na WPF używanie formatowania zgodnego z systemem (np. przecinek w PL)
            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(
                    System.Windows.Markup.XmlLanguage.GetLanguage(System.Globalization.CultureInfo.CurrentCulture.IetfLanguageTag)));

            await DbInitializer.InitializeAsync();

            var loginWindow = new LoginWindow();
            loginWindow.Show();
        }
    }
}

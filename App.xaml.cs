using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Data;
using System.Windows;
using wpf_projekt.models;
using wpf_projekt.Services;

namespace wpf_projekt
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var services = new ServiceCollection();

            services.AddSingleton<AppDbContext>();

            services.AddScoped<IEventLogService, EventLogService>();

            ServiceProvider = services.BuildServiceProvider();

            // Wymusza na WPF używanie formatowania zgodnego z systemem (np. przecinek w PL)
            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(
                    System.Windows.Markup.XmlLanguage.GetLanguage(System.Globalization.CultureInfo.CurrentCulture.IetfLanguageTag)));
        }
    }
}

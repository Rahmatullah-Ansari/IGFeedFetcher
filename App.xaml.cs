using FeedFetcher.Interfaces;
using FeedFetcher.IOCAndServices;
using FeedFetcher.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace FeedFetcher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; }

        private IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            // Register your services
            //services.AddSingleton<IMessageService, ConsoleMessageService>();
            services.AddSingleton<ILogger, LoggerViewModel>();
            return services.BuildServiceProvider();
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
#if !DEBUG
            if (Debugger.IsAttached || Debugger.IsLogging())
                return;
#endif
            InstanceProvider.service = ConfigureServices();
        }
    }

}

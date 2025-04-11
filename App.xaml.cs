using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Windows;
using FeedFetcher.Interfaces;
using FeedFetcher.IOCAndServices;
using FeedFetcher.ViewModel;
using Microsoft.Extensions.DependencyInjection;

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

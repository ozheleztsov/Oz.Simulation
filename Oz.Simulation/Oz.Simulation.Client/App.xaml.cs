using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Oz.Simulation.Client.Contracts.Services;
using Oz.Simulation.Client.Contracts.Windows;
using Oz.Simulation.Client.HostedServices;
using Oz.Simulation.Client.Services;
using Oz.Simulation.Client.ViewModels;
using Oz.Simulation.ClientLib.Contracts;
using Oz.Simulation.ClientLib.Services;
using Oz.SimulationLib.Extensions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Oz.Simulation.Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IHost? _host;

        internal T? GetService<T>() where T : class =>
            _host?.Services.GetService(typeof(T)) as T ?? null;

        private async void InitializeApplication(StartupEventArgs e)
        {
            try
            {
                var appLocation =
                    Path.GetDirectoryName(
                        Assembly.GetEntryAssembly()?.Location ?? throw new InvalidOperationException());
                _host = Host.CreateDefaultBuilder(e.Args)
                    .ConfigureAppConfiguration((context, config) =>
                    {
                        config.SetBasePath(appLocation);
                        var env = context.HostingEnvironment;
                        config.AddJsonFile("appsettings.json", true, true);
                        if (env.IsDevelopment() && !string.IsNullOrEmpty(env.ApplicationName))
                        {
                            var appAssembly = Assembly.Load(new AssemblyName(env.ApplicationName));
                            config.AddUserSecrets(appAssembly, true);
                        }

                        config.AddEnvironmentVariables();
                        config.AddCommandLine(e.Args);
                    })
                    .ConfigureServices(ConfigureServices)
                    .Build();
                await _host.StartAsync();
            }
            catch (Exception exception)
            {
                await Current.Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show(
                        $"Error of initializing application{Environment.NewLine}{exception.Message}{Environment.NewLine}{exception.StackTrace}",
                        "Error", MessageBoxButton.OK);
                    Current.Shutdown(-1);
                });
            }
        }

        private void ConfigureServices(HostBuilderContext context, IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IAsyncService, AsyncService>();
            serviceCollection.AddTransient<MainWindowViewModel>();
            serviceCollection.AddTransient<IMainWindow, MainWindow>();
            serviceCollection.AddHostedService<ApplicationHostedService>();
            serviceCollection.RegisterSimulator();
            serviceCollection.AddSingleton<ISimulationService, SimulationService>();
        }

        private async void OnStartup(object sender, StartupEventArgs e)
        {
            Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
            InitializeApplication(e);
        }

        private async void OnExit(object sender, ExitEventArgs e)
        {
            if (_host != null)
            {
                await _host.StopAsync();
                _host.Dispose();
                _host = null;
            }
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            
        }
    }
}
using System;
using ExchangeRatesOnDate.Bot;
using ExchangeRatesOnDate.ExtensionsWrapper;
using ExchangeRatesOnDate.Resources;
using FreeCurrencyExchangeApiLib;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using NLog.LayoutRenderers;

namespace ExchangeRatesOnDate
{
    public class Program
    {
        private static ILogger? _logger;

        private static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += (_, e) =>
                LogUnhandledException((Exception)e.ExceptionObject, "AppDomain.CurrentDomain.UnhandledException");

            LayoutRenderer.Register<BuildConfigLayoutRenderer>("buildConfiguration");

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
            _logger = (ILogger)serviceProvider.GetService(typeof(ILogger<Program>));

            ExchangeRatesBot bot = serviceProvider.GetRequiredService<ExchangeRatesBot>();
            bot.Run();
        }

        private static void LogUnhandledException(Exception exception, string source)
        {
            string message = string.Format(TextResources.UnhandledException, source);
            try
            {
                System.Reflection.AssemblyName assemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName();
                message = string.Format(TextResources.UnhandledExceptionWithAssumblyData,
                    assemblyName.Name, assemblyName.Version);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Exception in LogUnhandledException");
            }
            finally
            {
                _logger?.LogError(exception, message);
            }
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ExchangeRatesBot>();
            services.AddSingleton<ICurrencyExchanger, CurrencyExchanger>();
            services.AddSingleton<IExtensionsWrapper, ExtensionsWrapper.ExtensionsWrapper>();
            services.AddLogging(logBuilder =>
            {
                logBuilder.ClearProviders();
                logBuilder.SetMinimumLevel(LogLevel.Debug);
                logBuilder.AddNLog("NLog.config");
            });
        }
    }
}

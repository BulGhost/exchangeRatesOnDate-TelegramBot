using System;
using System.Threading;
using ExchangeRatesOnDate.ExtensionsWrapper;
using ExchangeRatesOnDate.Resources;
using FreeCurrencyExchangeApiLib;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;

namespace ExchangeRatesOnDate.Bot
{
    public class ExchangeRatesBot
    {
        private static TelegramBotClient? _bot;
        private readonly ICurrencyExchanger _exchanger;
        private readonly IExtensionsWrapper _extensionsWrapper;
        private readonly ILogger _logger;

        public ExchangeRatesBot(ICurrencyExchanger exchanger, IExtensionsWrapper extensionsWrapper,
            ILogger<ExchangeRatesBot> logger)
        {
            _exchanger = exchanger;
            _extensionsWrapper = extensionsWrapper;
            _logger = logger;
        }

        public void Run()
        {
            _logger.LogInformation("Start application running");
            _bot = new TelegramBotClient(Configuration.BotToken);
            Console.WriteLine(TextResources.BotIsRunning);

            using var cts = new CancellationTokenSource();

            ReceiverOptions receiverOptions = new() { AllowedUpdates = { } };
            Handlers handlers = new(_exchanger, _extensionsWrapper, _logger);
            _bot.StartReceiving(handlers.HandleUpdateAsync,
                handlers.HandleErrorAsync,
                receiverOptions,
                cts.Token);
            _logger.LogDebug("Handlers configured, start receiving messages");

            Console.WriteLine(TextResources.StartReceivingUpdates);
            Console.WriteLine(TextResources.MessageToStop);
            while (Console.ReadKey(true).Key != ConsoleKey.Escape)
            {
            }

            cts.Cancel();
            _logger.LogInformation("Application stopped");
            Console.WriteLine(TextResources.Finish);
        }
    }
}

using System;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using ExchangeRatesOnDate.Resources;
using FreeCurrencyExchangeApiLib;

namespace ExchangeRatesOnDate
{
    public class Program
    {
        private static TelegramBotClient? _bot;

        private static void Main()
        {
            _bot = new TelegramBotClient(Configuration.BotToken);
            Console.WriteLine(TextResources.BotIsRunning);

            using var cts = new CancellationTokenSource();

            ReceiverOptions receiverOptions = new() { AllowedUpdates = { } };
            Handlers handlers = new Handlers(new CurrencyExchanger());
            _bot.StartReceiving(handlers.HandleUpdateAsync,
                handlers.HandleErrorAsync,
                receiverOptions,
                cts.Token);

            Console.WriteLine(TextResources.StartReceivingUpdates);
            Console.WriteLine(TextResources.MessageToStop);
            while (Console.ReadKey(true).Key != ConsoleKey.Escape)
            {
            }

            cts.Cancel();
            Console.WriteLine(TextResources.Finish);
        }
    }
}

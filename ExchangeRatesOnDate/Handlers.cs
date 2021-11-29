using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ExchangeRatesOnDate.Resources;
using FreeCurrencyExchangeApiLib;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace ExchangeRatesOnDate
{
    public class Handlers
    {
        private const int _currencyCodeLength = 3;
        private const string _baseCurrencyCode = "RUB";
        private const int _minDecimalsForCheapCurrencies = 3;
        private ICurrencyExchanger _exchanger;

        public Handlers(ICurrencyExchanger exchanger)
        {
            _exchanger = exchanger;
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            Task handler = update.Type switch
            {
                UpdateType.Message => BotOnMessageReceived(botClient, update.Message!),
                UpdateType.EditedMessage => BotOnMessageReceived(botClient, update.EditedMessage!),
                _ => UnknownUpdateHandlerAsync(botClient, update)
            };

            try
            {
                await handler;
            }
            catch (Exception exception)
            {
                await HandleErrorAsync(botClient, exception, cancellationToken);
            }
        }

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            string errorMessage = exception switch
            {
                ApiRequestException apiRequestException => string.Format(TextResources.TelegramApiError,
                    apiRequestException.ErrorCode, apiRequestException.Message),
                _ => exception.ToString()
            };

            Console.WriteLine(errorMessage);
            return Task.CompletedTask;
        }

        private async Task BotOnMessageReceived(ITelegramBotClient botClient, Message message)
        {
            if (message.Type != MessageType.Text)
                return;

            await botClient.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

            string[] requestParameters = message.Text!.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (!CurrencyCodeIsValid(requestParameters[0]))
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, TextResources.UnknownCurrencyCode);
                await botClient.SendTextMessageAsync(message.Chat.Id, TextResources.Instruction);
                return;
            }

            string targetCurrencyCode = requestParameters[0];

            if (!DateTime.TryParse(requestParameters[1], out DateTime date))
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, TextResources.InvalidDate);
                await botClient.SendTextMessageAsync(message.Chat.Id, TextResources.Instruction);
                return;
            }

            decimal exchangeRate = await _exchanger.DetermineExchangeRateAsync(_baseCurrencyCode,
                targetCurrencyCode, date);
            string reply = string.Format(TextResources.Reply, date.ToShortDateString(),
                targetCurrencyCode, FormatExchangeRate(exchangeRate));
            await botClient.SendTextMessageAsync(message.Chat.Id, reply);
        }

        private bool CurrencyCodeIsValid(string currencyCode)
        {
            return currencyCode.Length == _currencyCodeLength &&
                   Regex.IsMatch(currencyCode, @"^[a-zA-Z]+$");
        }

        private string FormatExchangeRate(decimal exchangeRate)
        {
            if (exchangeRate < 1m)
            {
                return $"{1 / exchangeRate:F2}";
            }

            int decimals = _minDecimalsForCheapCurrencies;
            for (int i = 1; (double)exchangeRate / Math.Pow(10, i) > 1d; i++)
            {
                decimals++;
            }

            return (1 / exchangeRate).ToString("N" + decimals);
        }

        private Task UnknownUpdateHandlerAsync(ITelegramBotClient botClient, Update update)
        {
            Console.WriteLine(TextResources.UnknownUpdateType, update.Type);
            return Task.CompletedTask;
        }
    }
}

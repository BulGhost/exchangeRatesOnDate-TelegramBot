using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ExchangeRatesOnDate.Resources;
using FreeCurrencyExchangeApiLib;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ExchangeRatesOnDate
{
    public class Handlers
    {
        private const int _currencyCodeLength = 3;
        private const string _baseCurrencyCode = "RUB";
        private const int _minDecimalsForCheapCurrencies = 3;
        private readonly ICurrencyExchanger _exchanger;

        public Handlers(ICurrencyExchanger exchanger)
        {
            _exchanger = exchanger;
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Task handler = update.Type switch
            {
                UpdateType.Message => BotOnMessageReceived(botClient, update.Message!),
                UpdateType.EditedMessage => BotOnMessageReceived(botClient, update.EditedMessage!),
                _ => UnknownUpdateHandlerAsync(botClient, update)
            };

            try
            {
                await handler.ConfigureAwait(false);
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
            await botClient.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

            if (!await TryParseMessage(botClient, message))
            {
                return;
            }

            string[] requestParameters = GetRequestParemeters(message);
            string targetCurrencyCode = requestParameters[0];
            DateTime date = DateTime.Parse(requestParameters[1]);

            try
            {
                decimal exchangeRate = await _exchanger.DetermineExchangeRateAsync(_baseCurrencyCode,
                    targetCurrencyCode, date);
                string reply = string.Format(TextResources.Reply, date.ToShortDateString(),
                    targetCurrencyCode, FormatExchangeRate(exchangeRate));
                await botClient.SendTextMessageAsync(message.Chat.Id, reply);
            }
            catch (ArgumentException ex)
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, ex.Message.Remove(ex.Message.IndexOf('(')));
            }
            catch (HttpRequestException ex)
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, TextResources.HttpRequestFail);
                Console.WriteLine(ex.Message);
            }
        }

        private async Task<bool> TryParseMessage(ITelegramBotClient botClient, Message message)
        {
            if (message.Type != MessageType.Text)
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, TextResources.NotSupportedCommand);
                await botClient.SendTextMessageAsync(message.Chat.Id, TextResources.Instruction);
                return false;
            }

            string[] requestParameters = GetRequestParemeters(message);
            if (!CurrencyCodeIsValid(requestParameters[0]))
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, TextResources.UnknownCurrencyCode);
                await botClient.SendTextMessageAsync(message.Chat.Id, TextResources.Instruction);
                return false;
            }

            if (!DateTime.TryParse(requestParameters[1], out _))
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, TextResources.InvalidDate);
                await botClient.SendTextMessageAsync(message.Chat.Id, TextResources.Instruction);
                return false;
            }

            return true;
        }

        private string[] GetRequestParemeters(Message message)
        {
            return message.Text!.Split(' ', StringSplitOptions.RemoveEmptyEntries);
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

using System;
using System.Globalization;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ExchangeRatesOnDate.ExtensionsWrapper;
using ExchangeRatesOnDate.Resources;
using FreeCurrencyExchangeApiLib;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ExchangeRatesOnDate.Bot
{
    public class Handlers
    {
        private const int _currencyCodeLength = 3;
        private const string _baseCurrencyCode = "RUB";
        private const int _minDecimalsForCheapCurrencies = 3;
        private readonly ICurrencyExchanger _exchanger;
        private readonly IExtensionsWrapper _extensionsWrapper;
        private readonly ILogger _logger;

        public Handlers(ICurrencyExchanger exchanger, IExtensionsWrapper extensionsWrapper, ILogger logger)
        {
            _exchanger = exchanger;
            _extensionsWrapper = extensionsWrapper;
            _logger = logger;
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            _logger.LogInformation("New update (id={0}) received, Type = {1}", update.Id, update.Type);
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
                _logger.LogInformation("Update with id={0} processed", update.Id);
            }
            catch (Exception exception)
            {
                _logger.LogError("Error during processing update with id={0}", update.Id);
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

            _logger.LogError(errorMessage);
            return Task.CompletedTask;
        }

        private async Task BotOnMessageReceived(ITelegramBotClient botClient, Message message)
        {
            _logger.LogInformation("Start message (id={0}) processing", message.MessageId);
            await botClient.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

            if (!TryParseMessage(message, out string? targetCurrencyCode, out DateTime date, out string? errorMessage))
            {
                _logger.LogInformation("Unable to make out message with id={0}", message.MessageId);
                await _extensionsWrapper.SendTextMessageAsync(botClient, message.Chat.Id, errorMessage!);
                await _extensionsWrapper.SendTextMessageAsync(botClient, message.Chat.Id, TextResources.Instruction);
                return;
            }

            try
            {
                _logger.LogInformation("Message with id={0} successfully parsed", message.MessageId);
                decimal exchangeRate = await _exchanger.DetermineExchangeRateAsync(_baseCurrencyCode,
                    targetCurrencyCode, date);
                _logger.LogInformation("Exchange rate received from server (message id={0})", message.MessageId);
                string reply = string.Format(TextResources.Reply, date.ToShortDateString(),
                    targetCurrencyCode, FormatExchangeRate(exchangeRate));
                await _extensionsWrapper.SendTextMessageAsync(botClient, message.Chat.Id, reply);
            }
            catch (ArgumentException ex)
            {
                _logger.LogInformation("No data on server (message id={0})", message.MessageId);
                await _extensionsWrapper.SendTextMessageAsync(botClient, message.Chat.Id, ex.Message);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "Error during http request");
                await _extensionsWrapper.SendTextMessageAsync(botClient, message.Chat.Id, TextResources.HttpRequestFail);
            }
        }

        private bool TryParseMessage(Message message, out string? currencyCode, out DateTime date, out string? errorMessage)
        {
            currencyCode = default;
            date = default;
            if (message.Type != MessageType.Text)
            {
                _logger.LogDebug("Message with id={0} is non-text message", message.MessageId);
                errorMessage = TextResources.NotSupportedCommand;
                return false;
            }

            string[] requestParameters = GetRequestParemeters(message);
            if (!CurrencyCodeIsValid(requestParameters[0]))
            {
                _logger.LogDebug("Message with id={0} has invalid currency code", message.MessageId);
                errorMessage = TextResources.UnknownCurrencyCode;
                return false;
            }

            currencyCode = requestParameters[0];
            string[] formatStrings = { "dd.MM/yyyy", "MM-dd-yyyy", "yyyy-MM-dd", "dd/MM/yyyy" };
            if (!DateTime.TryParseExact(requestParameters[1], formatStrings, new CultureInfo("ru-RU"), DateTimeStyles.None, out date))
            {
                _logger.LogDebug("Message with id={0} has invalid date", message.MessageId);
                errorMessage = TextResources.InvalidDate;
                return false;
            }

            if (date > DateTime.Now)
            {
                _logger.LogDebug("Message with id={0} has date in future", message.MessageId);
                errorMessage = TextResources.DateInFuture;
                return false;
            }

            errorMessage = default;
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
            _logger.LogInformation("Received update with unprocessed type: {0}", update.Type);
            return Task.CompletedTask;
        }
    }
}

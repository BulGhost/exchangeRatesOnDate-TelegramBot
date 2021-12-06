using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using FreeCurrencyExchangeApiLib.Resources;
using Microsoft.Extensions.Logging;

namespace FreeCurrencyExchangeApiLib
{
    public class CurrencyExchanger : ICurrencyExchanger
    {
        private readonly HttpClient _client = new()
        {
            BaseAddress = new Uri("https://freecurrencyapi.net")
        };

        private const string _invalidRequestErrorMessage = "Error during checking request parameters";
        private const string _jsonParsingErrorMessage = "Fail on Json parsing";
        private const string _noDataErrorMessage = "No request data on the server";
        private const int _numberOfAttemptsToSendRequest = 3;
        private string _dateForRequestToApi;
        private readonly string _apiKey;
        private readonly ILogger _logger;

        public CurrencyExchanger(string apiKey, ILogger<CurrencyExchanger> logger)
        {
            _apiKey = apiKey;
            _logger = logger;
        }

        public async Task<decimal> GetExchangeRateFromApiAsync(string baseCurrency, string targetCurrency,
            DateTime date)
        {
            _logger.LogInformation("Start determine exchange rate");
            CheckRequestParameters(baseCurrency, targetCurrency, date);

            string requestUri = GetRequestUri(baseCurrency, date);
            await using Stream stream = await SendRequest(requestUri).ConfigureAwait(false);

            try
            {
                using JsonDocument doc = await JsonDocument.ParseAsync(stream);
                JsonElement currencyRateData = doc.RootElement.GetProperty("data");
                JsonElement exchangeRatesData = date.Date == DateTime.Today
                    ? currencyRateData
                    : currencyRateData.GetProperty(_dateForRequestToApi);
                if (exchangeRatesData.TryGetProperty(targetCurrency, out JsonElement exchangeRate))
                {
                    _logger.LogInformation("Required exchange rate is got from Json");
                    return exchangeRate.GetDecimal();
                }
            }
            catch (Exception ex) when (ex is JsonException or KeyNotFoundException)
            {
                _logger.LogError(ex, "Fail on trying to get data from Json");
                throw new CurrencyExchangeException(_jsonParsingErrorMessage, ex, TextResources.UnableToProcessData);
            }

            _logger.LogInformation("No {0} exchange rate data as of {1}", targetCurrency, date.ToShortDateString());
            throw new CurrencyExchangeException(_noDataErrorMessage, TextResources.UnacceptablePastDate);
        }

        private void CheckRequestParameters(string baseCurrency, string targetCurrency, DateTime date)
        {
            if (!ApiSettings.AvailableCurrencies.Contains(baseCurrency) ||
                !ApiSettings.AvailableCurrencies.Contains(targetCurrency))
            {
                _logger.LogInformation("One or both of currency codes is unavailable: {0}, {1}", baseCurrency, targetCurrency);
                throw new CurrencyExchangeException(_invalidRequestErrorMessage, TextResources.InvalidCurrencyCode);
            }

            if (date > ApiSettings.LatestDate)
            {
                _logger.LogInformation("Unavailable date: {0}", date.ToShortDateString());
                throw new CurrencyExchangeException(_invalidRequestErrorMessage, TextResources.DateInFutureError);
            }

            if (date < ApiSettings.EarliestDate)
            {
                _logger.LogInformation("No data on this date: {0}", date.ToShortDateString());
                throw new CurrencyExchangeException(_invalidRequestErrorMessage, TextResources.UnacceptablePastDate);
            }

            _logger.LogInformation("Checking request parameters is complete");
        }

        private string GetRequestUri(string baseCurrency, DateTime date)
        {
            _logger.LogDebug("Start getting request URI");
            baseCurrency = baseCurrency.ToUpper();
            _dateForRequestToApi = $"{date.Year:D4}-{date.Month:D2}-{date.Day:D2}";

            return date.Date == DateTime.Today
                ? "api/v2/latest?apikey=" + _apiKey + "&base_currency=" + baseCurrency
                : "api/v2/historical?apikey=" + _apiKey + "&base_currency=" + baseCurrency +
                  "&date_from=" + _dateForRequestToApi + "&date_to=" + _dateForRequestToApi;
        }

        private async Task<Stream> SendRequest(string uri)
        {
            _logger.LogInformation("Sending request to API");
            Stream stream = null;
            for (int i = 1; i <= _numberOfAttemptsToSendRequest; i++)
            {
                try
                {
                    stream = await _client.GetStreamAsync(uri);
                    break;
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogInformation("Unsuccessful attempt to get response from API");
                    if (i == _numberOfAttemptsToSendRequest)
                    {
                        _logger.LogWarning(ex, "Attempts to get response from API exhausted. URI: {0}", uri);
                        throw;
                    }
                }
            }

            _logger.LogInformation("Response is received from API");
            return stream;
        }
    }
}

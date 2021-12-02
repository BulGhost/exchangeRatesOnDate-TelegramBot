using System;
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
        private static readonly HttpClient _client = new()
        {
            BaseAddress = new Uri("https://freecurrencyapi.net"),
        };

        private const int _numberOfAttemptsToSendRequest = 3;
        private string _dateForRequestToApi;
        private readonly ILogger _logger;

        public CurrencyExchanger(ILogger<CurrencyExchanger> logger)
        {
            _logger = logger;
        }

        public async Task<decimal> DetermineExchangeRateAsync(string baseCurrency, string targetCurrency, DateTime date)
        {
            _logger.LogInformation("Start determine exchange rate");
            CheckRequestParameters(baseCurrency, targetCurrency, date);

            string requestUri = GetRequestUri(baseCurrency, date);
            Stream stream = await SendRequest(requestUri);

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

            _logger.LogInformation("No {0} exchange rate data as of {1}", targetCurrency, date.ToShortDateString());
            throw new ArgumentException(TextResources.UnacceptablePastDate);
        }

        private void CheckRequestParameters(string baseCurrency, string targetCurrency, DateTime date)
        {
            if (!ApiSettings.AvailableCurrencies.Contains(baseCurrency) ||
                !ApiSettings.AvailableCurrencies.Contains(targetCurrency))
            {
                _logger.LogInformation("One or both of currency codes is unavailable: {0}, {1}", baseCurrency, targetCurrency);
                throw new ArgumentException(TextResources.InvalidCurrencyCode);
            }

            if (date > ApiSettings.LatestDate)
            {
                _logger.LogInformation("Unavailable date: {0}", date.ToShortDateString());
                throw new ArgumentException(TextResources.DateInFutureError);
            }

            if (date < ApiSettings.EarliestDate)
            {
                _logger.LogInformation("No data on this date: {0}", date.ToShortDateString());
                throw new ArgumentException(TextResources.UnacceptablePastDate);
            }

            _logger.LogInformation("Checking request parameters is complete");
        }

        private string GetRequestUri(string baseCurrency, DateTime date)
        {
            _logger.LogDebug("Start getting request URI");
            baseCurrency = baseCurrency.ToUpper();
            _dateForRequestToApi = $"{date.Year:D4}-{date.Month:D2}-{date.Day:D2}";

            return date.Date == DateTime.Today
                ? "api/v2/latest?apikey=" + ApiSettings.ApiKey + "&base_currency=" + baseCurrency
                : "api/v2/historical?apikey=" + ApiSettings.ApiKey + "&base_currency=" + baseCurrency +
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
                catch (HttpRequestException)
                {
                    _logger.LogInformation("Unsuccessful attempt to get response from API");
                    if (i == _numberOfAttemptsToSendRequest)
                    {
                        _logger.LogWarning("Attempts to get response from API exhausted");
                        throw;
                    }
                }
            }

            _logger.LogInformation("Response is received from API");
            return stream;
        }
    }
}

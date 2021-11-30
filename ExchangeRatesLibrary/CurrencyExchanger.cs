using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using FreeCurrencyExchangeApiLib.Resources;

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

        public async Task<decimal> DetermineExchangeRateAsync(string baseCurrency, string targetCurrency, DateTime date)
        {
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
                return exchangeRate.GetDecimal();
            }

            throw new ArgumentException(TextResources.UnacceptablePastDate, nameof(date));
        }

        private void CheckRequestParameters(string baseCurrency, string targetCurrency, DateTime date)
        {
            if (!ApiSettings.AvailableCurrencies.Contains(baseCurrency))
            {
                throw new ArgumentException(TextResources.InvalidCurrencyCode, nameof(baseCurrency));
            }

            if (!ApiSettings.AvailableCurrencies.Contains(targetCurrency))
            {
                throw new ArgumentException(TextResources.InvalidCurrencyCode, nameof(targetCurrency));
            }

            if (date > ApiSettings.LatestDate)
            {
                throw new ArgumentException(TextResources.DateInFutureError, nameof(date));
            }

            if (date < ApiSettings.EarliestDate)
            {
                throw new ArgumentException(TextResources.UnacceptablePastDate, nameof(date));
            }
        }

        private string GetRequestUri(string baseCurrency, DateTime date)
        {
            baseCurrency = baseCurrency.ToUpper();
            _dateForRequestToApi = $"{date.Year:D4}-{date.Month:D2}-{date.Day:D2}";

            return date.Date == DateTime.Today
                ? "api/v2/latest?apikey=" + ApiSettings.ApiKey + "&base_currency=" + baseCurrency
                : "api/v2/historical?apikey=" + ApiSettings.ApiKey + "&base_currency=" + baseCurrency +
                  "&date_from=" + _dateForRequestToApi + "&date_to=" + _dateForRequestToApi;
        }

        private async Task<Stream> SendRequest(string uri)
        {
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
                    if (i == _numberOfAttemptsToSendRequest)
                    {
                        throw;
                    }
                }
            }

            return stream;
        }
    }
}

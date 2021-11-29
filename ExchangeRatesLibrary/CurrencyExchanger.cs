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
        private string _dateForRequestToApi;

        public decimal DetermineExchangeRate(string baseCurrency, string targetCurrency, DateTime date)
        {
            CheckRequestParemeters(baseCurrency, targetCurrency, date);

            using HttpClient client = new()
            {
                BaseAddress = new Uri("https://freecurrencyapi.net")
            };

            string requestUri = GetRequestUri(baseCurrency, date);
            Stream stream = client.GetStreamAsync(requestUri).Result;

            using JsonDocument doc = JsonDocument.Parse(stream);
            JsonElement exchangeRatesData = doc.RootElement[1];

            return exchangeRatesData.GetProperty(targetCurrency).GetDecimal();
        }

        public async Task<decimal> DetermineExchangeRateAsync(string baseCurrency, string targetCurrency, DateTime date)
        {
            CheckRequestParemeters(baseCurrency, targetCurrency, date);

            using HttpClient client = new()
            {
                BaseAddress = new Uri("https://freecurrencyapi.net")
            };

            string requestUri = GetRequestUri(baseCurrency, date);
            Stream stream = await client.GetStreamAsync(requestUri);

            using JsonDocument doc = await JsonDocument.ParseAsync(stream);
            JsonElement currencyRateData = doc.RootElement.GetProperty("data");
            JsonElement exchangeRatesData = currencyRateData.GetProperty(_dateForRequestToApi);

            return exchangeRatesData.GetProperty(targetCurrency).GetDecimal();
        }

        private void CheckRequestParemeters(string baseCurrency, string targetCurrency, DateTime date)
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
    }
}

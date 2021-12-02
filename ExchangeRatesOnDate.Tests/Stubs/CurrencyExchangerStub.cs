using System;
using System.Threading.Tasks;
using FreeCurrencyExchangeApiLib;

namespace ExchangeRatesOnDate.Tests.Stubs
{
    internal class CurrencyExchangerStub : ICurrencyExchanger
    {
        public Task<decimal> DetermineExchangeRateAsync(string baseCurrency, string targetCurrency, DateTime date)
        {
            return default;
        }
    }
}

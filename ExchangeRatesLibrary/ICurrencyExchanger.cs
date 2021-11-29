using System;
using System.Threading.Tasks;

namespace FreeCurrencyExchangeApiLib
{
    public interface ICurrencyExchanger
    {
        decimal DetermineExchangeRate(string baseCurrency, string targetCurrency, DateTime date);

        Task<decimal> DetermineExchangeRateAsync(string baseCurrency, string targetCurrency, DateTime date);
    }
}

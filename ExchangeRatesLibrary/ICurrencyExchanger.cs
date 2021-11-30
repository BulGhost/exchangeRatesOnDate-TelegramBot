using System;
using System.Threading.Tasks;

namespace FreeCurrencyExchangeApiLib
{
    public interface ICurrencyExchanger
    {
        Task<decimal> DetermineExchangeRateAsync(string baseCurrency, string targetCurrency, DateTime date);
    }
}

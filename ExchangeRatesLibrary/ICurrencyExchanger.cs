using System;
using System.Threading.Tasks;

namespace FreeCurrencyExchangeApiLib
{
    public interface ICurrencyExchanger
    {
        Task<decimal> GetExchangeRateFromApiAsync(string baseCurrency, string targetCurrency, DateTime date);
    }
}

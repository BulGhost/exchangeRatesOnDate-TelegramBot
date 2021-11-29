using System;

namespace FreeCurrencyExchangeApiLib
{
    public static class ApiSettings
    {
        public static readonly string ApiKey = "043d75f0-4eec-11ec-a304-49a801ae1c08";

        public static string[] AvailableCurrencies =>
            new[]
            {
                "AED", "NGN", "PYG", "PLN", "PKR", "PHP", "PGK", "PEN", "PAB", "OMR", "NZD", "NPR", "NOK", "NIO",
                "NAD", "RON", "MYR", "MXN", "MWK", "MVR", "MUR", "MOP", "MNT", "MMK", "MKD", "MDL", "MAD", "LYD",
                "QAR", "RUB", "LRD", "TRY", "YER", "XPF", "XOF", "XAF", "VND", "UZS", "USD", "UGX", "UAH", "TZS",
                "TWD", "TTD", "TND", "RWF", "TJS", "THB", "SZL", "SYP", "SVC", "STD", "SOS", "SLL", "SGD", "SEK",
                "SCR", "SAR", "LSL", "LKR", "ALL", "CAD", "DZD", "DOP", "DKK", "DJF", "CZK", "CVE", "CRC", "COP",
                "CNY", "CLP", "CHF", "CDF", "BYR", "ETB", "BWP", "BSD", "BRL", "BOB", "BND", "BIF", "BHD", "BGN",
                "BDT", "AUD", "ARS", "AMD", "EGP", "EUR", "LBP", "IQD", "LAK", "KZT", "KRW", "KMF", "KHR", "KGS",
                "KES", "JPY", "JOD", "JOD", "ISK", "IRR", "INR", "FJD", "ILS", "IDR", "HUF", "HTG", "HNL", "HKD",
                "GYD", "GTQ", "GNF", "GMD", "GEL", "GBP", "ZAR"
            };

        public static DateTime EarliestDate => new(1999, 1, 1);

        public static DateTime LatestDate => DateTime.Today;
    }
}

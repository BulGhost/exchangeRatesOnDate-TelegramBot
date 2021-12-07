using System;
using System.Runtime.Serialization;

namespace FreeCurrencyExchangeApiLib
{
    [Serializable]
    public class CurrencyExchangeException : Exception
    {
        public virtual string CauseOfError { get; }

        public CurrencyExchangeException()
        {
        }

        public CurrencyExchangeException(string message)
            : base(message)
        {
        }

        public CurrencyExchangeException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public CurrencyExchangeException(string message, string causeOfError)
            : base(message)
        {
            CauseOfError = causeOfError;
        }

        public CurrencyExchangeException(string message, Exception inner, string causeOfError)
            : base(message, inner)
        {
            CauseOfError = causeOfError;
        }

        protected CurrencyExchangeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}

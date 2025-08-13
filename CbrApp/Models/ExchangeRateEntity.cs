using System;

namespace CbrApp.Models
{
    public class ExchangeRateEntity
    {
        public string CurrencyNumCode { get; set; }

        public DateTime Date { get; set; }

        public int Nominal { get; set; }

        public decimal Value { get; set; }

        public CurrencyEntity Currency { get; set; } = null!;
    }
}

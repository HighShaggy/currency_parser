using CbrApp.Models;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace CbrApp.Services
{
    public static class CbrParser
    {
        public static List<(CurrencyEntity, ExchangeRateEntity)> Parse(string response, DateTime requestDate)
        {
            var doc = XDocument.Parse(response);

            var dateStr = doc.Root?.Attribute("Date")?.Value;
            DateTime.TryParse(dateStr, out var xmlDate);

            var parseResult = new List<(CurrencyEntity, ExchangeRateEntity)>();
            try
            {
                foreach (var v in doc.Descendants("Valute"))
                {
                    var currency = new CurrencyEntity
                    {
                        NumCode = v.Element("NumCode")?.Value ?? "",
                        CharCode = v.Element("CharCode")?.Value ?? "",
                        Name = v.Element("Name")?.Value ?? ""
                    };

                    var rate = new ExchangeRateEntity
                    {
                        CurrencyNumCode = currency.NumCode,
                        Date = xmlDate,
                        Nominal = int.Parse(v.Element("Nominal").Value),
                        Value = Convert.ToDecimal(v.Element("Value")?.Value)
                    };
                    parseResult.Add((currency, rate));
                }
                return parseResult;
            }
            catch (Exception ex)
            {
                return new List<(CurrencyEntity, ExchangeRateEntity)>();
            }
        }
    }
}

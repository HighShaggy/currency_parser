using CbrApp.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;

namespace CbrApp.Services
{
    public static class CbrParser
    {
        /// <summary>
        /// Парсит XML-ответ ЦБ РФ и возвращает дату из XML и список курсов валют.
        /// </summary>
        public static (DateTime xmlDate, List<(CurrencyEntity, ExchangeRateEntity)> rates) Parse(string response, DateTime requestDate)
        {
            var doc = XDocument.Parse(response);

            var dateStr = doc.Root?.Attribute("Date")?.Value;
            DateTime.TryParse(dateStr, new CultureInfo("ru-RU"), DateTimeStyles.None, out var xmlDate);

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
            }
            catch (Exception ex)
            {
                parseResult = new List<(CurrencyEntity, ExchangeRateEntity)>();
                xmlDate = DateTime.MinValue;
            }
            return (xmlDate, parseResult);
        }
    }
}

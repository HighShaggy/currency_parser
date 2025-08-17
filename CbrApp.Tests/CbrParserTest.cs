using CbrApp.Services;

namespace CbrApp.Tests
{
    public class CbrParserTests
    {
        /// <summary>
        /// Проверяет, что парсер корректно обрабатывает XML с одной валютой.
        /// </summary>
        [Fact]
        public void Parse_EmptyList()
        {
            var xml = "<ValCurs Date='01.01.2023' name='Abudabi Market'>" +
                    "<Valute ID='R01235'>" +
                    "<NumCode>840</NumCode>" +
                    "<CharCode>USD</CharCode>" +
                    "<Nominal>1</Nominal>" +
                    "<Name>Доллар США</Name>" +
                    "<Value>74,1234</Value>" +
                    "</Valute>" +
                    "</ValCurs>";

            var requestDate = new DateTime(2023, 1, 1);

            var (xmlDate, rates) = CbrParser.Parse(xml, requestDate);

            // xmlDate должен быть равен дате, указанной в корне XML
            Assert.Equal(requestDate, xmlDate);

            Assert.Single(rates);

            var (currency, rate) = rates[0];

            Assert.Equal("840", currency.NumCode);
            Assert.Equal("USD", currency.CharCode);
            Assert.Equal("Доллар США", currency.Name);

            Assert.Equal("840", rate.CurrencyNumCode);
            Assert.Equal(requestDate, rate.Date);
            Assert.Equal(1, rate.Nominal);
            Assert.Equal(74.1234m, rate.Value);
        }

        /// <summary>
        /// Проверяет, что парсер возвращает пустой список при некорректном XML.
        /// </summary>
        [Fact]
        public void Parse_BrokenXml()
        {
            var badXml = "<BrokenXml>" +
                        "<Item>" +
                        "<Code>123</Code>" +
                        "<Value>99.99</Value>" +
                        "</Item>" +
                        "</BrokenXml>";

            var requestDate = new DateTime(2025, 1, 1);
            var (xmlDate, rates) = CbrParser.Parse(badXml, requestDate);

            // xmlDate должен быть MinValue при ошибке парсинга
            Assert.Equal(DateTime.MinValue, xmlDate);
            Assert.Empty(rates);
        }
    }
}

using CbrApp.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;

namespace CbrApp.Tests
{
    public class CbrClientTest
    {
        /// <summary>
        /// Проверяет, что ЦБ корректно возвращает данные при успешном HTTP-запросе.
        /// </summary>
        [Fact]
        public async Task ReturnsRates_ForValidDate()
        {
            var loggerMock = new Mock<ILogger<CbrClient>>();
            var handlerMock = new Mock<HttpMessageHandler>();

            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("Fake XML data")
                });

            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("https://fake.cbr.test")
            };

            var client = new CbrClient(loggerMock.Object, httpClient);

            var validDate = new DateTime(2025, 1, 1);
            var result = await client.GetDailyRatesAsync(validDate);

            Assert.Equal("Fake XML data", result);
        }
    }
}
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace CbrApp.Services
{
    public class CbrClient(ILogger<CbrClient> logger, HttpClient httpClient)
    {
        private readonly ILogger<CbrClient> _logger = logger;
        private readonly HttpClient _httpClient = httpClient;

        /// <summary>
        /// Возвращает XML курсов валют с сайта ЦБ РФ на указанную дату.
        /// </summary>
        public async Task<string?> GetDailyRatesAsync(DateTime date)
        {
            // ЦБ РФ не хранит данные "из будущего" :)
            if (date.Date > DateTime.Now.Date)
            {
                _logger.LogWarning($"Запрошена будущая дата: {date}. Запрос отменён.");
                return null;
            }

            try
            {
                var dateStr = date.ToString("dd/MM/yyyy");
                var response = await _httpClient.GetAsync($"scripts/XML_daily.asp?date_req={dateStr}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"Ошибка при получении курсов за {date}");
                return null;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, $"Превышен таймаут запроса");
                return null;
            }
        }
    }
}

using Quartz;
using System;
using System.Threading.Tasks;

namespace CbrApp.Services
{
    public class DailyAppRunner(ICurrencyRateService currencyRateService) : IJob
    {
        private readonly ICurrencyRateService _currencyRateService = currencyRateService;
        /// <summary>
        /// Точка входа в бизнес-логику сервиса:
        /// обновляет курсы за предыдущие даты и загружает курс на текущую дату.
        /// </summary>
        public async Task Execute(IJobExecutionContext context)
        {
            Console.WriteLine($"DailyAppRunner запустился {DateTime.Now}");
            await _currencyRateService.UpdateRatesAsync();
            await _currencyRateService.LoadRatesForDateAsync(DateTime.Now);
        }
    }
}
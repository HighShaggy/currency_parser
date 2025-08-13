using System;
using System.Threading.Tasks;

namespace CbrApp.Services
{
    public class AppRunner(ICurrencyRateService currencyRateService)
    {
        private readonly ICurrencyRateService _currencyRateService = currencyRateService;
        /// <summary>
        /// Точка входа в бизнес-логику сервиса:
        /// обновляет курсы за предыдущие даты и загружает курс на текущую дату.
        /// </summary>
        public async Task RunAppAsync()
        {
            await _currencyRateService.UpdateRatesAsync();
            await _currencyRateService.LoadRatesForDateAsync(DateTime.Now);
        }
    }
}
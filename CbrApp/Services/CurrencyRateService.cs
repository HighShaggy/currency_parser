using CbrApp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CbrApp.Services
{
    public class CurrencyRateService(CbrClient cbrClient, AppDbContext dbContext, ILogger<CurrencyRateService> logger) : ICurrencyRateService
    {
        private readonly CbrClient _cbrClient = cbrClient;
        private readonly AppDbContext _dbContext = dbContext;
        private readonly ILogger<CurrencyRateService> _logger = logger;
        /// <summary>
        /// Загружает курсы валют на указанную дату и сохраняет в БД,
        /// если они  отсутствуют.
        /// </summary>
        public async Task LoadRatesForDateAsync(DateTime date)
        {
            try
            {
                var xml = await _cbrClient.GetDailyRatesAsync(date);
                var rates = CbrParser.Parse(xml, date);

                if (rates.Count == 0)
                {
                    _logger.LogWarning($"Нет данных по курсам за {date:dd.MM.yyyy}");
                    return;
                }

                var existingCurrency = await _dbContext.Currencies
                    .AsNoTracking()
                    .Select(c => c.NumCode)
                    .ToHashSetAsync();

                var existingRates = await _dbContext.ExchangeRates
                    .AsNoTracking()
                    .Select(r => new { r.CurrencyNumCode, r.Date })
                    .ToHashSetAsync();

                var newCurrencies = rates
                    .Select(r => r.Item1)
                    .Where(c => !existingCurrency.Contains(c.NumCode))
                    .DistinctBy(c => c.NumCode)
                    .ToList();

                var newRates = rates
                    .Select(r => r.Item2)
                    .Where(r => !existingRates.Contains(new { r.CurrencyNumCode, r.Date }))
                    .ToList();

                if (newCurrencies.Count > 0)
                    _dbContext.Currencies.AddRange(newCurrencies);

                if (newRates.Count > 0)
                    _dbContext.ExchangeRates.AddRange(newRates);

                await _dbContext.SaveChangesAsync();
                _logger.LogInformation($"Курсы за {date:dd.MM.yyyy} сохранены.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при загрузке курсов за {date:dd.MM.yyyy}");
            }
        }
        /// <summary>
        /// Проверяет наличие курсов за последний месяц и
        /// автоматически догружает недостающие даты.
        /// </summary>
        public async Task UpdateRatesAsync()
        {
            var today = DateTime.Today;
            var monthAgo = today.AddMonths(-1);

            bool hasData = await _dbContext.ExchangeRates.AnyAsync();
            if (!hasData)
            {
                _logger.LogInformation("База пустая — загружаем за месяц...");
                for (var date = monthAgo; date <= today; date = date.AddDays(1))
                {
                    await LoadRatesForDateAsync(date);
                }
                return;
            }

            for (var date = monthAgo; date <= today; date = date.AddDays(1))
            {
                bool exists = await _dbContext.ExchangeRates.AnyAsync(r => r.Date == date);
                if (!exists)
                {
                    _logger.LogInformation($"Нет данных за {date:dd.MM.yyyy} — загружаем...");
                    await LoadRatesForDateAsync(date);
                }
            }
        }
    }
}

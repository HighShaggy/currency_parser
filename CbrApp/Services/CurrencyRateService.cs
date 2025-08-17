using CbrApp.Data;
using CbrApp.Models;
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
        public async Task<bool> LoadRatesForDateAsync(DateTime date)
        {
            try
            {
                var xmlContent = await _cbrClient.GetDailyRatesAsync(date);
                var parsedResult = CbrParser.Parse(xmlContent, date);

                var xmlDate = parsedResult.xmlDate;
                var rates = parsedResult.rates;

                if (xmlDate.Date != date.Date)
                {
                    _logger.LogInformation(
                        $"Курсы за {date:dd.MM.yyyy} ещё не опубликованы (XML дата {xmlDate})");
                    return false;
                }

                if (rates.Count == 0)
                {
                    _logger.LogWarning($"Нет данных по курсам за {date:dd.MM.yyyy}");
                    return false;
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
                _logger.LogInformation($"LoadRatesForDateAsync: date={date}, rates.Count={rates.Count}");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при загрузке курсов за {date:dd.MM.yyyy}");
                return false;
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

            var existingRates = await _dbContext.ExchangeRates
                .Where(r => r.Date >= monthAgo && r.Date <= today)
                .Select(r => r.Date)
                .Distinct()
                .ToListAsync();

            var processed = await _dbContext.ProcessedDates
                .Where(r => r.Date >= monthAgo && r.Date <= today)
                .Select(r => r.Date)
                .ToListAsync();

            var processedDates = existingRates.Union(processed).ToHashSet();

            for (var date = monthAgo; date <= today; date = date.AddDays(1))
            {
                if (processedDates.Contains(date))
                    continue;

                _logger.LogInformation($"Загружаем курсы за {date:dd.MM.yyyy}…");
                await LoadRatesForDateAsync(date);

                bool hasRates = await LoadRatesForDateAsync(date);
                if (hasRates)
                {
                    await MarkDateAsync(date, ProcessedDateStatus.Ok);
                    _logger.LogInformation($"Курсы за {date:dd.MM.yyyy} обновлены.");
                }
                else
                {
                    await MarkDateAsync(date, ProcessedDateStatus.Empty);
                }
            }
        }
        /// <summary>
        /// Помечает указанную дату как обработанную с заданным статусом.
        /// </summary>
        private async Task MarkDateAsync(DateTime date, ProcessedDateStatus status)
        {
            _dbContext.ProcessedDates.Add(new ProcessedDateEntity
            {
                Date = date,
                Status = status
            });

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation(
                $"Дата {date:dd.MM.yyyy} помечена как {status}.");
        }
    }
}

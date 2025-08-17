using System;
using System.Threading.Tasks;

namespace CbrApp.Services
{
    public interface ICurrencyRateService
    {
        Task <bool>LoadRatesForDateAsync(DateTime date);
        Task UpdateRatesAsync();
    }
}
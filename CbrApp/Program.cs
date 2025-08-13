using CbrApp.Data;
using CbrApp.Options;
using CbrApp.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        // кодировка Windows-1251 для чтение xml ЦБ РФ
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        using var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                // Настройки БД
                services.Configure<DatabaseOptions>(context.Configuration.GetSection("Database"));
                var dbOptions = context.Configuration.GetSection("Database").Get<DatabaseOptions>();
                services.AddDbContext<AppDbContext>(options =>
                    options.UseNpgsql(dbOptions.GetConnectionString()));

                // HttpClient для ЦБ
                services.AddHttpClient<CbrClient>(client =>
                {
                    client.BaseAddress = new Uri(context.Configuration["CbrApi:BaseUrl"]);
                    client.Timeout = TimeSpan.FromSeconds(10);
                });

                services.AddLogging(logging => logging.AddConsole());

                services.AddTransient<ICurrencyRateService, CurrencyRateService>();

                services.AddTransient<AppRunner>();
            })
            .Build();

        using var scope = host.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<AppRunner>().RunAppAsync();
    }
}
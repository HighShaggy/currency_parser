using CbrApp.Data;
using CbrApp.Options;
using CbrApp.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Text;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        // кодировка Windows-1251 для чтение xml ЦБ РФ
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var builder = Host.CreateDefaultBuilder(args);

        // режим для работы сервиса на Windows
        if (OperatingSystem.IsWindows() && !Environment.UserInteractive)
        {
            builder = builder.UseWindowsService();
        }

        using var host = builder
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

                services.AddQuartz(q =>
                {
                    var jobKey = new JobKey("DailyJob");
                    q.AddJob<DailyAppRunner>(opts => opts.WithIdentity(jobKey));

                    var schedule = context.Configuration["Quartz:Schedule"];
                    q.AddTrigger(opts => opts
                        .ForJob(jobKey)
                        .WithIdentity("DailyJobTrigger")
                        .WithSchedule(CronScheduleBuilder.CronSchedule(schedule)));
                });
                services.AddQuartzHostedService();
            })
            .Build();

        await host.RunAsync();
    }
}
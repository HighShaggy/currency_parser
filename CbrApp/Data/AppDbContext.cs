using CbrApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CbrApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<CurrencyEntity> Currencies { get; set; }

        public DbSet<ExchangeRateEntity> ExchangeRates { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CurrencyEntity>(entity =>
            {
                entity.ToTable("currencies");
                entity.HasKey(c => c.NumCode);

                entity.Property(c => c.NumCode).HasColumnName("num_code");
                entity.Property(c => c.CharCode).HasColumnName("char_code");
                entity.Property(c => c.Name).HasColumnName("name");
            });

            modelBuilder.Entity<ExchangeRateEntity>(entity =>
            {
                entity.ToTable("exchange_rates");
                entity.HasKey(r => new { r.Date, r.CurrencyNumCode });
                entity.Property(r => r.Date).HasColumnType("timestamp without time zone");

                entity.Property(r => r.CurrencyNumCode).HasColumnName("currency_num_code");
                entity.Property(r => r.Date).HasColumnName("date");
                entity.Property(r => r.Nominal).HasColumnName("nominal");
                entity.Property(r => r.Value).HasColumnName("value");
            });
        }
    }
}

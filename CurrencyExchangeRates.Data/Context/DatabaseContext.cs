using CurrencyExchangeRates.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CurrencyExchangeRates.Data.Context;

public class DatabaseContext : DbContext
{
    public DbSet<Currency> Currencies { get; set; }
    public DbSet<CurrencyExchangeRate> CurrencyExchangeRates { get; set; }

    public DatabaseContext() : base()
    {
        
    }
    
    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
    {
        
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Currency>(b =>
        {
            b.HasIndex(x => x.Code)
                .IsUnique();
        });
        
        modelBuilder.Entity<CurrencyExchangeRate>(
            builder =>
            {
                builder.HasIndex(x => x.EffectiveDate);
                builder.HasIndex(x => x.ForDate);
                
                builder.Property(x => x.AvarageRate)
                    .HasPrecision(20, 10);
                
                builder.Property(x => x.SaleRate)
                    .HasPrecision(20, 10);
                
                builder.Property(x => x.PurchaseRate)
                    .HasPrecision(20, 10);
                
                builder.HasOne(x => x.Currency)
                    .WithMany(x => x.CurrencyExchangeRates)
                    .HasForeignKey(x => x.CurrencyId);
            });

    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistance;
using Domain.Products;

namespace Web.API.Extensions
{
    public static class MigrationExtensions
    {
        public static void ApplyMigrations(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            try
            {
                dbContext.Database.Migrate();
                

                dbContext.Database.ExecuteSqlRaw(@"
                    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Products]') AND name = 'Sku')
                    BEGIN
                        ALTER TABLE [Products] DROP COLUMN [Sku];
                    END
                ");
                
                dbContext.Database.ExecuteSqlRaw(@"
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Products]') AND name = 'IsSold')
                    BEGIN
                        ALTER TABLE [Products] ADD [IsSold] bit NOT NULL DEFAULT 0;
                    END
                ");
                
                SeedData(dbContext);
            }
            catch (Exception ex)
            {
                var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger("MigrationExtensions");
                logger.LogError(ex, "An error occurred while applying migrations.");
                throw;
            }
        }

        private static void SeedData(ApplicationDbContext dbContext)
        {
            if (dbContext.Products.Any())
            {
                return;
            }

            var products = new[]
            {
                new Product(new ProductId(Guid.Parse("11111111-1111-1111-1111-111111111111")), "Laptop", new Money("USD", 999.99m)),
                new Product(new ProductId(Guid.Parse("22222222-2222-2222-2222-222222222222")), "Smartphone", new Money("USD", 699.99m)),
                new Product(new ProductId(Guid.Parse("33333333-3333-3333-3333-333333333333")), "Tablet", new Money("USD", 399.99m)),
                new Product(new ProductId(Guid.Parse("44444444-4444-4444-4444-444444444444")), "Headphones", new Money("USD", 199.99m)),
                new Product(new ProductId(Guid.Parse("55555555-5555-5555-5555-555555555555")), "Keyboard", new Money("USD", 79.99m)),
                new Product(new ProductId(Guid.Parse("66666666-6666-6666-6666-666666666666")), "Mouse", new Money("USD", 49.99m)),
                new Product(new ProductId(Guid.Parse("77777777-7777-7777-7777-777777777777")), "Monitor", new Money("USD", 299.99m)),
                new Product(new ProductId(Guid.Parse("88888888-8888-8888-8888-888888888888")), "Webcam", new Money("USD", 89.99m)),
                new Product(new ProductId(Guid.Parse("99999999-9999-9999-9999-999999999999")), "Speaker", new Money("USD", 149.99m)),
                new Product(new ProductId(Guid.Parse("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA")), "USB Drive", new Money("USD", 19.99m))
            };

            dbContext.Products.AddRange(products);
            dbContext.SaveChanges();
        }
    }
}

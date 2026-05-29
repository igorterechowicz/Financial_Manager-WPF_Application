using Microsoft.EntityFrameworkCore;
using wpf_projekt.Data;
using wpf_projekt.Entities;

namespace wpf_projekt.Services
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync()
        {
            using var context = new AppDbContext();
            await context.Database.MigrateAsync();
            await EnsureSeedUserHasCredentialsAsync(context);
            await SeedInitialDataAsync(context);
        }

        private static async Task EnsureSeedUserHasCredentialsAsync(AppDbContext context)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == string.Empty);
            if (user == null) return;
            user.Email = "test@example.com";
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test123");
            await context.SaveChangesAsync();
        }

        private static async Task SeedInitialDataAsync(AppDbContext context)
        {
            if (await context.Users.AnyAsync()) return;

            context.TransactionTypes.AddRange(
                new TransactionType { Name = "Jedzenie" },
                new TransactionType { Name = "Transport" },
                new TransactionType { Name = "Wypłata" },
                new TransactionType { Name = "Rozrywka" },
                new TransactionType { Name = "Transfer" }
            );

            var user = new User
            {
                FirstName = "Jan", LastName = "Kowalski", Earnings = 5000,
                Email = "test@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test123")
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            context.PersonalAccounts.Add(new PersonalAccount
                { Name = "Konto główne", Balance = 2500, UserId = user.Id });
            context.SharedAccounts.Add(new SharedAccount
                { Name = "Konto wspólne", Balance = 1200, User1Id = user.Id, User2Id = user.Id });
            await context.SaveChangesAsync();
        }
    }
}

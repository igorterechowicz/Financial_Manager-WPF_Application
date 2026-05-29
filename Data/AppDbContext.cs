using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpf_projekt.Entities;

namespace wpf_projekt.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<PersonalAccount> PersonalAccounts { get; set; }
        public DbSet<SharedAccount> SharedAccounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<TransactionType> TransactionTypes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "finance_manager.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SharedAccount>()
                .HasOne(s => s.User1)
                .WithMany()
                .HasForeignKey(s => s.User1Id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SharedAccount>()
                .HasOne(s => s.User2)
                .WithMany()
                .HasForeignKey(s => s.User2Id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
        }
    }
}

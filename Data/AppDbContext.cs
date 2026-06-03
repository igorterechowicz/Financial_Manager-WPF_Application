using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using wpf_projekt.Entities;
using wpf_projekt.Models;

namespace wpf_projekt.Data
{
    public class AppDbContext : DbContext
    {

        public AppDbContext() { }

        public DbSet<User> Users { get; set; }
        public DbSet<PersonalAccount> PersonalAccounts { get; set; }
        public DbSet<SharedAccount> SharedAccounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<TransactionType> TransactionTypes { get; set; }
        public DbSet<EventLog> EventLogs { get; set; }

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

using Microsoft.EntityFrameworkCore;
using FinPayDashboard.Api.Models.Entities;

namespace FinPayDashboard.Api.Data
{
    public class FinPayDbContext : DbContext
    {
        public FinPayDbContext(DbContextOptions<FinPayDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<CreditCard> CreditCards { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Reward> Rewards { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User relationships
            modelBuilder.Entity<User>()
                .HasMany(u => u.CreditCards)
                .WithOne(c => c.User)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Rewards)
                .WithOne(r => r.User)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // CreditCard relationships
            modelBuilder.Entity<CreditCard>()
                .HasMany(c => c.Transactions)
                .WithOne(t => t.CreditCard)
                .HasForeignKey(t => t.CardId)
                .OnDelete(DeleteBehavior.Cascade);

            // Transaction relationships
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Category)
                .WithMany(c => c.Transactions)
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            // Decimal precision
            modelBuilder.Entity<Transaction>()
                .Property(t => t.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<CreditCard>()
                .Property(c => c.CreditLimit)
                .HasPrecision(18, 2);

            modelBuilder.Entity<CreditCard>()
                .Property(c => c.CurrentBalance)
                .HasPrecision(18, 2);

            modelBuilder.Entity<CreditCard>()
                .Property(c => c.AvailableBalance)
                .HasPrecision(18, 2);

        }
    }
}
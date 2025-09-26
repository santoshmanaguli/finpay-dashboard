  using Microsoft.EntityFrameworkCore;
  using FinPayDashboard.Api.Models.Entities;
  using FinPayDashboard.Api.Data.Configurations;

  namespace FinPayDashboard.Api.Data
  {
      public class FinPayDbContext : DbContext
      {
          public FinPayDbContext(DbContextOptions<FinPayDbContext> options) : base(options)
          {
          }

          // DbSets
          public DbSet<User> Users { get; set; }
          public DbSet<CreditCard> CreditCards { get; set; }
          public DbSet<Transaction> Transactions { get; set; }
          public DbSet<Category> Categories { get; set; }
          public DbSet<Reward> Rewards { get; set; }

          protected override void OnModelCreating(ModelBuilder modelBuilder)
          {
              base.OnModelCreating(modelBuilder);

              // Apply configurations
              modelBuilder.ApplyConfiguration(new UserConfiguration());

              // Configure relationships
              modelBuilder.Entity<CreditCard>()
                  .HasOne(c => c.User)
                  .WithMany(u => u.CreditCards)
                  .HasForeignKey(c => c.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

              modelBuilder.Entity<Transaction>()
                  .HasOne(t => t.CreditCard)
                  .WithMany(c => c.Transactions)
                  .HasForeignKey(t => t.CardId)
                  .OnDelete(DeleteBehavior.Cascade);

              modelBuilder.Entity<Transaction>()
                  .HasOne(t => t.Category)
                  .WithMany(c => c.Transactions)
                  .HasForeignKey(t => t.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);

              modelBuilder.Entity<Reward>()
                  .HasOne(r => r.User)
                  .WithMany(u => u.Rewards)
                  .HasForeignKey(r => r.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

              modelBuilder.Entity<Reward>()
                  .HasOne(r => r.Transaction)
                  .WithMany(t => t.Rewards)
                  .HasForeignKey(r => r.TransactionId)
                  .OnDelete(DeleteBehavior.SetNull);

              // Seed initial data
              SeedData(modelBuilder);
          }

          private static void SeedData(ModelBuilder modelBuilder)
          {
              // Seed Categories
              var categories = new[]
              {
                  new Category
                  {
                      Id = "cat-1",
                      Name = "Food & Dining",
                      Description = "Restaurants, cafes, and food delivery",
                      IconUrl = "🍽️",
                      Color = "#FF6B6B"
                  },
                  new Category
                  {
                      Id = "cat-2",
                      Name = "Transportation",
                      Description = "Gas, public transport, rideshares",
                      IconUrl = "🚗",
                      Color = "#4ECDC4"
                  },
                  new Category
                  {
                      Id = "cat-3",
                      Name = "Shopping",
                      Description = "Retail, online shopping, clothing",
                      IconUrl = "🛍️",
                      Color = "#45B7D1"
                  },
                  new Category
                  {
                      Id = "cat-4",
                      Name = "Entertainment",
                      Description = "Movies, games, subscriptions",
                      IconUrl = "🎬",
                      Color = "#96CEB4"
                  },
                  new Category
                  {
                      Id = "cat-5",
                      Name = "Bills & Utilities",
                      Description = "Electricity, water, internet, phone",
                      IconUrl = "📄",
                      Color = "#FFEAA7"
                  }
              };

              modelBuilder.Entity<Category>().HasData(categories);
          }
      }
  }
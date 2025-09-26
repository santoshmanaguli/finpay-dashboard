  using Microsoft.EntityFrameworkCore;
  using Microsoft.EntityFrameworkCore.Metadata.Builders;
  using FinPayDashboard.Api.Models.Entities;

  namespace FinPayDashboard.Api.Data.Configurations
  {
      public class UserConfiguration : IEntityTypeConfiguration<User>
      {
          public void Configure(EntityTypeBuilder<User> builder)
          {
              builder.HasKey(u => u.Id);

              builder.Property(u => u.Email)
                     .IsRequired()
                     .HasMaxLength(100);

              builder.HasIndex(u => u.Email)
                     .IsUnique();

              builder.Property(u => u.FirstName)
                     .IsRequired()
                     .HasMaxLength(50);

              builder.Property(u => u.LastName)
                     .IsRequired()
                     .HasMaxLength(50);

              builder.Property(u => u.CreatedAt)
                     .HasDefaultValueSql("GETUTCDATE()");

              builder.Property(u => u.UpdatedAt)
                     .HasDefaultValueSql("GETUTCDATE()");
          }
      }
  }
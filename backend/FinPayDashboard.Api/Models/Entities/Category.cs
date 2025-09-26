  using System.ComponentModel.DataAnnotations;

  namespace FinPayDashboard.Api.Models.Entities
  {
      public class Category
      {
          [Key]
          public string Id { get; set; } = Guid.NewGuid().ToString();

          [Required]
          [MaxLength(50)]
          public string Name { get; set; } = string.Empty;

          [MaxLength(200)]
          public string Description { get; set; } = string.Empty;

          [MaxLength(100)]
          public string IconUrl { get; set; } = string.Empty;

          [MaxLength(7)] // Hex color code
          public string Color { get; set; } = "#000000";

          public bool IsActive { get; set; } = true;

          public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

          // Navigation properties
          public virtual ICollection<Transaction> Transactions { get; set; } = new
  List<Transaction>();
      }
  }
  using System.ComponentModel.DataAnnotations;
  using System.ComponentModel.DataAnnotations.Schema;

  namespace FinPayDashboard.Api.Models.Entities
  {
      public class Transaction
      {
          [Key]
          public string Id { get; set; } = Guid.NewGuid().ToString();

          [Required]
          public string CardId { get; set; } = string.Empty;

          [Column(TypeName = "decimal(18,2)")]
          public decimal Amount { get; set; }

          [Required]
          [MaxLength(200)]
          public string Description { get; set; } = string.Empty;

          public string? CategoryId { get; set; }

          [MaxLength(100)]
          public string MerchantName { get; set; } = string.Empty;

          public DateTime TransactionDate { get; set; }

          [Required]
          [MaxLength(20)]
          public string Status { get; set; } = "Completed"; // Completed, Pending, Failed

          [MaxLength(50)]
          public string TransactionType { get; set; } = "Purchase"; // Purchase, Refund, Payment

          public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

          // Navigation properties
          [ForeignKey(nameof(CardId))]
          public virtual CreditCard CreditCard { get; set; } = null!;

          [ForeignKey(nameof(CategoryId))]
          public virtual Category Category { get; set; } = null!;

          public virtual ICollection<Reward> Rewards { get; set; } = new List<Reward>();
      }
  }
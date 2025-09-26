  using System.ComponentModel.DataAnnotations;
  using System.ComponentModel.DataAnnotations.Schema;

  namespace FinPayDashboard.Api.Models.Entities
  {
      public class Reward
      {
          [Key]
          public string Id { get; set; } = Guid.NewGuid().ToString();

          [Required]
          public string UserId { get; set; } = string.Empty;

          public string? TransactionId { get; set; }

          public int PointsEarned { get; set; } = 0;

          public int PointsRedeemed { get; set; } = 0;

          [MaxLength(100)]
          public string RewardType { get; set; } = "Purchase"; // Purchase, Bonus, Referral

          [MaxLength(200)]
          public string Description { get; set; } = string.Empty;

          public DateTime EarnedDate { get; set; } = DateTime.UtcNow;

          public DateTime? RedeemedDate { get; set; }

          // Navigation properties
          [ForeignKey(nameof(UserId))]
          public virtual User User { get; set; } = null!;

          [ForeignKey(nameof(TransactionId))]
          public virtual Transaction? Transaction { get; set; }
      }
  }
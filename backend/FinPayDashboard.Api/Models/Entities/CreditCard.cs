using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinPayDashboard.Api.Models.Entities
{
    public class CreditCard
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [MaxLength]
        public string CardNumberLastFour { get; set; } = string.Empty;

        [Required]
        public string CardHolderName { get; set; } = string.Empty;

        [Required]
        public DateTime ExpiryDate { get; set; }

        [Required]
        [MaxLength(20)]
        public string CardType { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal CreditLimit { get; set; }

        [Column(TypeName = "decimal(18,2)")]
          public decimal AvailableBalance { get; set; }

          [Column(TypeName = "decimal(18,2)")]
          public decimal CurrentBalance { get; set; }

          public bool IsActive { get; set; } = true;

          public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
          public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

          // Navigation properties
          [ForeignKey(nameof(UserId))]
          public virtual User User { get; set; } = null!;
          public virtual ICollection<Transaction> Transactions { get; set; } = new
  List<Transaction>();
    }
}
using System.ComponentModel.DataAnnotations;

namespace FinPayDashboard.Api.Models.Entities{
	public class User
	{
		[Key]
		public string Id { get; set; } = Guid.NewGuid().ToString();

		[Required]
		[MaxLength(100)]
		public string Email { get; set; } = string.Empty;

		[Required]
		[MaxLength(50)]
		public string FirstName { get; set; } = string.Empty;

		[Required]
		[MaxLength(50)]
		public string LastName { get; set; } = string.Empty;

		[MaxLength(10)]
		public string? PhoneNumber { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
		public virtual ICollection<CreditCard> CreditCards { get; set; } = new List<CreditCard>();
		public virtual ICollection<Reward> Rewards { get; set; } = new List<Reward>();
	}
}

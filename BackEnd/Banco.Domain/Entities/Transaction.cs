using System.ComponentModel.DataAnnotations;

namespace Banco.Domain.Entities
{
    public class Transaction
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime Date { get; set; } = DateTime.UtcNow;
        [Required]
        public string TransactionType { get; set; } 
        public decimal Amount { get; set; }
        public decimal BalanceAfter { get; set; }
        public decimal CurrentBalance { get; set; }
        public Guid AccountId { get; set; }
        public Account Account { get; set; }
    }
}

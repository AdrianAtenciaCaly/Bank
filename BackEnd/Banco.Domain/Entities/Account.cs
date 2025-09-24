using System.ComponentModel.DataAnnotations;

namespace Banco.Domain.Entities
{
    public class Account
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        public string AccountNumber { get; set; }
        [Required]
        public string AccountType { get; set; }
        public decimal InitialBalance { get; set; }
        public decimal CurrentBalance { get; set; }
        public bool IsActive { get; set; } = true;
        public Guid CustomerId { get; set; }
        public Customer Customer { get; set; }
    }
}

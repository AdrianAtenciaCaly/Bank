using System.ComponentModel.DataAnnotations;

namespace Banco.Domain.Entities
{
    public class Customer 
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public string CustomerCode { get; set; }
        public string PasswordHash { get; set; }
        public bool IsActive { get; set; } = true;

        public Guid PersonId { get; set; }
        public Person Person { get; set; }
    }
}

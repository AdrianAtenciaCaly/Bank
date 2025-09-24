using System.ComponentModel.DataAnnotations;

namespace Banco.Domain.Entities
{
    public class Person
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; }
        [MaxLength(100)]
        public string LastName { get; set; }
        [MaxLength(20)]
        public string Gender { get; set; }
        public int Age { get; set; }
        [Required]
        [MaxLength(50)]
        public string Identification { get; set; }
        [MaxLength(200)]
        public string Address { get; set; }
        [MaxLength(50)]
        public string Phone { get; set; }

        public ICollection<Customer> Customers { get; set; }
    }

}

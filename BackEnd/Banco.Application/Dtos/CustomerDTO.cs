namespace Banco.Application.Dtos
{
    public class CustomerDTO
    {
        public Guid Id { get; set; }
        public string CustomerCode { get; set; }
        public string Password { get; set; }
        public bool IsActive { get; set; } = true;
        public string PersonId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }
}

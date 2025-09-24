namespace Banco.Application.Dtos
{
    public class AccountDTO
    {
        public Guid Id { get; set; }
        public string AccountNumber { get; set; }
        public string AccountType { get; set; }
        public decimal InitialBalance { get; set; }
        public decimal? CurrentBalance { get; set; }
        public bool IsActive { get; set; } = true;
        public Guid CustomerId { get; set; }
        public string? CustomerCode { get; set; }
        public string? PersonId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

    }
}

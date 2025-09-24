namespace Banco.Application.Dtos
{
    public class MovementDTO
    {
        public Guid Id { get; set; }
        public Guid AccountId { get; set; }
        public string MovementType { get; set; }
        public decimal Amount { get; set; }
        public decimal Balance { get; set; }
        public DateTime Date { get; set; }
        public string? AccountNumber { get; set; }
        public string? FullName { get; set; }
    }

}

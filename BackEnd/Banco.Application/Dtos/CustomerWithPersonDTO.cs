namespace Banco.Application.Dtos
{
    public class CustomerWithPersonDTO
    {
        public Guid Id { get; set; }
        public string CustomerCode { get; set; }
        public bool IsActive { get; set; }
        public Guid PersonId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}

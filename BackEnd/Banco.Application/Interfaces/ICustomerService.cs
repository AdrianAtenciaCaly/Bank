using Banco.Application.Dtos;

namespace Banco.Application.Interfaces
{
    public interface ICustomerService
    {

        Task<(bool, string, CustomerDTO)> CreateAsync(CustomerDTO dto);
        Task<IEnumerable<CustomerDTO>> GetAllAsync();
        Task<CustomerDTO> GetByIdAsync(Guid id);
        Task<(bool IsSuccess, string Message)> UpdateAsync(Guid id, CustomerDTO dto);
        Task<(bool IsSuccess, string Message)> DeleteAsync(Guid id);
        Task<IEnumerable<CustomerWithPersonDTO>> GetAllWithPersonAsync();
    }
}

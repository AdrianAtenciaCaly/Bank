using Banco.Application.Dtos;

namespace Banco.Application.Interfaces
{
    public interface IPersonService
    {
        Task<PersonDTO> GetByIdAsync(Guid id);
        Task<IEnumerable<PersonDTO>> GetAllAsync();
        Task<(bool IsSuccess, string Message, PersonDTO Data)> CreateAsync(PersonDTO dto);
        Task<(bool IsSuccess, string Message)> UpdateAsync(Guid id, PersonDTO dto);
        Task<(bool IsSuccess, string Message)> DeleteAsync(Guid id);
    }
}

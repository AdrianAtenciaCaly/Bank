using Banco.Application.Dtos;

namespace Banco.Application.Interfaces
{
    public interface IAccountService
    {
        Task<(bool, string, AccountDTO)> CreateAsync(AccountDTO dto);
        Task<IEnumerable<AccountDTO>> GetAllAsync();
        Task<AccountDTO> GetByIdAsync(Guid id);
        Task<(bool, string)> UpdateAsync(Guid id, AccountDTO dto);
        Task<(bool, string)> DeleteAsync(Guid id);
    }
}

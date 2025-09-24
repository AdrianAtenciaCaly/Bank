using Banco.Application.Dtos;

namespace Banco.Application.Interfaces
{
    public interface IMovementService
    {
        Task<(bool, string, MovementDTO)> CreateAsync(MovementDTO dto);
        Task<IEnumerable<MovementDTO>> GetAllAsync();
        Task<IEnumerable<MovementDTO>> GetByFilterAsync(Guid? accountId = null, DateTime? start = null, DateTime? end = null);
        Task<bool> DeleteAsync(Guid id);
    }
}

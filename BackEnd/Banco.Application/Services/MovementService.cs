using Banco.Application.Dtos;
using Banco.Application.Interfaces;
using Banco.Domain.Entities;
using Banco.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Banco.Application.Services
{
    public class MovementService : IMovementService
    {
        private readonly IUnitOfWork _uow;
        private const decimal DAILY_LIMIT = 1000;

        public MovementService(IUnitOfWork uow) => _uow = uow;

        public async Task<(bool, string, MovementDTO)> CreateAsync(MovementDTO dto)
        {
            var repoMov = _uow.Repository<Transaction>();
            var repoAcc = _uow.Repository<Account>();
            var account = await repoAcc.GetByIdAsync(dto.AccountId);

            if (account == null) return (false, "Cuenta no encontrada", null);

            var movimientos = await repoMov.FindAsync(m => ((Transaction)m).AccountId == dto.AccountId);
            var ultimoSaldo = movimientos.Any()
                ? movimientos.OrderByDescending(m => ((Transaction)m).Date).First().BalanceAfter
                : account.InitialBalance;

            decimal nuevoSaldo = ultimoSaldo;

            if (dto.MovementType.Equals("Debito", StringComparison.OrdinalIgnoreCase))
            {
                if (ultimoSaldo <= 0)
                    return (false, "Saldo no disponible", null);

                if (dto.Amount > ultimoSaldo)
                    return (false, "Saldo insuficiente", null);

                var hoy = DateTime.UtcNow.Date;
                var retirosHoy = movimientos
                    .Where(m => ((Transaction)m).Date.Date == hoy && ((Transaction)m).TransactionType == "Debito")
                    .Sum(m => Math.Abs(((Transaction)m).Amount));

                if (retirosHoy + dto.Amount > DAILY_LIMIT)
                    return (false, "Cupo diario excedido", null);

                nuevoSaldo -= dto.Amount;
                dto.Amount = -Math.Abs(dto.Amount);
            }
            else if (dto.MovementType.Equals("Credito", StringComparison.OrdinalIgnoreCase))
            {
                nuevoSaldo += dto.Amount;
                dto.Amount = Math.Abs(dto.Amount);
            }

            var movement = new Transaction
            {
                AccountId = dto.AccountId,
                TransactionType = dto.MovementType,
                Amount = dto.Amount,
                BalanceAfter = nuevoSaldo,
                CurrentBalance = nuevoSaldo,
                Date = DateTime.UtcNow
            };

            await repoMov.AddAsync(movement);

            account.CurrentBalance = nuevoSaldo;
            repoAcc.Update(account);

            await _uow.SaveChangesAsync();

            dto.Id = movement.Id;
            dto.Balance = nuevoSaldo;
            dto.Date = movement.Date;

            return (true, "Movimiento registrado", dto);
        }

        public async Task<IEnumerable<MovementDTO>> GetAllAsync()
        {
            var repoMov = _uow.Repository<Transaction>();
            var query = repoMov.GetQueryable()
                     .Include(m => m.Account)
                         .ThenInclude(a => a.Customer)
                             .ThenInclude(c => c.Person);

            var movimientos = await query.ToListAsync();

            return movimientos.Select(m => new MovementDTO
            {
                Id = m.Id,
                AccountId = m.AccountId,
                Amount = m.Amount,
                Balance = m.BalanceAfter,
                Date = m.Date,
                MovementType = m.TransactionType,
                AccountNumber = m.Account.AccountNumber,
                FullName = $"{m.Account.Customer.Person.FirstName} {m.Account.Customer.Person.LastName}"

            });
        }

        public async Task<IEnumerable<MovementDTO>> GetByFilterAsync(Guid? accountId = null, DateTime? start = null, DateTime? end = null)
        {
            var repoMov = _uow.Repository<Transaction>();
            var query = repoMov.GetQueryable();

            if (accountId.HasValue)
                query = query.Where(m => m.AccountId == accountId.Value);

            if (start.HasValue)
                query = query.Where(m => m.Date >= start.Value);

            if (end.HasValue)
                query = query.Where(m => m.Date <= end.Value);

            var movimientos = await query.ToListAsync();

            return movimientos.Select(m => new MovementDTO
            {
                Id = m.Id,
                AccountId = m.AccountId,
                Amount = m.Amount,
                Balance = m.BalanceAfter,
                Date = m.Date,
                MovementType = m.TransactionType
            });
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var repoMov = _uow.Repository<Transaction>();
            var mov = await repoMov.GetByIdAsync(id);
            if (mov == null) return false;

            repoMov.Remove(mov);
            await _uow.SaveChangesAsync();
            return true;
        }
    }
}

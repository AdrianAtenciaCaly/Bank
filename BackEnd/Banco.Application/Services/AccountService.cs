using Banco.Application.Dtos;
using Banco.Application.Interfaces;
using Banco.Domain.Entities;
using Banco.Infrastructure.Data;
using Banco.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Banco.Application.Services
{
    public class AccountService : IAccountService
    {
        private readonly IUnitOfWork _uow;
        private readonly AppDbContext _context;

        public AccountService(IUnitOfWork uow, AppDbContext context)
        {
            _uow = uow;
            _context = context;
        }

        public async Task<(bool, string, AccountDTO)> CreateAsync(AccountDTO dto)
        {
            var repo = _uow.Repository<Account>();
            var acc = new Account
            {
                Id = new Guid(),
                AccountNumber = dto.AccountNumber,
                AccountType = dto.AccountType,
                InitialBalance = dto.InitialBalance,
                CustomerId = dto.CustomerId,
                IsActive = dto.IsActive
            };

            await repo.AddAsync(acc);
            await _uow.SaveChangesAsync();
            dto.Id = acc.Id;
            return (true, "Cuenta creada", dto);
        }

        public async Task<IEnumerable<AccountDTO>> GetAllAsync()
        {
            var list = await _context.Accounts.Include(a => a.Customer).ThenInclude(c => c.Person).ToListAsync();

            return list.Select(a => new AccountDTO
            {
                Id = a.Id,
                AccountNumber = a.AccountNumber,
                AccountType = a.AccountType,
                InitialBalance = a.InitialBalance,
                CurrentBalance = a.CurrentBalance,
                IsActive = a.IsActive,
                CustomerId = a.CustomerId,
                CustomerCode = a.Customer?.CustomerCode,
                FirstName = a.Customer?.Person?.FirstName,
                LastName = a.Customer?.Person?.LastName,
                PersonId = a.Customer.Person.Id.ToString()
            });
        }

        public async Task<AccountDTO> GetByIdAsync(Guid id)
        {
            var a = await _context.Accounts.Include(ac => ac.Customer).ThenInclude(c => c.Person).FirstOrDefaultAsync(ac => ac.Id == id);

            if (a == null) return null;

            return new AccountDTO
            {
                Id = a.Id,
                AccountNumber = a.AccountNumber,
                AccountType = a.AccountType,
                InitialBalance = a.InitialBalance,
                CurrentBalance = a.CurrentBalance,
                IsActive = a.IsActive,
                CustomerId = a.CustomerId,
                CustomerCode = a.Customer?.CustomerCode,
                FirstName = a.Customer?.Person?.FirstName,
                LastName = a.Customer?.Person?.LastName,
                PersonId = a.Customer.Person.Id.ToString()
            };
        }

        public async Task<(bool, string)> UpdateAsync(Guid id, AccountDTO dto)
        {
            var repo = _uow.Repository<Account>();
            var entity = await repo.GetByIdAsync(id);
            if (entity == null) return (false, "Cuenta no encontrada");
            entity.AccountNumber = dto.AccountNumber;
            entity.AccountType = dto.AccountType;
            entity.IsActive = dto.IsActive;
            repo.Update(entity);
            await _uow.SaveChangesAsync();
            return (true, "Cuenta actualizada");
        }

        public async Task<(bool, string)> DeleteAsync(Guid id)
        {
            var repo = _uow.Repository<Account>();
            var e = await repo.GetByIdAsync(id);
            if (e == null) return (false, "Cuenta no encontrada");
            repo.Remove(e);
            await _uow.SaveChangesAsync();
            return (true, "Cuenta eliminada");
        }
    }
}
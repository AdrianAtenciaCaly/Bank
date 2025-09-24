using Banco.Application.Dtos;
using Banco.Application.Interfaces;
using Banco.Domain.Entities;
using Banco.Infrastructure.Interfaces;

namespace Banco.Application.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly IUnitOfWork _uow;

        public CustomerService(IUnitOfWork uow) => _uow = uow;

        public async Task<(bool, string, CustomerDTO)> CreateAsync(CustomerDTO dto)
        {
            var repoCustomer = _uow.Repository<Customer>();
            var repoPerson = _uow.Repository<Person>();

            var existingCustomer = await repoCustomer.FindAsync(c => ((Customer)c).CustomerCode == dto.CustomerCode);
            if (existingCustomer.Any())
                return (false, "Ya existe un cliente con este código", null);

            if (!Guid.TryParse(dto.PersonId, out Guid personGuid))
                return (false, "PersonId inválido", null);
            var existingCustomerForPerson = await repoCustomer.FindAsync(c => ((Customer)c).PersonId == personGuid);
            if (existingCustomerForPerson.Any())
                return (false, "Ya existe un cliente asociado a esta persona", null);

            var customer = new Customer
            {
                CustomerCode = dto.CustomerCode,
                PasswordHash = dto.Password,
                IsActive = dto.IsActive,
                PersonId = personGuid
            };

            await repoCustomer.AddAsync(customer);
            await _uow.SaveChangesAsync();

            dto.Id = customer.Id;
            return (true, "Cliente creado", dto);
        }

        public async Task<IEnumerable<CustomerDTO>> GetAllAsync()
        {
            var repoCustomer = _uow.Repository<Customer>();
            var repoPerson = _uow.Repository<Person>();

            var customers = await repoCustomer.GetAllAsync();

            var result = new List<CustomerDTO>();

            foreach (var c in customers)
            {
                var person = await repoPerson.GetByIdAsync(c.PersonId);

                result.Add(new CustomerDTO
                {
                    Id = c.Id,
                    CustomerCode = c.CustomerCode,
                    Password = c.PasswordHash,
                    IsActive = c.IsActive,
                    PersonId = c.PersonId.ToString(),
                    FirstName = person?.FirstName,
                    LastName = person?.LastName
                });
            }

            return result;
        }

        public async Task<CustomerDTO> GetByIdAsync(Guid id)
        {
            var repoCustomer = _uow.Repository<Customer>();
            var repoPerson = _uow.Repository<Person>();

            var c = await repoCustomer.GetByIdAsync(id);
            if (c == null) return null;

            var person = await repoPerson.GetByIdAsync(c.PersonId);

            return new CustomerDTO
            {
                Id = c.Id,
                CustomerCode = c.CustomerCode,
                Password = c.PasswordHash,
                IsActive = c.IsActive,
                PersonId = c.PersonId.ToString(),
                FirstName = person?.FirstName,
                LastName = person?.LastName
            };
        }

        public async Task<(bool IsSuccess, string Message)> DeleteAsync(Guid id)
        {
            var repoCustomer = _uow.Repository<Customer>();
            var repoAccount = _uow.Repository<Account>();

            var customer = await repoCustomer.GetByIdAsync(id);
            if (customer == null) return (false, "Customer not found.");

            var accounts = await repoAccount.FindAsync(a => a.CustomerId == id);
            foreach (var account in accounts)
                repoAccount.Remove(account);

            repoCustomer.Remove(customer);

            await _uow.SaveChangesAsync();

            return (true, "Customer deleted along with their accounts.");
        }

        public async Task<(bool IsSuccess, string Message)> UpdateAsync(Guid id, CustomerDTO dto)
        {
            var repo = _uow.Repository<Customer>();
            var entity = await repo.GetByIdAsync(id);
            if (entity == null) return (false, "Person not found.");

            entity.CustomerCode = dto.CustomerCode;
            entity.IsActive = dto.IsActive;

            repo.Update(entity);
            await _uow.SaveChangesAsync();


            return (true, "Person updated.");
        }

        public async Task<IEnumerable<CustomerWithPersonDTO>> GetAllWithPersonAsync()
        {
            var repoCustomer = _uow.Repository<Customer>();
            var repoPerson = _uow.Repository<Person>();

            var customers = await repoCustomer.GetAllAsync();
            var result = new List<CustomerWithPersonDTO>();

            foreach (var c in customers)
            {
                var person = await repoPerson.GetByIdAsync(c.PersonId);
                result.Add(new CustomerWithPersonDTO
                {
                    Id = c.Id,
                    CustomerCode = c.CustomerCode,
                    IsActive = c.IsActive,
                    PersonId = c.PersonId,
                    FirstName = person?.FirstName,
                    LastName = person?.LastName
                });
            }

            return result;
        }
    }
}

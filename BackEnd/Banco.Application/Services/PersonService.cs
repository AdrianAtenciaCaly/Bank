using Banco.Application.Dtos;
using Banco.Application.Interfaces;
using Banco.Domain.Entities;
using Banco.Infrastructure.Interfaces;

namespace Banco.Application.Services
{
    public class PersonService : IPersonService
    {
        private readonly IUnitOfWork _uow;

        public PersonService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<(bool IsSuccess, string Message, PersonDTO Data)> CreateAsync(PersonDTO dto)
        {
            var personsRepo = _uow.Repository<Person>();
            var existing = await personsRepo.FindAsync(p => ((Person)p).Identification == dto.Identification);
            if (existing.Any())
                return (false, "A person with the same identification already exists.", null);


            var person = new Person
            {
                Id = new Guid(),
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Gender = dto.Gender,
                Age = dto.Age,
                Identification = dto.Identification,
                Address = dto.Address,
                Phone = dto.Phone
            };


            await personsRepo.AddAsync(person);
            await _uow.SaveChangesAsync();


            dto.Id = person.Id;
            return (true, "Person created successfully.", dto);
        }

        public async Task<(bool IsSuccess, string Message)> DeleteAsync(Guid id)
        {
            var repo = _uow.Repository<Person>();
            var entity = await repo.GetByIdAsync(id);
            if (entity == null) return (false, "Person not found.");


            repo.Remove(entity);
            await _uow.SaveChangesAsync();


            return (true, "Person deleted.");
        }

        public async Task<IEnumerable<PersonDTO>> GetAllAsync()
        {
            var repo = _uow.Repository<Person>();
            var list = await repo.GetAllAsync();
            return list.Select(p => new PersonDTO
            {
                Id = ((Person)p).Id,
                FirstName = ((Person)p).FirstName,
                LastName = ((Person)p).LastName,
                Gender = ((Person)p).Gender,
                Age = ((Person)p).Age,
                Identification = ((Person)p).Identification,
                Address = ((Person)p).Address,
                Phone = ((Person)p).Phone
            });
        }

        public async Task<PersonDTO> GetByIdAsync(Guid id)
        {
            var repo = _uow.Repository<Person>();
            var p = await repo.GetByIdAsync(id);
            if (p == null) return null;


            return new PersonDTO
            {
                Id = p.Id,
                FirstName = p.FirstName,
                LastName = p.LastName,
                Gender = p.Gender,
                Age = p.Age,
                Identification = p.Identification,
                Address = p.Address,
                Phone = p.Phone
            };
        }

        public async Task<(bool IsSuccess, string Message)> UpdateAsync(Guid id, PersonDTO dto)
        {
            var repo = _uow.Repository<Person>();
            var entity = await repo.GetByIdAsync(id);
            if (entity == null) return (false, "Person not found.");

            entity.FirstName = dto.FirstName;
            entity.LastName = dto.LastName;
            entity.Gender = dto.Gender;
            entity.Age = dto.Age;
            entity.Address = dto.Address;
            entity.Phone = dto.Phone;


            repo.Update(entity);
            await _uow.SaveChangesAsync();


            return (true, "Person updated.");
        }
    }
}
using Banco.Application.Dtos;
using Banco.Application.Services;
using Banco.Domain.Entities;
using Banco.Infrastructure.Interfaces;
using Moq;
using System.Linq.Expressions;

namespace Test.Application
{
    public class PersonServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUow;
        private readonly Mock<IRepository<Person>> _mockRepo;
        private readonly PersonService _service;

        public PersonServiceTests()
        {
            _mockUow = new Mock<IUnitOfWork>();
            _mockRepo = new Mock<IRepository<Person>>();
            _mockUow.Setup(u => u.Repository<Person>()).Returns(_mockRepo.Object);
            _service = new PersonService(_mockUow.Object);
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnError_WhenPersonExists()
        {
            var dto = new PersonDTO { Identification = "12345" };
            _mockRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Person, bool>>>()))
                     .ReturnsAsync(new List<Person> { new Person { Identification = "12345" } });

            var result = await _service.CreateAsync(dto);

            Assert.False(result.IsSuccess);
            Assert.Equal("Ya existe una persona con la misma identificación.", result.Message);
        }

        [Fact]
        public async Task CreateAsync_ShouldCreatePerson_WhenNotExists()
        {
            var dto = new PersonDTO
            {
                Identification = "67890",
                FirstName = "John",
                LastName = "Doe"
            };

            _mockRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Person, bool>>>()))
                     .ReturnsAsync(new List<Person>());

            _mockRepo.Setup(r => r.AddAsync(It.IsAny<Person>()))
                     .Callback<Person>(p => p.Id = Guid.NewGuid())
                     .Returns(Task.CompletedTask);

            _mockUow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var result = await _service.CreateAsync(dto);

            Assert.True(result.IsSuccess);
            Assert.Equal("Persona creada correctamente.", result.Message);
            Assert.NotEqual(Guid.Empty, result.Data.Id);
        }


        [Fact]
        public async Task DeleteAsync_ShouldReturnError_WhenPersonNotFound()
        {
            _mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Person)null);

            var result = await _service.DeleteAsync(Guid.NewGuid());

            Assert.False(result.IsSuccess);
            Assert.Equal("Persona no encontrada.", result.Message);
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeletePerson_WhenExists()
        {
            var person = new Person { Id = Guid.NewGuid() };
            _mockRepo.Setup(r => r.GetByIdAsync(person.Id)).ReturnsAsync(person);
            _mockRepo.Setup(r => r.Remove(person));
            _mockUow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var result = await _service.DeleteAsync(person.Id);

            Assert.True(result.IsSuccess);
            Assert.Equal("Persona eliminada.", result.Message);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnListOfPersonDTO()
        {
            var persons = new List<Person>
            {
                new Person { Id = Guid.NewGuid(), FirstName = "Alice", LastName = "Smith" },
                new Person { Id = Guid.NewGuid(), FirstName = "Bob", LastName = "Brown" }
            };
            _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(persons);

            var result = await _service.GetAllAsync();

            Assert.Equal(2, result.Count());
            Assert.Contains(result, p => p.FirstName == "Alice");
            Assert.Contains(result, p => p.FirstName == "Bob");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenPersonNotFound()
        {
            _mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Person)null);

            var result = await _service.GetByIdAsync(Guid.NewGuid());

            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnPersonDTO_WhenPersonExists()
        {
            var person = new Person { Id = Guid.NewGuid(), FirstName = "Charlie", LastName = "Day" };
            _mockRepo.Setup(r => r.GetByIdAsync(person.Id)).ReturnsAsync(person);

            var result = await _service.GetByIdAsync(person.Id);

            Assert.NotNull(result);
            Assert.Equal("Charlie", result.FirstName);
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnError_WhenPersonNotFound()
        {
            _mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Person)null);

            var result = await _service.UpdateAsync(Guid.NewGuid(), new PersonDTO());

            Assert.False(result.IsSuccess);
            Assert.Equal("Persona no encontrada.", result.Message);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdatePerson_WhenExists()
        {
            var person = new Person { Id = Guid.NewGuid(), FirstName = "OldName" };
            var dto = new PersonDTO { FirstName = "NewName", LastName = "UpdatedLastName" };
            _mockRepo.Setup(r => r.GetByIdAsync(person.Id)).ReturnsAsync(person);
            _mockRepo.Setup(r => r.Update(person));
            _mockUow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var result = await _service.UpdateAsync(person.Id, dto);

            Assert.True(result.IsSuccess);
            Assert.Equal("Persona actualizada.", result.Message);
            Assert.Equal("NewName", person.FirstName);
            Assert.Equal("UpdatedLastName", person.LastName);
        }
    }
}

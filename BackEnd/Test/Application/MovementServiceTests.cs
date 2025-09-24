using Banco.Application.Dtos;
using Banco.Application.Services;
using Banco.Domain.Entities;
using Banco.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using System.Linq.Expressions;

namespace Test.Application
{
    public class MovementServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUow;
        private readonly Mock<IRepository<Transaction>> _mockRepoMov;
        private readonly Mock<IRepository<Account>> _mockRepoAcc;
        private readonly MovementService _service;

        public MovementServiceTests()
        {
            _mockUow = new Mock<IUnitOfWork>();
            _mockRepoMov = new Mock<IRepository<Transaction>>();
            _mockRepoAcc = new Mock<IRepository<Account>>();

            _mockUow.Setup(u => u.Repository<Transaction>()).Returns(_mockRepoMov.Object);
            _mockUow.Setup(u => u.Repository<Account>()).Returns(_mockRepoAcc.Object);

            _service = new MovementService(_mockUow.Object);
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnError_WhenAccountNotFound()
        {
            _mockRepoAcc.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Account)null);

            var dto = new MovementDTO { AccountId = Guid.NewGuid(), Amount = 100, MovementType = "Debito" };
            var result = await _service.CreateAsync(dto);

            Assert.False(result.Item1);
            Assert.Equal("Cuenta no encontrada", result.Item2);
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnError_WhenSaldoInsuficiente()
        {
            var account = new Account { Id = Guid.NewGuid(), InitialBalance = 50, CurrentBalance = 50 };
            _mockRepoAcc.Setup(r => r.GetByIdAsync(account.Id)).ReturnsAsync(account);
            _mockRepoMov.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Transaction, bool>>>()))
                        .ReturnsAsync(new List<Transaction>());

            var dto = new MovementDTO { AccountId = account.Id, Amount = 100, MovementType = "Debito" };
            var result = await _service.CreateAsync(dto);

            Assert.False(result.Item1);
            Assert.Equal("Saldo insuficiente", result.Item2);
        }

        [Fact]
        public async Task CreateAsync_ShouldCreateDebitMovement_WhenSufficientBalance()
        {
            var account = new Account { Id = Guid.NewGuid(), InitialBalance = 500, CurrentBalance = 500 };
            _mockRepoAcc.Setup(r => r.GetByIdAsync(account.Id)).ReturnsAsync(account);
            _mockRepoMov.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Transaction, bool>>>()))
                        .ReturnsAsync(new List<Transaction>());

            _mockRepoMov.Setup(r => r.AddAsync(It.IsAny<Transaction>()));
            _mockRepoAcc.Setup(r => r.Update(It.IsAny<Account>()));
            _mockUow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var dto = new MovementDTO { AccountId = account.Id, Amount = 200, MovementType = "Debito" };
            var result = await _service.CreateAsync(dto);

            Assert.True(result.Item1);
            Assert.Equal("Movimiento registrado", result.Item2);
            Assert.Equal(-200, result.Item3.Amount);
            Assert.Equal(300, result.Item3.Balance);
        }

        [Fact]
        public async Task CreateAsync_ShouldCreateCreditMovement()
        {
            var account = new Account { Id = Guid.NewGuid(), InitialBalance = 500, CurrentBalance = 500 };
            _mockRepoAcc.Setup(r => r.GetByIdAsync(account.Id)).ReturnsAsync(account);
            _mockRepoMov.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Transaction, bool>>>()))
                        .ReturnsAsync(new List<Transaction>());

            _mockRepoMov.Setup(r => r.AddAsync(It.IsAny<Transaction>()));
            _mockRepoAcc.Setup(r => r.Update(It.IsAny<Account>()));
            _mockUow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var dto = new MovementDTO { AccountId = account.Id, Amount = 150, MovementType = "Credito" };
            var result = await _service.CreateAsync(dto);

            Assert.True(result.Item1);
            Assert.Equal("Movimiento registrado", result.Item2);
            Assert.Equal(150, result.Item3.Amount);
            Assert.Equal(650, result.Item3.Balance);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnFalse_WhenTransactionNotFound()
        {
            _mockRepoMov.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Transaction)null);

            var result = await _service.DeleteAsync(Guid.NewGuid());

            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnTrue_WhenTransactionExists()
        {
            var mov = new Transaction { Id = Guid.NewGuid() };
            _mockRepoMov.Setup(r => r.GetByIdAsync(mov.Id)).ReturnsAsync(mov);
            _mockRepoMov.Setup(r => r.Remove(It.IsAny<Transaction>()));
            _mockUow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var result = await _service.DeleteAsync(mov.Id);

            Assert.True(result);
        }

        [Fact]
        public async Task GetByFilterAsync_ShouldReturnFilteredMovements()
        {
            var accountId = Guid.NewGuid();
            var movements = new List<Transaction>
            {
                new Transaction { Id = Guid.NewGuid(), AccountId = accountId, Amount = 100, Date = DateTime.UtcNow },
                new Transaction { Id = Guid.NewGuid(), AccountId = Guid.NewGuid(), Amount = 50, Date = DateTime.UtcNow }
            }.AsQueryable();

            var mockSet = new Mock<DbSet<Transaction>>();
            mockSet.As<IAsyncEnumerable<Transaction>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<Transaction>(movements.GetEnumerator()));

            mockSet.As<IQueryable<Transaction>>()
                .Setup(m => m.Provider)
                .Returns(new TestAsyncQueryProvider<Transaction>(movements.Provider));
            mockSet.As<IQueryable<Transaction>>().Setup(m => m.Expression).Returns(movements.Expression);
            mockSet.As<IQueryable<Transaction>>().Setup(m => m.ElementType).Returns(movements.ElementType);
            mockSet.As<IQueryable<Transaction>>().Setup(m => m.GetEnumerator()).Returns(movements.GetEnumerator());

            _mockRepoMov.Setup(r => r.GetQueryable()).Returns(mockSet.Object);

            var result = await _service.GetByFilterAsync(accountId);

            Assert.Single(result);
            Assert.All(result, m => Assert.Equal(accountId, m.AccountId));
        }

        internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
        {
            private readonly IEnumerator<T> _inner;
            public TestAsyncEnumerator(IEnumerator<T> inner) => _inner = inner;
            public T Current => _inner.Current;
            public ValueTask DisposeAsync() { _inner.Dispose(); return default; }
            public ValueTask<bool> MoveNextAsync() => new ValueTask<bool>(_inner.MoveNext());
        }

        internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
        {
            private readonly IQueryProvider _inner;
            public TestAsyncQueryProvider(IQueryProvider inner) => _inner = inner;

            public IQueryable CreateQuery(Expression expression) => new TestAsyncEnumerable<TEntity>(expression);
            public IQueryable<TElement> CreateQuery<TElement>(Expression expression) => new TestAsyncEnumerable<TElement>(expression);
            public object Execute(Expression expression) => _inner.Execute(expression);
            public TResult Execute<TResult>(Expression expression) => _inner.Execute<TResult>(expression);
            public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
            {
                var resultType = typeof(TResult).GenericTypeArguments[0];
                var executionResult = typeof(IQueryProvider)
                    .GetMethod(nameof(IQueryProvider.Execute), new[] { typeof(Expression) })
                    .MakeGenericMethod(resultType)
                    .Invoke(_inner, new object[] { expression });
                return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))
                    .MakeGenericMethod(resultType)
                    .Invoke(null, new[] { executionResult });
            }
        }

        internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
        {
            public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable) { }
            public TestAsyncEnumerable(Expression expression) : base(expression) { }
            public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) =>
                new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());

            IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
        }
    }
}

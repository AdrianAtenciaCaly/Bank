using Banco.Infrastructure.Data;
using Banco.Infrastructure.Interfaces;
using System.Collections.Concurrent;

namespace Banco.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private readonly ConcurrentDictionary<string, object> _repositories = new();


        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public IRepository<T> Repository<T>() where T : class
        {
            var type = typeof(T).Name;
            if (_repositories.ContainsKey(type)) return (IRepository<T>)_repositories[type];

            var repositoryInstance = new GenericRepository<T>(_context);
            _repositories.TryAdd(type, repositoryInstance);
            return repositoryInstance;
        }

        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

        public void Dispose() => _context?.Dispose();
    }
}

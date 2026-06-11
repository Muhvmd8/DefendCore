using DefendCore.Domain.Entities.Common;
using DefendCore.Domain.Interfaces;
using DefendCore.Infrastructure.Presistence.DbContexts;

namespace DefendCore.Infrastructure.Presistence.Repositoreis
{
    public class UnitOfWork(ApplicationDbContext _context)
    : IUnitOfWork
    {
        private readonly Dictionary<string/*typeName*/, object/*Repo*/> _repositories = [];

        public IGenericRepository<TEntity, TKey> GetRepository<TEntity, TKey>()
            where TEntity : BaseEntity<TKey>
        {
            var typeName = typeof(TEntity).Name;
            // Get repo from the container
            if (_repositories.ContainsKey(typeName))
                return (IGenericRepository<TEntity, TKey>)_repositories[typeName];

            var repo = new GenericRepository<TEntity, TKey>(_context);
            _repositories[typeName] = repo;
            return repo;
        }

        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();
        public async Task BeginTransactionAsync() => await _context.Database.BeginTransactionAsync();
        public async Task CommitTransactionAsync() => await _context.Database.CommitTransactionAsync();
        public async Task RollbackTransactionAsync() => await _context.Database.RollbackTransactionAsync();
    }
}

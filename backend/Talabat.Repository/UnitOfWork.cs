using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core;
using Talabat.Core.Entites;
using Talabat.Core.Repositories;
using Talabat.Repository.Data;

namespace Talabat.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly StoreContext _dbContext;
        private Hashtable _repositories;

        public UnitOfWork(StoreContext dbContext)
        {
            _repositories = new Hashtable();
            _dbContext = dbContext;
        }

        public async Task<int> CompleteAsync()
        => await _dbContext.SaveChangesAsync();
        
        
        public ValueTask DisposeAsync()
        => _dbContext.DisposeAsync();

        public IGenericRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity
        {
            var type = typeof(TEntity).Name;
            if (!_repositories.ContainsKey(type))
            {
                var Repository = new GenericRepository<TEntity>(_dbContext);
                _repositories.Add(type, Repository);
            }
            return _repositories[type] as IGenericRepository<TEntity>;
        }
    }
}

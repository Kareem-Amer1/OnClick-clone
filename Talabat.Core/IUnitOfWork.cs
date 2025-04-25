using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core.Entites;
using Talabat.Core.Repositories;

namespace Talabat.Core
{
    public interface IUnitOfWork: IAsyncDisposable
    {
        Task<int> CompleteAsync();
        IGenericRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity;
    }
}

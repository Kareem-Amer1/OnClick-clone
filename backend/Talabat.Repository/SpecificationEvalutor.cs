using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Talabat.Core.Entites;
using Talabat.Core.Specifications;

namespace Talabat.Repository
{
    public static class SpecificationEvalutor<T> where T : BaseEntity
    {
        public static IQueryable<T> GetQuery(IQueryable<T> inputQuery, ISpecifications<T> Spec)
        {
            var Query = inputQuery;
            if (Spec.Criteria is not null)
            {
                Query = Query.Where(Spec.Criteria);
            }
            if (Spec.OrderBy is not null)
            {
                Query = Query.OrderBy(Spec.OrderBy);
            }
            if (Spec.OrderByDescending is not null)
            {
                Query = Query.OrderByDescending(Spec.OrderByDescending);
            }

            if (Spec.IsPaginationEnabled)
            {
                Query = Query.Skip(Spec.Skip).Take(Spec.Take);
            }

            Query =Spec.Includes.Aggregate(Query, (CurrentQuery, IncludeExpression) => CurrentQuery.Include(IncludeExpression));
          
            return Query;
        }
    }
}

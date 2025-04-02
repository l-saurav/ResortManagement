using ResortManagement.Domain.Entities;
using System.Linq.Expressions;

namespace ResortManagement.Application.Common.Interfaces
{
    public interface IVillaRepository : IRepository<Villa>
    {
        void Update(Villa entity); 
        void Save();
    }
}

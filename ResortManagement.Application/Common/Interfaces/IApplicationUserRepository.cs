using ResortManagement.Domain.Entities;

namespace ResortManagement.Application.Common.Interfaces
{
    public interface IApplicationUserRepository : IRepository<ApplicationUser>
    {
        void Save();
    }
}

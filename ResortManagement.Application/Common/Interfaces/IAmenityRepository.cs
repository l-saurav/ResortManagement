
using ResortManagement.Domain.Entities;

namespace ResortManagement.Application.Common.Interfaces
{
    public interface IAmenityRepository : IRepository<Amenity>
    {
        void Update(Amenity amenity);
        void Save();
    }
}

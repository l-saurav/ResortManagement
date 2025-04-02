using ResortManagement.Domain.Entities;

namespace ResortManagement.Application.Common.Interfaces
{
    public interface IVillaNumberRepository : IRepository<VillaNumber>
    {
        void Update(VillaNumber villaNumber);
        void Save();
    }
}

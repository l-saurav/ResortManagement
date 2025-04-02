using ResortManagement.Application.Common.Interfaces;
using ResortManagement.Domain.Entities;
using ResortManagement.Infrastructure.Data;

namespace ResortManagement.Infrastructure.Repositories
{
    public class AmenityRepository : Repository<Amenity>, IAmenityRepository
    {
        private readonly ApplicationDBContext _dBContext;
        public AmenityRepository(ApplicationDBContext dBContext) : base (dBContext)
        {
            _dBContext = dBContext;
        }
        public void Save()
        {
            _dBContext.SaveChanges();
        }

        public void Update(Amenity amenity)
        {
            _dBContext.Amenities.Update(amenity);
        }
    }
}

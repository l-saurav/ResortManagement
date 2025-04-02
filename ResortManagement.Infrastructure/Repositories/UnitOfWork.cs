using ResortManagement.Application.Common.Interfaces;
using ResortManagement.Infrastructure.Data;

namespace ResortManagement.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        public IVillaRepository Villa { get; private set; }
        public IVillaNumberRepository VillaNumber { get; private set; }
        public IAmenityRepository Amenity { get; private set; }

        private readonly ApplicationDBContext _dbContext;
        public UnitOfWork(ApplicationDBContext dBContext)
        {
            _dbContext = dBContext;
            Villa = new VillaRepository(_dbContext);
            VillaNumber = new VillaNumberRepository(_dbContext);
            Amenity = new AmenityRepository(_dbContext);
        }
    }
}


using ResortManagement.Application.Common.Interfaces;
using ResortManagement.Domain.Entities;
using ResortManagement.Infrastructure.Data;

namespace ResortManagement.Infrastructure.Repositories
{
    public class VillaNumberRepository : Repository<VillaNumber>, IVillaNumberRepository
    {
        private readonly ApplicationDBContext _dbContext;
        public VillaNumberRepository(ApplicationDBContext dbContext) : base (dbContext)
        {
            _dbContext = dbContext;
        }

        public void Save()
        {
            _dbContext.SaveChanges();
        }

        public void Update(VillaNumber villaNumber)
        {
            _dbContext.VillaNumbers.Update(villaNumber);
        }
    }
}

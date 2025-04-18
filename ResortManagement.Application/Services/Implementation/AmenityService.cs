using ResortManagement.Application.Common.Interfaces;
using ResortManagement.Application.Services.Interface;
using ResortManagement.Domain.Entities;

namespace ResortManagement.Application.Services.Implementation
{
    public class AmenityService : IAmenityService
    {
        private readonly IUnitOfWork _unitOfWork;
        public AmenityService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public bool CheckAmenityExists(int id)
        {
            return _unitOfWork.Amenity.Any(a => a.ID == id);
        }

        public void CreateAmenity(Amenity amenity)
        {
            _unitOfWork.Amenity.Add(amenity);
            _unitOfWork.Amenity.Save();
        }

        public bool DeleteAmenity(int id)
        {
            try
            {

                Amenity? amenityToDelete = _unitOfWork.Amenity.Get(a => a.ID == id);
                if (amenityToDelete is not null)
                {
                    _unitOfWork.Amenity.Delete(amenityToDelete);
                    _unitOfWork.Amenity.Save();
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public IEnumerable<Amenity> GetAllAmenities()
        {
            return _unitOfWork.Amenity.GetAll(includeProperties: "villa");
        }

        public Amenity GetAmenityById(int id)
        {
            return _unitOfWork.Amenity.Get(a => a.ID == id);
        }

        public void UpdateAmenity(Amenity amenity)
        {
            _unitOfWork.Amenity.Update(amenity);
            _unitOfWork.Amenity.Save();
        }
    }
}

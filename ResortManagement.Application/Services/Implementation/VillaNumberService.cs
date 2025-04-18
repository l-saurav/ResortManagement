using Microsoft.AspNetCore.Hosting;
using ResortManagement.Application.Common.Interfaces;
using ResortManagement.Application.Services.Interface;
using ResortManagement.Domain.Entities;

namespace ResortManagement.Application.Services.Implementation
{
    public class VillaNumberService : IVillaNumberService
    {
        private readonly IUnitOfWork _unitOfWork;
        public VillaNumberService(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
        }

        public bool CheckVillaNumberExists(int villa_Number)
        {
            return _unitOfWork.VillaNumber.Any(vn => vn.Villa_Number == villa_Number);
        }

        public void CreateVillaNumber(VillaNumber villaNumber)
        {
            _unitOfWork.VillaNumber.Add(villaNumber);
            _unitOfWork.VillaNumber.Save();
        }

        public bool DeleteVillaNumber(int id)
        {
            try
            {
                VillaNumber? villaNumberToDelete = _unitOfWork.VillaNumber.Get(v => v.Villa_Number == id);
                if (villaNumberToDelete is not null)
                {
                    _unitOfWork.VillaNumber.Delete(villaNumberToDelete);
                    _unitOfWork.VillaNumber.Save();
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public IEnumerable<VillaNumber> GetAllVillaNumbers()
        {
            return _unitOfWork.VillaNumber.GetAll(includeProperties: "Villa");
        }

        public VillaNumber GetVillaNumberById(int id)
        {
            return _unitOfWork.VillaNumber.Get(v => v.Villa_Number == id, includeProperties: "Villa");
        }

        public void UpdateVillaNumber(VillaNumber villaNumber)
        {
            _unitOfWork.VillaNumber.Update(villaNumber);
            _unitOfWork.VillaNumber.Save();
        }
    }
}

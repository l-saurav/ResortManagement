using Microsoft.AspNetCore.Hosting;
using ResortManagement.Application.Common.Interfaces;
using ResortManagement.Application.Common.Utility;
using ResortManagement.Application.Services.Interface;
using ResortManagement.Domain.Entities;

namespace ResortManagement.Application.Services.Implementation
{
    public class VillaService : IVillaService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public VillaService(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }
        public void CreateVilla(Villa villa)
        {
            if (villa.Image != null)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(villa.Image.FileName);
                string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, @"images\VillaImage");
                using var fileStream = new FileStream(Path.Combine(imagePath, fileName), FileMode.Create);
                villa.Image.CopyTo(fileStream);
                villa.ImageURL = @"\images\VillaImage\" + fileName;
            }
            else
            {
                //If image is not setup; keep this as default
                villa.ImageURL = "https://placehold.co/600x400";
            }
            _unitOfWork.Villa.Add(villa);
            _unitOfWork.Villa.Save();
        }

        public bool DeleteVilla(int id)
        {
            try
            {
                Villa? villaToDelete = _unitOfWork.Villa.Get(v => v.ID == id);
                if (villaToDelete is not null)
                {
                    //Delete Old image if it exist!
                    if (!string.IsNullOrEmpty(villaToDelete.ImageURL))
                    {
                        var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, villaToDelete.ImageURL.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }
                    _unitOfWork.Villa.Delete(villaToDelete);
                    _unitOfWork.Villa.Save();
                }
                return true;
            }catch (Exception)
            {
                return false;
            }
        }

        public IEnumerable<Villa> GetAllVillas()
        {
            return _unitOfWork.Villa.GetAll(includeProperties:"VillaAmenity");
        }

        public Villa GetVillaById(int id)
        {
            return _unitOfWork.Villa.Get(v => v.ID == id, includeProperties:"VillaAmenity");
        }

        public IEnumerable<Villa> GetVillasAvailabilityByDate(int nights, DateOnly checkInDate)
        {
            var villaList = _unitOfWork.Villa.GetAll(includeProperties: "VillaAmenity").ToList();
            var villaNumberList = _unitOfWork.VillaNumber.GetAll().ToList();
            var bookedVillas = _unitOfWork.Booking.GetAll(b => b.Status == SD.StatusApproved || b.Status == SD.StatusCheckedIn).ToList();
            foreach (var villa in villaList)
            {
                int roomAvailable = SD.VillaRoomsAvailable_Count(villa.ID, villaNumberList, checkInDate, nights, bookedVillas);
                villa.isAvailable = roomAvailable > 0 ? true : false;
            }
            return villaList;
        }

        public bool isVillaAvailableByDate(int villaId, int nights, DateOnly checkInDate)
        {
            var villaNumberList = _unitOfWork.VillaNumber.GetAll().ToList();
            var bookedVillas = _unitOfWork.Booking.GetAll(b => b.Status == SD.StatusApproved || b.Status == SD.StatusCheckedIn).ToList();
            int roomAvailable = SD.VillaRoomsAvailable_Count(villaId, villaNumberList, checkInDate, nights, bookedVillas);
            return roomAvailable > 0 ? true : false;
        }

        public void UpdateVilla(Villa villa)
        {
            if (villa.Image != null)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(villa.Image.FileName);
                string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, @"images\VillaImage");
                //Delete Old image if it exist!
                if (!string.IsNullOrEmpty(villa.ImageURL))
                {
                    var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, villa.ImageURL.TrimStart('\\'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }
                using var fileStream = new FileStream(Path.Combine(imagePath, fileName), FileMode.Create);
                villa.Image.CopyTo(fileStream);
                villa.ImageURL = @"\images\VillaImage\" + fileName;
            }
            _unitOfWork.Villa.Update(villa);
            _unitOfWork.Villa.Save();
        }
    }
}

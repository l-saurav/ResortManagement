using Microsoft.AspNetCore.Mvc;
using ResortManagement.Application.Common.Interfaces;
using ResortManagement.Application.Common.Utility;
using ResortManagement.Web.Models;

namespace ResortManagementWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public HomeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            HomeViewModel homeViewModel = new()
            {
                VillaList = _unitOfWork.Villa.GetAll(includeProperties: "VillaAmenity"),
                CheckInDate = DateOnly.FromDateTime(DateTime.Now),
                NoOfNight = 1
            };
            return View(homeViewModel);
        }

        [HttpPost]
        public IActionResult GetVillasByDate(int nights, DateOnly checkInDate)
        {
            var villaList = _unitOfWork.Villa.GetAll(includeProperties: "VillaAmenity").ToList();
            var villaNumberList = _unitOfWork.VillaNumber.GetAll().ToList();
            var bookedVillas = _unitOfWork.Booking.GetAll(b => b.Status == SD.StatusApproved || b.Status == SD.StatusCheckedIn).ToList();
            foreach (var villa in villaList)
            {
                int roomAvailable = SD.VillaRoomsAvailable_Count(villa.ID, villaNumberList, checkInDate, nights, bookedVillas);
                villa.isAvailable = roomAvailable > 0 ? true : false;
            }
            HomeViewModel homeViewModel = new()
            {
                CheckInDate = checkInDate,
                VillaList = villaList,
                NoOfNight = nights
            };
            return PartialView("_VillaList",homeViewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}

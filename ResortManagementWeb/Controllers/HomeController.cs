using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ResortManagement.Application.Common.Interfaces;
using ResortManagement.Web.Models;
using ResortManagementWeb.Models;

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
        public IActionResult Index(HomeViewModel homeViewModel)
        {
            //Retreiving all villa list with amenity in VillaList variable
            homeViewModel.VillaList = _unitOfWork.Villa.GetAll(includeProperties: "VillaAmenity");
            return View(homeViewModel);
        }

        public IActionResult GetVillasByDate(int nights, DateOnly checkInDate)
        {
            var villaList = _unitOfWork.Villa.GetAll(includeProperties: "VillaAmenity").ToList();
            foreach (var villa in villaList)
            {
                if (villa.ID % 2 == 0)
                {
                    villa.isAvailable = false;
                }
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

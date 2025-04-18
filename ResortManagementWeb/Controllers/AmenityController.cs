using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ResortManagement.Application.Common.Interfaces;
using ResortManagement.Application.Common.Utility;
using ResortManagement.Application.Services.Interface;
using ResortManagement.Domain.Entities;
using ResortManagement.Web.Models;

namespace ResortManagement.Web.Controllers
{
    //Only let admin to access this controller if he/she is admin
    [Authorize(Roles =SD.Role_Admin)]
    public class AmenityController : Controller
    {
        private IAmenityService _amenityService;
        private IVillaService _villaService;
        public AmenityController(IAmenityService amenityService, IVillaService villaService)
        {
            _amenityService = amenityService;
            _villaService = villaService;
        }

        public IActionResult Index()
        {
            var amenities = _amenityService.GetAllAmenities().OrderBy(a => a.villaID);
            return View(amenities);
        }
        public IActionResult Create()
        {
            AmenityViewModel amenityViewModel = new()
            {
                villaList = _villaService.GetAllVillas().Select(v => new SelectListItem
                {
                    Text = v.Name,
                    Value = v.ID.ToString()
                })
            };
            return View(amenityViewModel);
        }
        [HttpPost]
        public IActionResult Create(AmenityViewModel amenityViewModel)
        {
            bool amenityExistCheck = _amenityService.CheckAmenityExists(amenityViewModel.amenity.ID);
            if (ModelState.IsValid && !amenityExistCheck)
            {
                _amenityService.CreateAmenity(amenityViewModel.amenity);
                TempData["success"] = "New Amenity has been added Successfully!";
                return RedirectToAction(nameof(Index));
            }
            if (amenityExistCheck)
            {
                TempData["error"] = "Sorry! There already exist same villa Number";
            }
            amenityViewModel.villaList = _amenityService.GetAllAmenities().Select(v => new SelectListItem
            {
                Text = v.Name,
                Value = v.ID.ToString()
            });
            return View(amenityViewModel);
        }

        public IActionResult Update(int amenityID)
        {
            AmenityViewModel amenityViewModel = new()
            {
                villaList = _villaService.GetAllVillas().Select(v => new SelectListItem
                {
                    Text = v.Name,
                    Value = v.ID.ToString()
                }),
                amenity = _amenityService.GetAmenityById(amenityID)
            };
            if (amenityViewModel.amenity is null)
            {
                return RedirectToAction("Error", "Home");
            }
            return View(amenityViewModel);
        }
        [HttpPost]
        public IActionResult Update(Amenity amenity)
        {
            if (ModelState.IsValid)
            {
                _amenityService.UpdateAmenity(amenity);
                TempData["success"] = "The Amenity has been successfully updated!";
                return RedirectToAction(nameof(Index));
            }
            TempData["error"] = "Sorry! Unable to update the Amenity!";
            return View();
        }
        public IActionResult Delete(int amenityID)
        {
            AmenityViewModel amenityViewModel = new()
            {
                villaList = _villaService.GetAllVillas().Select(v => new SelectListItem
                {
                    Text = v.Name,
                    Value = v.ID.ToString()
                }),
                amenity = _amenityService.GetAmenityById(amenityID)
            };
            if (amenityViewModel.amenity is null)
            {
                return RedirectToAction("Error", "Home");
            }
            return View(amenityViewModel);
        }

        [HttpPost]
        public IActionResult Delete(AmenityViewModel amenityViewModel)
        {
            if (_amenityService.DeleteAmenity(amenityViewModel.amenity.ID))
            {
                TempData["success"] = "Amenity has been successfully deleted!";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["error"] = "Sorry! Unable to delete the Amenity";
                return View();
            }
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ResortManagement.Application.Common.Interfaces;
using ResortManagement.Domain.Entities;
using ResortManagement.Web.Models;

namespace ResortManagement.Web.Controllers
{
    public class AmenityController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public AmenityController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var amenities = _unitOfWork.Amenity.GetAll(includeProperties: "villa").OrderBy(a => a.villaID);
            return View(amenities);
        }
        public IActionResult Create()
        {
            AmenityViewModel amenityViewModel = new()
            {
                villaList = _unitOfWork.Villa.GetAll().Select(v => new SelectListItem
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
            bool amenityExistCheck = _unitOfWork.Amenity.Any(a => a.ID == amenityViewModel.amenity.ID);
            if (ModelState.IsValid && !amenityExistCheck)
            {
                _unitOfWork.Amenity.Add(amenityViewModel.amenity);
                _unitOfWork.Amenity.Save();
                TempData["success"] = "New Amenity has been added Successfully!";
                return RedirectToAction(nameof(Index));
            }
            if (amenityExistCheck)
            {
                TempData["error"] = "Sorry! There already exist same villa Number";
            }
            amenityViewModel.villaList = _unitOfWork.Villa.GetAll().Select(v => new SelectListItem
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
                villaList = _unitOfWork.Villa.GetAll().Select(v => new SelectListItem
                {
                    Text = v.Name,
                    Value = v.ID.ToString()
                }),
                amenity = _unitOfWork.Amenity.Get(a => a.ID == amenityID)
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
                _unitOfWork.Amenity.Update(amenity);
                _unitOfWork.Amenity.Save();
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
                villaList = _unitOfWork.Villa.GetAll().Select(v => new SelectListItem
                {
                    Text = v.Name,
                    Value = v.ID.ToString()
                }),
                amenity = _unitOfWork.Amenity.Get(a => a.ID == amenityID)
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
            Amenity? amenityToDelete = _unitOfWork.Amenity.Get(a => a.ID == amenityViewModel.amenity.ID);
            if (amenityViewModel is not null)
            {
                _unitOfWork.Amenity.Delete(amenityToDelete);
                _unitOfWork.Amenity.Save();
                TempData["success"] = "Amenity has been successfully deleted!";
                return RedirectToAction(nameof(Index));
            }
            TempData["error"] = "Sorry! Unable to delete the Amenity";
            return View();
        }
    }
}

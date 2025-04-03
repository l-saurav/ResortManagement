using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ResortManagement.Application.Common.Interfaces;
using ResortManagement.Domain.Entities;
using ResortManagement.Infrastructure.Data;
using ResortManagement.Web.Models;

namespace ResortManagement.Web.Controllers
{
    [Authorize]
    public class VillaNumberController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public VillaNumberController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            //var villaNumbers = _dbContext.VillaNumbers.Include(v => v.Villa).OrderBy(v => v.VillaID).ToList();
            var villaNumbers = _unitOfWork.VillaNumber.GetAll(includeProperties: "Villa").OrderBy(v => v.VillaID);
            return View(villaNumbers);
        }

        public IActionResult Create()
        {
            VillaNumberViewModel villaNumberVM = new()
            {
                villaList = _unitOfWork.Villa.GetAll().Select(v => new SelectListItem
                {
                    Text = v.Name,
                    Value = v.ID.ToString()
                })
            };
            return View(villaNumberVM);
        }
        [HttpPost]
        public IActionResult Create(VillaNumberViewModel villaNumberVM)
        {
            bool villaExistCheck = _unitOfWork.VillaNumber.Any(vn => vn.Villa_Number == villaNumberVM.villaNumber.Villa_Number);
            if (ModelState.IsValid && !villaExistCheck)
            {
                _unitOfWork.VillaNumber.Add(villaNumberVM.villaNumber);
                _unitOfWork.VillaNumber.Save();
                TempData["success"] = "New Villa Number has been added Successfully!";
                return RedirectToAction(nameof(Index));
            }
            if (villaExistCheck)
            {
                TempData["error"] = "Sorry! There already exist same villa Number";
            }
            villaNumberVM.villaList = _unitOfWork.Villa.GetAll().Select(v => new SelectListItem
            {
                Text = v.Name,
                Value = v.ID.ToString()
            });
            return View(villaNumberVM);
        }

        public IActionResult Update(int villaNumber)
        {
            VillaNumberViewModel villaNumberVM = new()
            {
                villaList = _unitOfWork.Villa.GetAll().Select(v => new SelectListItem
                {
                    Text = v.Name,
                    Value = v.ID.ToString()
                }),
                villaNumber = _unitOfWork.VillaNumber.Get(vn => vn.Villa_Number == villaNumber)
            };
            if (villaNumberVM.villaNumber is null)
            {
                return RedirectToAction("Error", "Home");
            }
            return View(villaNumberVM);
        }
        [HttpPost]
        public IActionResult Update(VillaNumber villaNumber)
        {
            if(ModelState.IsValid)
            {
                _unitOfWork.VillaNumber.Update(villaNumber);
                _unitOfWork.VillaNumber.Save();
                TempData["success"] = "The Villa Number has been successfully updated!";
                return RedirectToAction(nameof(Index));
            }
            TempData["error"] = "Sorry! Unable to update the Villa Number!";
            return View();
        }

        public IActionResult Delete(int villaNumber)
        {
            VillaNumberViewModel villaNumberVM = new()
            {
                villaList = _unitOfWork.Villa.GetAll().Select(v => new SelectListItem
                {
                    Text = v.Name,
                    Value = v.ID.ToString()
                }),
                villaNumber = _unitOfWork.VillaNumber.Get(vn => vn.Villa_Number == villaNumber)
            };
            if (villaNumberVM.villaNumber is null)
            {
                return RedirectToAction("Error", "Home");
            }
            return View(villaNumberVM);
        }

        [HttpPost]
        public IActionResult Delete(VillaNumberViewModel villa_Number)
        {
            VillaNumber? villaNumberToDelete = _unitOfWork.VillaNumber.Get(v => v.Villa_Number == villa_Number.villaNumber.Villa_Number);
            if(villa_Number is not null)
            {
                _unitOfWork.VillaNumber.Delete(villaNumberToDelete);
                _unitOfWork.VillaNumber.Save();
                TempData["success"] = "Villa Number has been successfully deleted!";
                return RedirectToAction(nameof(Index));
            }
            TempData["error"] = "Sorry! Unable to delete the Villa Number";
            return View();
        }
    }
}

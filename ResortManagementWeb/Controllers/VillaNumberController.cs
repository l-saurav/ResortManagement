using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ResortManagement.Application.Services.Interface;
using ResortManagement.Domain.Entities;
using ResortManagement.Web.Models;

namespace ResortManagement.Web.Controllers
{
    [Authorize]
    public class VillaNumberController : Controller
    {
        private readonly IVillaNumberService _villaNumberService;
        private readonly IVillaService _villaService;
        public VillaNumberController(IVillaService villaService,IVillaNumberService villaNumberService)
        {
            _villaService = villaService;
            _villaNumberService = villaNumberService;
        }

        public IActionResult Index()
        {
            //var villaNumbers = _dbContext.VillaNumbers.Include(v => v.Villa).OrderBy(v => v.VillaID).ToList();
            var villaNumbers = _villaNumberService.GetAllVillaNumbers().OrderBy(v => v.VillaID);
            return View(villaNumbers);
        }

        public IActionResult Create()
        {
            VillaNumberViewModel villaNumberVM = new()
            {
                villaList = _villaService.GetAllVillas().Select(v => new SelectListItem
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
            bool villaExistCheck = _villaNumberService.CheckVillaNumberExists(villaNumberVM.villaNumber.Villa_Number);
            if (ModelState.IsValid && !villaExistCheck)
            {
                _villaNumberService.CreateVillaNumber(villaNumberVM.villaNumber);
                TempData["success"] = "New Villa Number has been added Successfully!";
                return RedirectToAction(nameof(Index));
            }
            if (villaExistCheck)
            {
                TempData["error"] = "Sorry! There already exist same villa Number";
            }
            villaNumberVM.villaList = _villaService.GetAllVillas().Select(v => new SelectListItem
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
                villaList = _villaService.GetAllVillas().Select(v => new SelectListItem
                {
                    Text = v.Name,
                    Value = v.ID.ToString()
                }),
                villaNumber = _villaNumberService.GetVillaNumberById(villaNumber)
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
               _villaNumberService.UpdateVillaNumber(villaNumber);
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
                villaList = _villaService.GetAllVillas().Select(v => new SelectListItem
                {
                    Text = v.Name,
                    Value = v.ID.ToString()
                }),
                villaNumber = _villaNumberService.GetVillaNumberById(villaNumber)
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
            VillaNumber? villaNumberToDelete = _villaNumberService.GetVillaNumberById(villa_Number.villaNumber.Villa_Number);
            if(villa_Number is not null)
            {
                _villaNumberService.DeleteVillaNumber(villaNumberToDelete.Villa_Number);
                TempData["success"] = "Villa Number has been successfully deleted!";
                return RedirectToAction(nameof(Index));
            }
            TempData["error"] = "Sorry! Unable to delete the Villa Number";
            return View();
        }
    }
}

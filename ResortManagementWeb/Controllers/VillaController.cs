using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResortManagement.Application.Common.Interfaces;
using ResortManagement.Application.Services.Interface;
using ResortManagement.Domain.Entities;

namespace ResortManagement.Web.Controllers
{
    [Authorize]
    public class VillaController : Controller
    {
        private readonly IVillaService _villaService;
        public VillaController(IVillaService villaService)
        {
            _villaService = villaService;
        }

        public IActionResult Index()
        {
            var villas = _villaService.GetAllVillas();
            return View(villas);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Villa villa)
        {
            if (villa.Name == villa.Description)
            {
                ModelState.AddModelError("", "The Description cannot exactly match the Name.");
            }
            if (ModelState.IsValid)
            {
                _villaService.CreateVilla(villa);
                TempData["success"] = "New villa has been added Successfully!";
                return RedirectToAction(nameof(Index));
            }
            TempData["error"] = "Sorry, Unable to Add New Villa!";
            return View();
        }
        

        public IActionResult Update(int villaID)
        {
            Villa? villa = _villaService.GetVillaById(villaID);
            if (villa is null)
            {
                return RedirectToAction("Error", "Home");
            }
            return View(villa);
        }

        [HttpPost]
        public IActionResult Update(Villa villa)
        {
            if (ModelState.IsValid)
            {
                _villaService.UpdateVilla(villa);
                TempData["success"] = "Villa has been Updated Successfully!";
                return RedirectToAction(nameof(Index));
            }
            TempData["error"] = "Sorry, Unable to Update Villa Information!";
            return View();
        }

        public IActionResult Delete(int villaID)
        {
            Villa? villa = _villaService.GetVillaById(villaID);
            if(villa is null)
            {
                return RedirectToAction("Error", "Home");
            }
            return View(villa);
        }

        [HttpPost]
        public IActionResult Delete(Villa villa)
        {
            if (_villaService.DeleteVilla(villa.ID))
            {
                TempData["success"] = "Villa has been deleted Successfully";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["error"] = "Sorry! Unable to delete Villa Information";
                return View();
            }
        }
    }
}

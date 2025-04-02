using Microsoft.AspNetCore.Mvc;
using ResortManagement.Application.Common.Interfaces;
using ResortManagement.Domain.Entities;

namespace ResortManagement.Web.Controllers
{
    public class VillaController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public VillaController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            var villas = _unitOfWork.Villa.GetAll();
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
                TempData["success"] = "New villa has been added Successfully!";
                return RedirectToAction(nameof(Index));
            }
            TempData["error"] = "Sorry, Unable to Add New Villa!";
            return View();
        }
        

        public IActionResult Update(int villaID)
        {
            Villa? villa = _unitOfWork.Villa.Get(v => v.ID == villaID);
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
                if (villa.Image != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(villa.Image.FileName);
                    string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, @"images\VillaImage");
                    //Delete Old image if it exist!
                    if(!string.IsNullOrEmpty(villa.ImageURL))
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
                TempData["success"] = "Villa has been Updated Successfully!";
                return RedirectToAction(nameof(Index));
            }
            TempData["error"] = "Sorry, Unable to Update Villa Information!";
            return View();
        }

        public IActionResult Delete(int villaID)
        {
            Villa? villa = _unitOfWork.Villa.Get(v => v.ID == villaID);
            if(villa is null)
            {
                return RedirectToAction("Error", "Home");
            }
            return View(villa);
        }

        [HttpPost]
        public IActionResult Delete(Villa villa)
        {
            Villa? villaToDelete = _unitOfWork.Villa.Get(v => v.ID == villa.ID);
            if(villaToDelete is not null)
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
                TempData["success"] = "Villa has been deleted Successfully";
                return RedirectToAction(nameof(Index));
            }
            TempData["error"] = "Sorry! Unable to delete Villa Information";
            return View();
        }
    }
}

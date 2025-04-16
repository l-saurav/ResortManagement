using Microsoft.AspNetCore.Mvc;
using ResortManagement.Application.Common.Interfaces;
using ResortManagement.Application.Common.Utility;
using ResortManagement.Web.Models;
using Syncfusion.Presentation;

namespace ResortManagementWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment; //To get access to the root file
        public HomeController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
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
        [HttpPost]
        public IActionResult GeneratePPTExport(int id)
        {
            var villa = _unitOfWork.Villa.GetAll(includeProperties: "VillaAmenity").FirstOrDefault(v => v.ID == id);
            if(villa is null)
            {
                return RedirectToAction(nameof(Error));
            }
            string basePath = _webHostEnvironment.WebRootPath;
            string filePath = basePath + @"/exports/ExportVillaDetails.pptx";
            using IPresentation presentation = Presentation.Open(filePath); //Open Powerpoint presentation in our code
            ISlide slide0 = presentation.Slides[0]; //Open Slide number 0
            //update the powerpoint
            IShape? shape = slide0.Shapes.FirstOrDefault(s => s.ShapeName == "txtVillaName") as IShape; //Everything is a shape in poerpoint
            if(shape is not null)
            {
                shape.TextBody.Text = villa.Name;
            }
            shape = slide0.Shapes.FirstOrDefault(s => s.ShapeName == "txtVillaDescription") as IShape; //Everything is a shape in poerpoint
            float descriptionHeight = 0;
            if (shape is not null)
            {
                shape.TextBody.Text = villa.Description;
                descriptionHeight = (float)shape.Height; // Get the height of the description text
            }
            shape = slide0.Shapes.FirstOrDefault(s => s.ShapeName == "txtOccupancy") as IShape; //Everything is a shape in poerpoint
            if (shape is not null)
            {
                shape.TextBody.Text = String.Format("Max Occupancy : {0} adults", villa.Occupancy.ToString());
            }
            shape = slide0.Shapes.FirstOrDefault(s => s.ShapeName == "txtVillaSize") as IShape; //Everything is a shape in poerpoint
            if (shape is not null)
            {
                shape.TextBody.Text = String.Format("Villa Size: {0} sq.ft", villa.Sqft.ToString());
            }
            shape = slide0.Shapes.FirstOrDefault(s => s.ShapeName == "txtPricePerNight") as IShape; //Everything is a shape in poerpoint
            if (shape is not null)
            {
                shape.TextBody.Text = String.Format("USD {0}/night", villa.Price.ToString("c"));
            }
            //Updating Villa Amenities
            shape = slide0.Shapes.FirstOrDefault(s => s.ShapeName == "txtVillaAmenitiesHeading") as IShape;
            if(shape is not null)
            {
                List<string> listItems = villa.VillaAmenity.Select(va => va.Name).ToList();
                shape.TextBody.Text = "Villa Amenities";
                // Adjust the position of the Villa Amenities heading
                shape.Top = shape.Top + descriptionHeight; // Add some padding (e.g., 20 units)
                foreach (var item in listItems)
                {
                    IParagraph paragraph = shape.TextBody.AddParagraph();
                    ITextPart textPart = paragraph.AddTextPart(item);
                    paragraph.ListFormat.Type = ListType.Bulleted;
                    paragraph.ListFormat.BulletCharacter = '\u2022';
                    textPart.Font.FontName = "system-ui";
                    textPart.Font.FontSize = 18;
                    textPart.Font.Color = ColorObject.FromArgb(144, 148, 152);
                }
            }
            //Updating image in the power point
            shape = slide0.Shapes.FirstOrDefault(s => s.ShapeName == "imgVilla") as IShape;
            if(shape is not null)
            {
                byte[] imageData;
                string imageURL;
                try
                {
                    imageURL = string.Format("{0}{1}", basePath, villa.ImageURL);
                    imageData = System.IO.File.ReadAllBytes(imageURL);
                }
                catch (Exception)
                {
                    imageURL = string.Format("{0}{1}", basePath, "/images/404NotFound.png");
                    imageData = System.IO.File.ReadAllBytes(imageURL);
                }
                slide0.Shapes.Remove(shape); //Remove old image
                using MemoryStream imageStream = new(imageData);
                IPicture newPicture = slide0.Pictures.AddPicture(imageStream, 60, 120, 300, 200); //Load new Image
            }


            //Save that to the presentation
            MemoryStream memoryStream = new();
            presentation.Save(memoryStream);
            memoryStream.Position = 0;
            return File(memoryStream, "application/pptx", "VillaDetails.pptx");
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

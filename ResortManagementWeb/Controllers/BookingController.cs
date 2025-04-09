using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResortManagement.Application.Common.Interfaces;
using ResortManagement.Application.Common.Utility;
using ResortManagement.Domain.Entities;
using Stripe.Checkout;
using Stripe;
using System.Security.Claims;

namespace ResortManagement.Web.Controllers
{
    public class BookingController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public BookingController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult FinalizeBooking(int villaID, DateOnly checkInDate, int nights)
        {
            //Retreive logged in User ID
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            //Now retrieve the information of the User
            var applicationUser = _unitOfWork.ApplicationUser.Get(au => au.Id == userId);

            Booking booking = new()
            {
                VillaID = villaID,
                Villa = _unitOfWork.Villa.Get(u => u.ID == villaID, includeProperties: "VillaAmenity"),
                CheckInDate = checkInDate,
                Nights = nights,
                CheckOutDate = checkInDate.AddDays(nights),
                UserID = userId,
                PhoneNumber = applicationUser.PhoneNumber,
                Email = applicationUser.Email,
                Name = applicationUser.Name
            };
            booking.TotalCost = booking.Villa.Price * nights;
            return View(booking);
        }

        [Authorize]
        [HttpPost]
        public IActionResult FinalizeBooking(Booking booking)
        {
            var villa = _unitOfWork.Villa.Get(v => v.ID == booking.VillaID);
            booking.TotalCost = villa.Price * booking.Nights;
            booking.Status = SD.StatusPending;
            booking.BookingDate = DateTime.Now;
            //Check if the villa is available
            var villaNumberList = _unitOfWork.VillaNumber.GetAll().ToList();
            var bookedVillas = _unitOfWork.Booking.GetAll(b => b.Status == SD.StatusApproved || b.Status == SD.StatusCheckedIn).ToList();
            int roomAvailable = SD.VillaRoomsAvailable_Count(villa.ID, villaNumberList, booking.CheckInDate, booking.Nights, bookedVillas);
            if(roomAvailable == 0)
            {
                TempData["error"] = "Room has been sold out!";
                return RedirectToAction(nameof(FinalizeBooking), new
                {
                    villaId = booking.VillaID,
                    checkInDate = booking.CheckInDate,
                    nights = booking.Nights
                });
            }
            _unitOfWork.Booking.Add(booking);
            _unitOfWork.Booking.Save();
            //TempData["success"] = "Congrats! Villa has been booked!";

            //Stripe Processing
            var domain = $"{Request.Scheme}://{Request.Host.Value}/";
            var options = new Stripe.Checkout.SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = domain + $"Booking/BookingConfirmation?bookingId={booking.ID}",
                CancelUrl = domain + $"Booking/FinalizeBooking?villaId={booking.VillaID}&checkInDate={booking.CheckInDate}&nights={booking.Nights}"
            };
            options.LineItems.Add(new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = (long)(booking.TotalCost * 100),
                    Currency = "USD",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = villa.Name,
                        Description = villa.Description
                    },
                },
                Quantity = 1,
            });
            var service = new Stripe.Checkout.SessionService();
            Stripe.Checkout.Session session = service.Create(options);
            //Save session ID in the database
            _unitOfWork.Booking.UpdateStripePaymentID(booking.ID, session.Id, session.PaymentIntentId);
            _unitOfWork.Booking.Save();

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        [Authorize]
        public IActionResult BookingConfirmation(int bookingId)
        {
            //Retrieve the stored stripe session ID and check if the payment was successful or not
            Booking bookingFromDb = _unitOfWork.Booking.Get(b => b.ID == bookingId, includeProperties: "User,Villa");
            if(bookingFromDb.Status == SD.StatusPending)
            {
                //From Pending order check if the payment was successful
                var service = new SessionService();
                Session session = service.Get(bookingFromDb.StripeSessionID);

                if(session.PaymentStatus == "paid")
                {
                    _unitOfWork.Booking.UpdateStatus(bookingFromDb.ID, SD.StatusApproved,0);
                    _unitOfWork.Booking.UpdateStripePaymentID(bookingFromDb.ID, session.Id, session.PaymentIntentId);
                    _unitOfWork.Booking.Save();
                }
            }

            return View(bookingId);
        }

        [Authorize]
        public IActionResult BookingDetails(int bookingId)
        {
            // Retrieve the booking details with related User and Villa
            Booking bookingDetail = _unitOfWork.Booking.Get(b => b.ID == bookingId, includeProperties: "User,Villa");

            // Check if the booking is approved and no villa number is assigned
            if (bookingDetail.VillaNumber == 0 && bookingDetail.Status == SD.StatusApproved)
            {
                // Fetch available villa numbers for the specific villa
                var availableVillaNumber = AssignAvailableVillaNumberByVilla(bookingDetail.VillaID);

                // Filter villa numbers in memory
                var allVillaNumbers = _unitOfWork.VillaNumber.GetAll(vn => vn.VillaID == bookingDetail.VillaID).ToList();
                bookingDetail.villaNumbers = allVillaNumbers
                    .Where(vn => availableVillaNumber.Contains(vn.Villa_Number))
                    .ToList();
            }

            return View(bookingDetail);
        }
        private List<int> AssignAvailableVillaNumberByVilla (int villaId)
        {
            List<int> availableVillaNumbers = new();
            var villaNumbers = _unitOfWork.VillaNumber.GetAll(vn => vn.VillaID == villaId);
            var checkedInVilla = _unitOfWork.Booking.GetAll(vn => vn.VillaID == villaId && vn.Status == SD.StatusCheckedIn)
                .Select(vn => vn.VillaNumber);
            foreach(var villaNumber in villaNumbers)
            {
                if (!checkedInVilla.Contains(villaNumber.Villa_Number))
                {
                    availableVillaNumbers.Add(villaNumber.Villa_Number);
                }
            }
            return availableVillaNumbers;
        }

        [HttpPost]
        [Authorize(Roles =SD.Role_Admin)]
        public IActionResult CheckIn(Booking booking)
        {
            _unitOfWork.Booking.UpdateStatus(booking.ID, SD.StatusCheckedIn, booking.VillaNumber);
            _unitOfWork.Booking.Save();
            TempData["success"] = "Booking Status Updated to Checked In!";
            return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.ID });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult CheckOut(Booking booking)
        {
            _unitOfWork.Booking.UpdateStatus(booking.ID, SD.StatusCheckedOut, booking.VillaNumber);
            _unitOfWork.Booking.Save();
            TempData["success"] = "Booking Status Updated to Check Out!";
            return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.ID });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult CancelBooking(Booking booking)
        {
            _unitOfWork.Booking.UpdateStatus(booking.ID, SD.StatusCancelled, 0);
            _unitOfWork.Booking.Save();
            TempData["success"] = "Booking Status updated to Cancelled!";
            return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.ID });
        }

        #region API Calls
        [HttpGet]
        [Authorize]
        public IActionResult GetAll(string status)
        {
            IEnumerable<Booking> bookingObject;
            if (User.IsInRole(SD.Role_Admin))
            {
                bookingObject = _unitOfWork.Booking.GetAll(includeProperties: "User,Villa");
            }
            else
            {
                //Get logged in User ID
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userID = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                //Get all the Booking specific to the user
                bookingObject = _unitOfWork.Booking.GetAll(b => b.UserID == userID, includeProperties: "User,Villa");
            }
            if (!string.IsNullOrEmpty(status))
            {
                bookingObject = bookingObject.Where(b => b.Status.ToLower().Equals(status.ToLower()));
            }
            return Json(new { data = bookingObject });
        }
        #endregion
    }
}

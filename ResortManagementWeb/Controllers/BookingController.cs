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
                    _unitOfWork.Booking.UpdateStatus(bookingFromDb.ID, SD.StatusApproved);
                    _unitOfWork.Booking.UpdateStripePaymentID(bookingFromDb.ID, session.Id, session.PaymentIntentId);
                    _unitOfWork.Booking.Save();
                }
            }

            return View(bookingId);
        }
    }
}

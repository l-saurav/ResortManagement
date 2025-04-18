﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ResortManagement.Application.Common.Interfaces;
using ResortManagement.Application.Common.Utility;
using ResortManagement.Application.Services.Interface;
using ResortManagement.Domain.Entities;
using ResortManagement.Infrastructure.Repositories;
using Stripe.Checkout;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIORenderer;
using Syncfusion.Drawing;
using Syncfusion.Pdf;
using System.Security.Claims;

namespace ResortManagement.Web.Controllers
{
    public class BookingController : Controller
    {
        private readonly IVillaService _villaService;
        private readonly IVillaNumberService _villaNumberService;
        private readonly IBookingService _bookingService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public BookingController(IVillaService villaService,
            IVillaNumberService villaNumberService,
            IBookingService bookingService,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment webHostEnvironment)
        {
            _villaService = villaService;
            _villaNumberService = villaNumberService;
            _bookingService = bookingService;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
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
            var applicationUser = _userManager.FindByIdAsync(userId).GetAwaiter().GetResult(); // Calling GetAwaiter as it is an async endpoint

            Booking booking = new()
            {
                VillaID = villaID,
                Villa = _villaService.GetVillaById(villaID),
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
            var villa = _villaService.GetVillaById(booking.VillaID);
            booking.TotalCost = villa.Price * booking.Nights;
            booking.Status = SD.StatusPending;
            booking.BookingDate = DateTime.Now;
            //Check if the villa is available
            if(!_villaService.isVillaAvailableByDate(villa.ID,booking.Nights,booking.CheckInDate))
            {
                TempData["error"] = "Room has been sold out!";
                return RedirectToAction(nameof(FinalizeBooking), new
                {
                    villaId = booking.VillaID,
                    checkInDate = booking.CheckInDate,
                    nights = booking.Nights
                });
            }
            _bookingService.CreateBooking(booking);
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
            _bookingService.UpdateStripePaymentID(booking.ID, session.Id, session.PaymentIntentId);

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        [Authorize]
        public IActionResult BookingConfirmation(int bookingId)
        {
            //Retrieve the stored stripe session ID and check if the payment was successful or not
            Booking bookingFromDb = _bookingService.GetBookingById(bookingId);
            if(bookingFromDb.Status == SD.StatusPending)
            {
                //From Pending order check if the payment was successful
                var service = new SessionService();
                Session session = service.Get(bookingFromDb.StripeSessionID);

                if(session.PaymentStatus == "paid")
                {
                    _bookingService.UpdateStatus(bookingFromDb.ID, SD.StatusApproved,0);
                    _bookingService.UpdateStripePaymentID(bookingFromDb.ID, session.Id, session.PaymentIntentId); 
                }
            }

            return View(bookingId);
        }

        [Authorize]
        public IActionResult BookingDetails(int bookingId)
        {
            // Retrieve the booking details with related User and Villa
            Booking bookingDetail = _bookingService.GetBookingById(bookingId);

            // Check if the booking is approved and no villa number is assigned
            if (bookingDetail.VillaNumber == 0 && bookingDetail.Status == SD.StatusApproved)
            {
                // Fetch available villa numbers for the specific villa
                var availableVillaNumber = AssignAvailableVillaNumberByVilla(bookingDetail.VillaID);

                // Filter villa numbers in memory
                var allVillaNumbers = _villaNumberService.GetAllVillaNumbers().Where(vn => vn.VillaID == bookingDetail.VillaID).ToList();
                bookingDetail.villaNumbers = allVillaNumbers
                    .Where(vn => availableVillaNumber.Contains(vn.Villa_Number))
                    .ToList();
            }

            return View(bookingDetail);
        }

        [HttpPost]
        [Authorize]
        public IActionResult GenerateInvoice(int id, string downloadType)
        {
            
            string basePath = _webHostEnvironment.WebRootPath; //Get the base path
            WordDocument wordDocument = new WordDocument(); //Create a new word document
            string dataPath = basePath + @"/exports/BookingDetails.docx"; //Path to load the template
            using FileStream fileStream = new FileStream(dataPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            wordDocument.Open(fileStream, FormatType.Automatic);
            //Update the template
            Booking bookingFromDb = _bookingService.GetBookingById(id);
            //Username
            TextSelection textSelection = wordDocument.Find("xx_customer_name", false, true);
            WTextRange textRange = textSelection.GetAsOneRange();
            textRange.Text = bookingFromDb.Name;
            //Phone Number
            textSelection = wordDocument.Find("xx_customer_phone", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = bookingFromDb.PhoneNumber;
            //Email Address
            textSelection = wordDocument.Find("xx_customer_email", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = bookingFromDb.Email;
            //Payment Date
            textSelection = wordDocument.Find("xx_payment_date", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = bookingFromDb.PaymentDate.ToShortDateString();
            //Checkin Date
            textSelection = wordDocument.Find("xx_checkin_date", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = bookingFromDb.CheckInDate.ToShortDateString();
            //Checkout Date
            textSelection = wordDocument.Find("xx_checkout_date", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = bookingFromDb.CheckOutDate.ToShortDateString();
            //Booking Total Amount
            textSelection = wordDocument.Find("xx_booking_total", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = bookingFromDb.TotalCost.ToString("c");
            //Booking Number
            textSelection = wordDocument.Find("XX_BOOKING_NUMBER", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = "Booking Number: " + bookingFromDb.ID;
            //Booking Date
            textSelection = wordDocument.Find("XX_BOOKING_DATE", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = "Booking Date: " + bookingFromDb.BookingDate.ToShortDateString();

            //Creating table and adding it to the word file
            WTable table = new(wordDocument);
            table.TableFormat.Borders.LineWidth = 1f;
            table.TableFormat.Borders.Color = Color.Black;
            table.TableFormat.Paddings.Top = 7f;
            table.TableFormat.Paddings.Bottom = 7f;
            table.TableFormat.Borders.Horizontal.LineWidth = 1f;
            //Define rows and column you will be having
            int rows = bookingFromDb.VillaNumber > 0 ? 3 : 2; //Checking if the user have been assigned villa number(Checked-In Status)
            table.ResetCells(rows, 4);
            //Work on individual row
            WTableRow row0 = table.Rows[0];
            //Table header
            row0.Cells[0].AddParagraph().AppendText("Nights");
            row0.Cells[0].Width = 80;
            row0.Cells[1].AddParagraph().AppendText("Villa");
            row0.Cells[1].Width = 220;
            row0.Cells[2].AddParagraph().AppendText("Price per Night");
            row0.Cells[3].AddParagraph().AppendText("Total");
            row0.Cells[3].Width = 80;
            //Table Data
            WTableRow row1 = table.Rows[1];
            row1.Cells[0].AddParagraph().AppendText(bookingFromDb.Nights.ToString());
            row1.Cells[0].Width = 80;
            row1.Cells[1].AddParagraph().AppendText(bookingFromDb.Villa.Name);
            row1.Cells[1].Width = 220;
            row1.Cells[2].AddParagraph().AppendText(bookingFromDb.Villa.Price.ToString("c"));
            row1.Cells[3].AddParagraph().AppendText(bookingFromDb.TotalCost.ToString("c"));
            row1.Cells[3].Width = 80;
            if (rows > 2)
            {

                WTableRow row2 = table.Rows[2];
                row2.Cells[0].Width = 80;
                row2.Cells[1].AddParagraph().AppendText("Villa Number: "+bookingFromDb.VillaNumber.ToString());
                row2.Cells[1].Width = 220;
                row2.Cells[3].Width = 80;
            }
            //Add Custom Design into the table
            WTableStyle tableStyle = wordDocument.AddTableStyle("CustomStyle") as WTableStyle;
            tableStyle.TableProperties.RowStripe = 1;
            tableStyle.TableProperties.ColumnStripe = 2;
            tableStyle.TableProperties.Paddings.Top = 2;
            tableStyle.TableProperties.Paddings.Bottom = 1;
            tableStyle.TableProperties.Paddings.Left = 5.4f;
            tableStyle.TableProperties.Paddings.Right = 5.4f;
            //Add style only on the heading of the table
            ConditionalFormattingStyle firstRowStyle = tableStyle.ConditionalFormattingStyles.Add(ConditionalFormattingType.FirstRow);
            firstRowStyle.CharacterFormat.Bold = true;
            firstRowStyle.CharacterFormat.TextColor = Color.FromArgb(255, 255, 255, 255);
            firstRowStyle.CellProperties.BackColor = Color.Black;
            table.ApplyStyle("CustomStyle");
            //Add created table on the document
            TextBodyPart bodyPart = new(wordDocument);
            bodyPart.BodyItems.Add(table);
            wordDocument.Replace("<ADDTABLEHERE>", bodyPart, false, false);


            //Save the updated into the file
            using DocIORenderer docIORenderer = new();
            MemoryStream stream = new();
            if (downloadType == "word")
            {
                wordDocument.Save(stream, FormatType.Docx);
                stream.Position = 0;
                return File(stream, "application/docx", "BookingDetails.docx");
            }
            else
            {
                PdfDocument pdfDocument = docIORenderer.ConvertToPDF(wordDocument);
                pdfDocument.Save(stream);
                stream.Position = 0;
                return File(stream, "application/pdf", "BookingDetails.pdf");
            }
        }

        private List<int> AssignAvailableVillaNumberByVilla (int villaId)
        {
            List<int> availableVillaNumbers = new();
            var villaNumbers = _villaNumberService.GetAllVillaNumbers().Where(vn => vn.VillaID == villaId);
            var checkedInVilla = _bookingService.GetCheckedInVillaNumbers(villaId);
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
            _bookingService.UpdateStatus(booking.ID, SD.StatusCheckedIn, booking.VillaNumber);
            TempData["success"] = "Booking Status Updated to Checked In!";
            return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.ID });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult CheckOut(Booking booking)
        {
            _bookingService.UpdateStatus(booking.ID, SD.StatusCheckedOut, booking.VillaNumber);
            TempData["success"] = "Booking Status Updated to Check Out!";
            return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.ID });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult CancelBooking(Booking booking)
        {
            if(booking.Status == SD.StatusApproved)
            {
                _bookingService.UpdateStatus(booking.ID, SD.StatusRefunded, 0);
                TempData["success"] = "Booking Status updated to Refunded!";
                return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.ID });
            }
            _bookingService.UpdateStatus(booking.ID, SD.StatusCancelled, 0);
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
                bookingObject = _bookingService.GetAllBookings();
            }
            else
            {
                //Get logged in User ID
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userID = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                bookingObject = _bookingService.GetAllBookings(userID);
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

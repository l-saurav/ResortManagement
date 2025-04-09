using ResortManagement.Application.Common.Interfaces;
using ResortManagement.Application.Common.Utility;
using ResortManagement.Domain.Entities;
using ResortManagement.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResortManagement.Infrastructure.Repositories
{
    public class BookingRepository : Repository<Booking>, IBookingRepository
    {
        private readonly ApplicationDBContext _dBContext;
        public BookingRepository(ApplicationDBContext dBContext) : base(dBContext)
        {
            _dBContext = dBContext;
        }

        public void Update(Booking booking)
        {
            _dBContext.Bookings.Update(booking);
        }

        public void Save()
        {
            _dBContext.SaveChanges();
        }

        public void UpdateStatus(int bookingId, string bookingStatus, int villaNumber=0)
        {
            var bookingFromDb = _dBContext.Bookings.FirstOrDefault(b => b.ID == bookingId);
            if(bookingFromDb is not null)
            {
                bookingFromDb.Status = bookingStatus;
                if(bookingStatus == SD.StatusCheckedIn)
                {
                    bookingFromDb.VillaNumber = villaNumber;
                    bookingFromDb.ActualCheckInDate = DateTime.Now;
                }
                if(bookingStatus == SD.StatusCheckedOut)
                {
                    bookingFromDb.ActualCheckOutDate = DateTime.Now;
                }

            }
        }

        public void UpdateStripePaymentID(int bookingId, string sessionId, string paymentIntentId)
        {
            var bookingFromDb = _dBContext.Bookings.FirstOrDefault(b => b.ID == bookingId);
            if(bookingFromDb is not null)
            {
                if (!string.IsNullOrEmpty(sessionId))
                {
                    bookingFromDb.StripeSessionID = sessionId;
                }
                if (!string.IsNullOrEmpty(paymentIntentId))
                {
                    bookingFromDb.StripePaymentIntentID = paymentIntentId;
                    bookingFromDb.PaymentDate = DateTime.Now;
                    bookingFromDb.isPaymentSuccessful = true;
                }
            }
        }
    }
}

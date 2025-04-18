
using ResortManagement.Domain.Entities;

namespace ResortManagement.Application.Services.Interface
{
    public interface IBookingService
    {
        IEnumerable<Booking> GetAllBookings(string userId = "", string? statusFilter = "");
        Booking GetBookingById(int id);
        void CreateBooking(Booking booking);
        void UpdateStatus(int bookingID, string bookingStatus, int villaNumber);
        void UpdateStripePaymentID(int id, string sessionId, string paymentIntentId);
        public IEnumerable<int> GetCheckedInVillaNumbers(int villaId);
    }
}

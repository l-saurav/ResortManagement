using ResortManagement.Domain.Entities;

namespace ResortManagement.Application.Common.Interfaces
{
    public interface IBookingRepository : IRepository<Booking>
    {
        void Update(Booking booking);
        void Save();
        void UpdateStatus(int bookingID, string bookingStatus, int villaNumber);
        void UpdateStripePaymentID(int id, string sessionId, string paymentIntentId);
    }
}

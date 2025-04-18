using ResortManagement.Domain.Entities;

namespace ResortManagement.Application.Common.Interfaces
{
    public interface IBookingRepository : IRepository<Booking>
    {
        void Update(Booking booking);
        void Save();
    }
}

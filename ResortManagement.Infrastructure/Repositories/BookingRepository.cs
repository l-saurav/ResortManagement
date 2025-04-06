using ResortManagement.Application.Common.Interfaces;
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
    }
}

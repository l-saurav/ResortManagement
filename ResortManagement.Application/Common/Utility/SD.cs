//Class to save static detail

using ResortManagement.Domain.Entities;

namespace ResortManagement.Application.Common.Utility
{
    public static class SD
    {
        public const string Role_Customer = "Customer";
        public const string Role_Admin = "Admin";

        public const string StatusPending = "Pending";
        public const string StatusApproved = "Approved";
        public const string StatusCheckedIn = "CheckedIn";
        public const string StatusCheckedOut = "CheckedOut";
        public const string StatusCancelled = "Cancelled";
        public const string StatusRefunded = "Refunded";

        public static int VillaRoomsAvailable_Count(int villaID, 
            List<VillaNumber> villaNumberList, DateOnly checkInDate, int nights, List<Booking> bookings)
        {
            //Count of all the available villa number
            var roomsInVilla = villaNumberList.Where(vn => vn.VillaID == villaID).Count();

            //Store how many booking collide with the date we want
            List<int> bookingInDate = new();

            int finalAvailableRoomForAllNights = int.MaxValue;
            for(int i = 0; i < nights; i++)
            {
                //Checking is villa is booked for the selected criteria
                var villasBooked = bookings.Where(b => b.CheckInDate <= checkInDate.AddDays(i) &&
                    b.CheckOutDate > checkInDate.AddDays(i) && b.VillaID == villaID);

                foreach(var booking in villasBooked)
                {
                    if (!bookingInDate.Contains(booking.ID))
                    {
                        bookingInDate.Add(booking.ID);
                    }
                }

                var totalAvailableRooms = roomsInVilla - bookingInDate.Count;
                if(totalAvailableRooms == 0)
                {
                    return 0;
                }
                else
                {
                    if (finalAvailableRoomForAllNights > totalAvailableRooms)
                    {
                        finalAvailableRoomForAllNights = totalAvailableRooms;
                    }
                }
            }
            return finalAvailableRoomForAllNights;
        }
    }
}

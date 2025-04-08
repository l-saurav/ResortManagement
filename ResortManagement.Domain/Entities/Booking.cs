using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResortManagement.Domain.Entities
{
    public class Booking
    {
        [Key]
        public int ID { get; set; }
        [Required]
        public string UserID { get; set; }
        [ForeignKey("UserID")]
        public ApplicationUser User { get; set; }

        [Required]
        public int VillaID { get; set; }
        [ForeignKey("VillaID")]
        public Villa Villa { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [Display(Name ="Email Address")]
        [EmailAddress]
        public string Email { get; set; }

        [Display(Name="Phone Number")]
        public string? PhoneNumber { get; set; }

        [Required]
        public double TotalCost { get; set; }
        [Display(Name="No. of Nights")]
        public int Nights { get; set; }
        public string? Status { get; set; }

        [Required]
        public DateTime BookingDate { get; set; }
        [Required]
        [Display(Name="Check in Date")]
        public DateOnly CheckInDate { get; set; }
        [Required]
        [Display(Name="Check out Date")]
        public DateOnly CheckOutDate { get; set; }

        public bool isPaymentSuccessful { get; set; } = false;
        public DateTime PaymentDate { get; set; }

        public string? StripeSessionID { get; set; }
        public string? StripePaymentIntentID { get; set; }

        public DateTime ActualCheckInDate { get; set; }
        public DateTime ActualCheckOutDate { get; set; }
        public int VillaNumber { get; set; }
    }
}

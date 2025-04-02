

using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResortManagement.Domain.Entities
{
    public class Amenity
    {

        [Key]
        public int ID { get; set; }
        [Required]
        [Display(Name = "Amenity Name")]
        public string Name { get; set; }
        [Display(Name="Amenity Description")]
        public string? Description { get; set; }
        [ForeignKey("Villa")]
        [Display(Name = "Villa Name")]
        public int villaID { get; set; }
        [ValidateNever]
        public Villa villa { get; set; }
    }
}

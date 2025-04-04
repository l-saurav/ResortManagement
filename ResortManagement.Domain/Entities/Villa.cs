using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using ResortManagement.Domain.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Villa
{
    public int ID { get; set; }
    [MaxLength(50)]
    public required string Name { get; set; }
    public string? Description { get; set; }
    [Display(Name = "Price per Night")]
    [Range(1, 1000)]
    public double Price { get; set; }
    [Display(Name = "Size (in Square feet)")]
    [Range(100, 10000)]
    public int Sqft { get; set; }
    [Range(1, 10)]
    public int Occupancy { get; set; }
    [NotMapped]
    public IFormFile? Image { get; set; }
    [Display(Name = "Image URL")]
    public string? ImageURL { get; set; }
    [Display(Name = "Date of Creation")]
    public DateTime? CreatedDate { get; set; }
    [Display(Name = "Date when Updated")]
    public DateTime? UpdatedDate { get; set; }

    [ValidateNever]
    public IEnumerable<Amenity> VillaAmenity { get; set; }
    [NotMapped]
    public bool isAvailable { get; set; } = true;
}
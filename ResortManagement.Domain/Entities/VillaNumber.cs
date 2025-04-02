using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResortManagement.Domain.Entities
{
    public class VillaNumber
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Display(Name ="Villa Number")]
        public int Villa_Number { get; set; }
        [ForeignKey("Villa")]
        [Display(Name = "Villa ID")]
        public int VillaID { get; set; }
        [ValidateNever]
        public Villa Villa { get; set; }
        [Display(Name ="Special Details")]
        public string? SpecialDetails { get; set; }
    }
}

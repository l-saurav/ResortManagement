using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using ResortManagement.Domain.Entities;

namespace ResortManagement.Web.Models
{
    public class VillaNumberViewModel
    {
        public VillaNumber? villaNumber { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem>? villaList { get; set; }
    }
}

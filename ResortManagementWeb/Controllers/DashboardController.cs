using Microsoft.AspNetCore.Mvc;

namespace ResortManagement.Web.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

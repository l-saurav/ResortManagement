using Microsoft.AspNetCore.Mvc;
using ResortManagement.Application.Common.Interfaces;
using ResortManagement.Application.Common.Utility;
using ResortManagement.Application.Services.Interface;
using ResortManagement.Web.Models;
using System.Linq;

namespace ResortManagement.Web.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;
        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }
        public IActionResult Index()
        {
            return View();
        }
        //Set the current and previous month
        static int previousMonth = DateTime.Now.Month == 1 ? 12 : (DateTime.Now.Month - 1);
        DateTime previousMonthStartDate = new(DateTime.Now.Year, previousMonth, 1);
        DateTime currentMonthStartDate = new(DateTime.Now.Year, DateTime.Now.Month, 1);

        public async Task<IActionResult> GetTotalBookingRadialChartData()
        {
            return Json(await _dashboardService.GetTotalBookingRadialChartData());
        }

        public async Task<IActionResult> GetRegisteredUserRadialChartData()
        {
            return Json(await _dashboardService.GetRegisteredUserRadialChartData());
        }

        public async Task<IActionResult> GetRevenueRadialChartData()
        {
            return Json(await _dashboardService.GetRevenueRadialChartData());
        }

        public async Task<IActionResult> GetBookingPieChartData()
        {
            return Json(await _dashboardService.GetBookingPieChartData());
        }

        public async Task<IActionResult> GetMemberAndBookingLineChartData()
        {
            return Json(await _dashboardService.GetMemberAndBookingLineChartData());
        }

    }
}

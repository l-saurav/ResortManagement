using ResortManagement.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResortManagement.Application.Services.Interface
{
    public interface IDashboardService
    {
        Task<RadialBarChartDTO> GetTotalBookingRadialChartData();
        Task<RadialBarChartDTO> GetRegisteredUserRadialChartData();
        Task<RadialBarChartDTO> GetRevenueRadialChartData();
        Task<PieChartDTO> GetBookingPieChartData();
        Task<LineChartDTO> GetMemberAndBookingLineChartData();
    }
}

﻿using Microsoft.AspNetCore.Mvc;
using ResortManagement.Application.Common.Interfaces;
using ResortManagement.Application.Common.Utility;
using ResortManagement.Application.Services.Interface;
using ResortManagement.Web.Models;

namespace ResortManagement.Application.Services.Implementation
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;
        public DashboardService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        //Set the current and previous month
        static int previousMonth = DateTime.Now.Month == 1 ? 12 : (DateTime.Now.Month - 1);
        DateTime previousMonthStartDate = new(DateTime.Now.Year, previousMonth, 1);
        DateTime currentMonthStartDate = new(DateTime.Now.Year, DateTime.Now.Month, 1);

        public async Task<PieChartDTO> GetBookingPieChartData()
        {

            //Get Total Booking in last 90 days
            var totalBooking = _unitOfWork.Booking.GetAll(b => b.BookingDate >= DateTime.Now.AddDays(-90) && (b.Status != SD.StatusCancelled && b.Status != SD.StatusRefunded));
            //Get Customer having only one booking
            var customerWithOneBooking = totalBooking.GroupBy(b => b.UserID).Where(b => b.Count() == 1).Select(b => b.Key).ToList();
            //Get Number of New and Returning Booking
            var bookingByNewCustomer = customerWithOneBooking.Count();
            var bookingByExistingCustomer = totalBooking.Count() - bookingByNewCustomer;
            PieChartDTO PieChartDTO = new()
            {
                Labels = new string[]
                {
                    "New Customer Booking",
                    "Returning Customer Booking"
                },
                Series = new decimal[]
                {
                    bookingByNewCustomer,bookingByExistingCustomer
                }
            };
            return PieChartDTO;
        }

        public async Task<LineChartDTO> GetMemberAndBookingLineChartData()
        {
            //Get last one month booking data
            var bookingData = _unitOfWork.Booking.GetAll(u => u.BookingDate >= DateTime.Now.AddDays(-30) && u.BookingDate <= DateTime.Now)
                .GroupBy(u => u.BookingDate.Date)
                .Select(u => new
                {
                    DateTime = u.Key,
                    NewBookingCount = u.Count()
                });
            //Get last one month Customer data
            var customerData = _unitOfWork.ApplicationUser.GetAll(u => u.CreatedAt >= DateTime.Now.AddDays(-30) && u.CreatedAt <= DateTime.Now)
                .GroupBy(u => u.CreatedAt.Date)
                .Select(u => new
                {
                    DateTime = u.Key,
                    NewCustomerCount = u.Count()
                });
            //Left join booking data and customer data so that all the booking data is available as well as common data
            var leftJoin = bookingData.GroupJoin(customerData, booking => booking.DateTime, customer => customer.DateTime, (booking, customer) => new
            {
                booking.DateTime,
                booking.NewBookingCount,
                NewCustomerCount = customer.Select(c => c.NewCustomerCount).FirstOrDefault()
            });
            var rightJoin = customerData.GroupJoin(bookingData, customer => customer.DateTime, booking => booking.DateTime, (customer, booking) => new
            {
                customer.DateTime,
                NewBookingCount = booking.Select(b => b.NewBookingCount).FirstOrDefault(),
                customer.NewCustomerCount,
            });
            //Merge Both table to generate all data
            var mergedData = leftJoin.Union(rightJoin).OrderBy(x => x.DateTime).ToList();
            //Retrieve individual data for Booking and Customer
            var newBookingData = mergedData.Select(x => x.NewBookingCount).ToArray();
            var newCustomerData = mergedData.Select(x => x.NewCustomerCount).ToArray();
            //Retrieve all Date only
            var categories = mergedData.Select(x => x.DateTime.ToString("MM/dd/yyyy")).ToArray();

            //Populate series to show on the line chart
            List<ChartData> chartDataList = new()
            {
                new ChartData
                {
                    Name = "New Bookings",
                    Data = newBookingData
                },
                new ChartData
                {
                    Name = "New Customer",
                    Data = newCustomerData
                }
            };
            LineChartDTO LineChartDTO = new()
            {
                Categories = categories,
                Series = chartDataList
            };

            return LineChartDTO;
        }

        public async Task<RadialBarChartDTO> GetRegisteredUserRadialChartData()
        {
            //Get Total User till date
            var totalUsers = _unitOfWork.ApplicationUser.GetAll();
            //Get Record of all user based on month
            var userCountOnCurrentMonth = totalUsers.Count(b => b.CreatedAt <= DateTime.Now && b.CreatedAt >= currentMonthStartDate);
            var userCountOnPreviousMonth = totalUsers.Count(b => b.CreatedAt >= previousMonthStartDate && b.CreatedAt <= currentMonthStartDate);


            return SD.GetRadialChartDataModel(totalUsers.Count(), userCountOnCurrentMonth, userCountOnPreviousMonth);
        }

        public async Task<RadialBarChartDTO> GetRevenueRadialChartData()
        {
            //Get Total Booking till date
            var totalBooking = _unitOfWork.Booking.GetAll(b => b.Status != SD.StatusPending && b.Status != SD.StatusCancelled && b.Status != SD.StatusRefunded);
            // Calculate Total Revenue gained from booking
            var totalRevenue = Convert.ToInt32(totalBooking.Sum(tb => tb.TotalCost));
            //Get Record of all booking based on month
            var revenueOnCurrentMonth = totalBooking.Where(b => b.BookingDate <= DateTime.Now && b.BookingDate >= currentMonthStartDate).Sum(b => b.TotalCost);
            var revenueOnPreviousMonth = totalBooking.Where(b => b.BookingDate >= previousMonthStartDate && b.BookingDate <= currentMonthStartDate).Sum(b => b.TotalCost);


            return SD.GetRadialChartDataModel(totalRevenue, revenueOnCurrentMonth, revenueOnPreviousMonth);
        }

        public async Task<RadialBarChartDTO> GetTotalBookingRadialChartData()
        {
            //Get Total Booking till date
            var totalBooking = _unitOfWork.Booking.GetAll(b => b.Status != SD.StatusCancelled && b.Status != SD.StatusRefunded);
            //Get Record of all booking based on month
            var bookingCountOnCurrentMonth = totalBooking.Count(b => b.BookingDate <= DateTime.Now && b.BookingDate >= currentMonthStartDate);
            var bookingCountOnPreviousMonth = totalBooking.Count(b => b.BookingDate >= previousMonthStartDate && b.BookingDate <= currentMonthStartDate);

            return SD.GetRadialChartDataModel(totalBooking.Count(), bookingCountOnCurrentMonth, bookingCountOnPreviousMonth);
        }
    }
}

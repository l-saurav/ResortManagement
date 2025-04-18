namespace ResortManagement.Web.Models
{
    public class RadialBarChartDTO
    {
        public decimal TotalCount { get; set; }
        public decimal IncreaseDecreaseAmount { get; set; }
        public bool HasRatioIncreased { get; set; }
        public int[] Series { get; set; }
    }
}

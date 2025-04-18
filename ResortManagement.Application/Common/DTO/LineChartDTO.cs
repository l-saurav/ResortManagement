namespace ResortManagement.Web.Models
{
    public class LineChartDTO
    {
        public List<ChartData> Series { get; set; }
        public string[] Categories { get; set; }
    }
    public class ChartData
    {
        public string Name { get; set; }
        public int[] Data { get; set; }
    }
}


namespace Firmeza.Admin.Models;

public class DashboardMetricsViewModel
{
    public int TotalProducts { get; set; }
    public int TotalCustomers { get; set; }
    public int TotalSales { get; set; }
    public decimal RevenueLast30Days { get; set; }
}
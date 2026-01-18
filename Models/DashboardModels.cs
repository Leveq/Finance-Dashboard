using FinanceDashboard.Web.Data.Entities;

namespace FinanceDashboard.Web.Models;

public class DashboardStats
{
    public decimal TotalIncome { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal NetSavings => TotalIncome - TotalExpenses;
    public List<MonthlyData> MonthlyTrend { get; set; } = new();
    public List<CategoryData> ExpensesByCategory { get; set; } = new();
    public List<Transaction> RecentTransactions { get; set; } = new();
}

public class MonthlyData
{
    public string Month { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal Income { get; set; }
    public decimal Expenses { get; set; }
}

public class CategoryData
{
    public string CategoryName { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Percentage { get; set; }
}

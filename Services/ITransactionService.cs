using FinanceDashboard.Web.Data;
using FinanceDashboard.Web.Data.Entities;
using FinanceDashboard.Web.Models;

namespace FinanceDashboard.Web.Services;

public interface ITransactionService
{
    Task<List<Transaction>> GetUserTransactionsAsync(string userId, TransactionType? type = null, DateTime? startDate = null, DateTime? endDate = null);
    Task<Transaction?> GetByIdAsync(int id, string userId);
    Task<Transaction> CreateAsync(Transaction transaction);
    Task UpdateAsync(Transaction transaction);
    Task DeleteAsync(int id, string userId);
    Task<List<Category>> GetCategoriesByTypeAsync(TransactionType type);
    Task<List<Category>> GetAllCategoriesAsync();
    Task<DashboardStats> GetDashboardStatsAsync(string userId);
}

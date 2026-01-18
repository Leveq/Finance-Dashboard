using FinanceDashboard.Web.Data;
using FinanceDashboard.Web.Data.Entities;
using FinanceDashboard.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceDashboard.Web.Services;

public class TransactionService : ITransactionService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public TransactionService(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<Transaction>> GetUserTransactionsAsync(
        string userId, 
        TransactionType? type = null, 
        DateTime? startDate = null, 
        DateTime? endDate = null)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var query = context.Transactions
            .Include(t => t.Category)
            .Where(t => t.UserId == userId);

        if (type.HasValue)
        {
            query = query.Where(t => t.Type == type.Value);
        }

        if (startDate.HasValue)
        {
            query = query.Where(t => t.Date >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(t => t.Date <= endDate.Value);
        }

        return await query
            .OrderByDescending(t => t.Date)
            .ThenByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<Transaction?> GetByIdAsync(int id, string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        return await context.Transactions
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
    }

    public async Task<Transaction> CreateAsync(Transaction transaction)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        transaction.CreatedAt = DateTime.UtcNow;
        context.Transactions.Add(transaction);
        await context.SaveChangesAsync();
        
        // Reload with category
        await context.Entry(transaction).Reference(t => t.Category).LoadAsync();
        return transaction;
    }

    public async Task UpdateAsync(Transaction transaction)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var existing = await context.Transactions
            .FirstOrDefaultAsync(t => t.Id == transaction.Id && t.UserId == transaction.UserId);

        if (existing == null)
        {
            throw new InvalidOperationException("Transaction not found or access denied.");
        }

        existing.Amount = transaction.Amount;
        existing.Type = transaction.Type;
        existing.CategoryId = transaction.CategoryId;
        existing.Date = transaction.Date;
        existing.Description = transaction.Description;

        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id, string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var transaction = await context.Transactions
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

        if (transaction == null)
        {
            throw new InvalidOperationException("Transaction not found or access denied.");
        }

        context.Transactions.Remove(transaction);
        await context.SaveChangesAsync();
    }

    public async Task<List<Category>> GetCategoriesByTypeAsync(TransactionType type)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        return await context.Categories
            .Where(c => c.Type == type)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<List<Category>> GetAllCategoriesAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        return await context.Categories
            .OrderBy(c => c.Type)
            .ThenBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<DashboardStats> GetDashboardStatsAsync(string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var now = DateTime.Now;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

        // Get all user transactions
        var allTransactions = await context.Transactions
            .Include(t => t.Category)
            .Where(t => t.UserId == userId)
            .ToListAsync();

        // Current month totals
        var currentMonthTransactions = allTransactions
            .Where(t => t.Date >= startOfMonth && t.Date <= endOfMonth)
            .ToList();

        var totalIncome = currentMonthTransactions
            .Where(t => t.Type == TransactionType.Income)
            .Sum(t => t.Amount);

        var totalExpenses = currentMonthTransactions
            .Where(t => t.Type == TransactionType.Expense)
            .Sum(t => t.Amount);

        // Monthly trend (last 6 months)
        var sixMonthsAgo = now.AddMonths(-5);
        var startOfSixMonthsAgo = new DateTime(sixMonthsAgo.Year, sixMonthsAgo.Month, 1);

        var monthlyTrend = allTransactions
            .Where(t => t.Date >= startOfSixMonthsAgo)
            .GroupBy(t => new { t.Date.Year, t.Date.Month })
            .Select(g => new MonthlyData
            {
                Year = g.Key.Year,
                Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM"),
                Income = g.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount),
                Expenses = g.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount)
            })
            .OrderBy(m => m.Year)
            .ThenBy(m => DateTime.ParseExact(m.Month, "MMM", System.Globalization.CultureInfo.InvariantCulture).Month)
            .ToList();

        // Expenses by category (current month)
        var expensesByCategory = currentMonthTransactions
            .Where(t => t.Type == TransactionType.Expense)
            .GroupBy(t => new { t.Category.Name, t.Category.Icon })
            .Select(g => new CategoryData
            {
                CategoryName = g.Key.Name,
                Icon = g.Key.Icon ?? "",
                Amount = g.Sum(t => t.Amount),
                Percentage = totalExpenses > 0 ? Math.Round((g.Sum(t => t.Amount) / totalExpenses) * 100, 1) : 0
            })
            .OrderByDescending(c => c.Amount)
            .ToList();

        // Recent transactions (last 5)
        var recentTransactions = allTransactions
            .OrderByDescending(t => t.Date)
            .ThenByDescending(t => t.CreatedAt)
            .Take(5)
            .ToList();

        return new DashboardStats
        {
            TotalIncome = totalIncome,
            TotalExpenses = totalExpenses,
            MonthlyTrend = monthlyTrend,
            ExpensesByCategory = expensesByCategory,
            RecentTransactions = recentTransactions
        };
    }
}

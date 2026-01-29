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

    public async Task ResetDemoDataAsync(string demoUserId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        // Delete all existing transactions for demo user
        var existingTransactions = await context.Transactions
            .Where(t => t.UserId == demoUserId)
            .ToListAsync();
        
        context.Transactions.RemoveRange(existingTransactions);
        await context.SaveChangesAsync();
        
        // Get categories for new transactions
        var categories = await context.Categories.ToListAsync();
        var salaryCategory = categories.FirstOrDefault(c => c.Name == "Salary");
        var freelanceCategory = categories.FirstOrDefault(c => c.Name == "Freelance");
        var foodCategory = categories.FirstOrDefault(c => c.Name == "Food");
        var rentCategory = categories.FirstOrDefault(c => c.Name == "Rent");
        var utilitiesCategory = categories.FirstOrDefault(c => c.Name == "Utilities");
        var transportCategory = categories.FirstOrDefault(c => c.Name == "Transportation");
        var entertainmentCategory = categories.FirstOrDefault(c => c.Name == "Entertainment");
        var shoppingCategory = categories.FirstOrDefault(c => c.Name == "Shopping");
        
        if (salaryCategory == null) return;
        
        // Create sample transactions for the last 6 months
        var transactions = new List<Transaction>();
        var random = new Random(); // Random seed for variety
        
        for (int monthsAgo = 5; monthsAgo >= 0; monthsAgo--)
        {
            var monthDate = DateTime.Today.AddMonths(-monthsAgo);
            var startOfMonth = new DateTime(monthDate.Year, monthDate.Month, 1);
            
            // Monthly salary
            transactions.Add(new Transaction
            {
                UserId = demoUserId,
                Amount = 5000 + random.Next(0, 500),
                Type = TransactionType.Income,
                CategoryId = salaryCategory.Id,
                Date = startOfMonth.AddDays(random.Next(1, 5)),
                Description = "Monthly salary",
                CreatedAt = DateTime.UtcNow
            });
            
            // Occasional freelance income
            if (random.Next(0, 3) == 0 && freelanceCategory != null)
            {
                transactions.Add(new Transaction
                {
                    UserId = demoUserId,
                    Amount = 500 + random.Next(0, 1000),
                    Type = TransactionType.Income,
                    CategoryId = freelanceCategory.Id,
                    Date = startOfMonth.AddDays(random.Next(10, 25)),
                    Description = "Freelance project",
                    CreatedAt = DateTime.UtcNow
                });
            }
            
            // Rent
            if (rentCategory != null)
            {
                transactions.Add(new Transaction
                {
                    UserId = demoUserId,
                    Amount = 1500,
                    Type = TransactionType.Expense,
                    CategoryId = rentCategory.Id,
                    Date = startOfMonth.AddDays(1),
                    Description = "Monthly rent",
                    CreatedAt = DateTime.UtcNow
                });
            }
            
            // Utilities
            if (utilitiesCategory != null)
            {
                transactions.Add(new Transaction
                {
                    UserId = demoUserId,
                    Amount = 100 + random.Next(0, 80),
                    Type = TransactionType.Expense,
                    CategoryId = utilitiesCategory.Id,
                    Date = startOfMonth.AddDays(random.Next(5, 15)),
                    Description = "Electric & water bill",
                    CreatedAt = DateTime.UtcNow
                });
            }
            
            // Food expenses (multiple per month)
            if (foodCategory != null)
            {
                var foodDescriptions = new[] { "Grocery shopping", "Restaurant dinner", "Coffee shop", "Lunch", "Fast food", "Takeout" };
                for (int i = 0; i < random.Next(8, 15); i++)
                {
                    transactions.Add(new Transaction
                    {
                        UserId = demoUserId,
                        Amount = 15 + random.Next(0, 60),
                        Type = TransactionType.Expense,
                        CategoryId = foodCategory.Id,
                        Date = startOfMonth.AddDays(random.Next(1, 28)),
                        Description = foodDescriptions[random.Next(foodDescriptions.Length)],
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
            
            // Transportation
            if (transportCategory != null)
            {
                for (int i = 0; i < random.Next(2, 5); i++)
                {
                    transactions.Add(new Transaction
                    {
                        UserId = demoUserId,
                        Amount = 20 + random.Next(0, 50),
                        Type = TransactionType.Expense,
                        CategoryId = transportCategory.Id,
                        Date = startOfMonth.AddDays(random.Next(1, 28)),
                        Description = random.Next(0, 2) == 0 ? "Gas" : "Uber ride",
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
            
            // Entertainment
            if (entertainmentCategory != null && random.Next(0, 2) == 0)
            {
                var entertainmentDescriptions = new[] { "Movie tickets", "Netflix subscription", "Concert tickets", "Video game", "Streaming service" };
                transactions.Add(new Transaction
                {
                    UserId = demoUserId,
                    Amount = 30 + random.Next(0, 70),
                    Type = TransactionType.Expense,
                    CategoryId = entertainmentCategory.Id,
                    Date = startOfMonth.AddDays(random.Next(10, 25)),
                    Description = entertainmentDescriptions[random.Next(entertainmentDescriptions.Length)],
                    CreatedAt = DateTime.UtcNow
                });
            }
            
            // Shopping
            if (shoppingCategory != null && random.Next(0, 3) == 0)
            {
                transactions.Add(new Transaction
                {
                    UserId = demoUserId,
                    Amount = 50 + random.Next(0, 200),
                    Type = TransactionType.Expense,
                    CategoryId = shoppingCategory.Id,
                    Date = startOfMonth.AddDays(random.Next(1, 28)),
                    Description = "Online shopping",
                    CreatedAt = DateTime.UtcNow
                });
            }
        }
        
        context.Transactions.AddRange(transactions);
        await context.SaveChangesAsync();
    }
}

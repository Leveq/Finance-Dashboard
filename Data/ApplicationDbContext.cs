using FinanceDashboard.Web.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FinanceDashboard.Web.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Salary", Type = TransactionType.Income, Icon = "💰" },
                new Category { Id = 2, Name = "Freelance", Type = TransactionType.Income, Icon = "💼" },
                new Category { Id = 3, Name = "Investments", Type = TransactionType.Income, Icon = "📈" },
                new Category { Id = 4, Name = "Food & Dining", Type = TransactionType.Expense, Icon = "🍔" },
                new Category { Id = 5, Name = "Rent", Type = TransactionType.Expense, Icon = "🏠" },
                new Category { Id = 6, Name = "Utilities", Type = TransactionType.Expense, Icon = "💡" },
                new Category { Id = 7, Name = "Transportation", Type = TransactionType.Expense, Icon = "🚗" },
                new Category { Id = 8, Name = "Entertainment", Type = TransactionType.Expense, Icon = "🎮" },
                new Category { Id = 9, Name = "Healthcare", Type = TransactionType.Expense, Icon = "🏥" },
                new Category { Id = 10, Name = "Shopping", Type = TransactionType.Expense, Icon = "🛍️" }
            );
        }
    }
}

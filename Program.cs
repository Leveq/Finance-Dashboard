using FinanceDashboard.Web.Components;
using FinanceDashboard.Web.Data;
using FinanceDashboard.Web.Data.Entities;
using FinanceDashboard.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException(
        "Connection string 'DefaultConnection' not found. " +
        "Make sure it's configured in Azure App Service -> Environment variables -> Connection strings");
}

builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Also register DbContext for Identity (it requires scoped DbContext)
builder.Services.AddScoped(sp => sp.GetRequiredService<IDbContextFactory<ApplicationDbContext>>().CreateDbContext());

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/login";
    options.LogoutPath = "/logout";
    options.AccessDeniedPath = "/access-denied";
});

// Register application services
builder.Services.AddScoped<ITransactionService, TransactionService>();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorization();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Auto-migrate database on startup (creates tables if they don't exist)
try
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();
    
    // Seed demo user
    await SeedDemoUserAsync(scope.ServiceProvider);
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred while migrating the database.");
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

// Seed demo user with sample transactions
async Task SeedDemoUserAsync(IServiceProvider serviceProvider)
{
    var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
    
    const string demoEmail = "demo@financedashboard.com";
    const string demoPassword = "Demo123!";
    
    // Check if demo user already exists
    var existingUser = await userManager.FindByEmailAsync(demoEmail);
    if (existingUser != null) return;
    
    // Create demo user
    var demoUser = new ApplicationUser
    {
        UserName = demoEmail,
        Email = demoEmail,
        FirstName = "Demo",
        EmailConfirmed = true
    };
    
    var result = await userManager.CreateAsync(demoUser, demoPassword);
    if (!result.Succeeded) return;
    
    // Get categories for transactions
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
    var random = new Random(42); // Fixed seed for consistent demo data
    
    for (int monthsAgo = 5; monthsAgo >= 0; monthsAgo--)
    {
        var monthDate = DateTime.Today.AddMonths(-monthsAgo);
        var startOfMonth = new DateTime(monthDate.Year, monthDate.Month, 1);
        
        // Monthly salary
        transactions.Add(new Transaction
        {
            UserId = demoUser.Id,
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
                UserId = demoUser.Id,
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
                UserId = demoUser.Id,
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
                UserId = demoUser.Id,
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
            for (int i = 0; i < random.Next(8, 15); i++)
            {
                transactions.Add(new Transaction
                {
                    UserId = demoUser.Id,
                    Amount = 15 + random.Next(0, 60),
                    Type = TransactionType.Expense,
                    CategoryId = foodCategory.Id,
                    Date = startOfMonth.AddDays(random.Next(1, 28)),
                    Description = GetRandomFoodDescription(random),
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
                    UserId = demoUser.Id,
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
            transactions.Add(new Transaction
            {
                UserId = demoUser.Id,
                Amount = 30 + random.Next(0, 70),
                Type = TransactionType.Expense,
                CategoryId = entertainmentCategory.Id,
                Date = startOfMonth.AddDays(random.Next(10, 25)),
                Description = GetRandomEntertainmentDescription(random),
                CreatedAt = DateTime.UtcNow
            });
        }
        
        // Shopping
        if (shoppingCategory != null && random.Next(0, 3) == 0)
        {
            transactions.Add(new Transaction
            {
                UserId = demoUser.Id,
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

string GetRandomFoodDescription(Random random)
{
    var descriptions = new[] { "Grocery shopping", "Restaurant dinner", "Coffee shop", "Lunch", "Fast food", "Takeout" };
    return descriptions[random.Next(descriptions.Length)];
}

string GetRandomEntertainmentDescription(Random random)
{
    var descriptions = new[] { "Movie tickets", "Netflix subscription", "Concert tickets", "Video game", "Streaming service" };
    return descriptions[random.Next(descriptions.Length)];
}


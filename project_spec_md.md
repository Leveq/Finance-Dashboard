# Finance Dashboard - Personal Portfolio Project

## Project Overview
Full-stack personal finance tracking application built with Blazor Server, ASP.NET Core 8, and SQL Server. Demonstrates authentication, CRUD operations, data visualization, and cloud deployment.

**Purpose:** Portfolio piece to showcase during contract job interviews  
**Timeline:** 2-3 days (Jan 10-12, 2026)  
**Target Audience:** Recruiters and hiring managers for .NET contract roles ($60-90/hr)

---

## Tech Stack

### Frontend
- Blazor Server (ASP.NET Core 8)
- Bootstrap 5
- Chart.js (via Blazor wrapper)
- Responsive design

### Backend
- ASP.NET Core 8
- Entity Framework Core 8
- ASP.NET Identity (authentication)
- REST API patterns

### Database
- SQL Server LocalDB (development)
- Azure SQL Database (production)

### DevOps
- Git / GitHub
- GitHub Actions (CI/CD)
- Docker
- Azure App Service

---

## Database Schema

### AspNetUsers (Identity)
- Id (string, PK)
- Email
- PasswordHash
- [Other Identity fields]

### Transactions
- Id (int, PK)
- UserId (string, FK → AspNetUsers)
- Amount (decimal(18,2))
- Type (enum: Income=1, Expense=2)
- CategoryId (int, FK → Categories)
- Date (datetime)
- Description (nvarchar(500))
- CreatedAt (datetime)

### Categories
- Id (int, PK)
- Name (nvarchar(100))
- Type (enum: Income=1, Expense=2)
- Icon (nvarchar(50)) - emoji
- UserId (string, FK, nullable) - null for system categories

**Seeded Categories:**
- Income: Salary 💰, Freelance 💼, Investments 📈
- Expenses: Food 🍔, Rent 🏠, Utilities 💡, Transportation 🚗, Entertainment 🎮, Healthcare 🏥, Shopping 🛍️

---

## Features Breakdown

### ✅ Phase 1: Authentication (COMPLETE)
**Status:** Done  
**Time:** 3 hours  
**Deliverables:**
- [x] User registration with validation
- [x] Login/logout functionality
- [x] Protected routes with [Authorize]
- [x] ASP.NET Identity integration
- [x] Secure password hashing (PBKDF2)
- [x] Cookie-based authentication

**Routes:**
- `/login` - Login page
- `/register` - Registration page
- `/logout` - Logout redirect
- `/` or `/dashboard` - Protected dashboard

---

### 🚧 Phase 2: Transaction Management (IN PROGRESS)
**Status:** Starting now  
**Time:** 2-3 hours  
**Goal:** Full CRUD operations for transactions

#### Features to Build:
1. **Add Transaction Modal/Form**
   - Amount input (decimal)
   - Type selector (Income/Expense radio buttons)
   - Category dropdown (filtered by type)
   - Date picker (default: today)
   - Description textarea (optional)
   - Validation (amount > 0, required fields)

2. **Transaction List/Table**
   - Display all user's transactions
   - Sortable columns (date, amount, category)
   - Filter by date range
   - Filter by type (Income/Expense/All)
   - Search by description
   - Pagination (10 per page)

3. **Edit Transaction**
   - Click row → open edit modal
   - Pre-populate form with existing data
   - Update functionality
   - Success/error messages

4. **Delete Transaction**
   - Confirmation modal ("Are you sure?")
   - Hard delete
   - Success message

#### Components to Create:
- `Pages/Transactions.razor` - Main transaction management page
- `Components/Transactions/TransactionForm.razor` - Add/Edit form modal
- `Components/Transactions/TransactionList.razor` - Table/grid component
- `Services/ITransactionService.cs` - Service interface
- `Services/TransactionService.cs` - Business logic

#### Service Methods Needed:
```csharp
Task<List<Transaction>> GetUserTransactions(string userId, DateTime? startDate = null, DateTime? endDate = null);
Task<Transaction?> GetById(int id, string userId);
Task<Transaction> CreateTransaction(Transaction transaction);
Task UpdateTransaction(Transaction transaction);
Task DeleteTransaction(int id, string userId);
Task<List<Category>> GetCategoriesByType(TransactionType type);
Task<List<Category>> GetAllCategories();
```

---

### 📊 Phase 3: Dashboard & Visualizations (TODO)
**Status:** Not started  
**Time:** 3-4 hours  
**Goal:** Interactive dashboard with charts

#### Features to Build:
1. **Stats Cards (Top Row)**
   - Total Income (this month) - Green card 💰
   - Total Expenses (this month) - Red card 💸
   - Net Savings (income - expenses) - Blue card 🏦
   - Large numbers with icons

2. **Line Chart: Income vs Expenses Over Time**
   - Last 6 months of data
   - Two lines: Income (green), Expenses (red)
   - X-axis: Months
   - Y-axis: Dollar amounts
   - Tooltips on hover

3. **Pie Chart: Expenses by Category**
   - Current month expenses breakdown
   - Color-coded by category
   - Show percentage and dollar amount
   - Legend with category names

4. **Recent Transactions Widget**
   - Last 5 transactions
   - Mini table format
   - Link to full Transactions page

#### Dashboard Layout:
```
+----------------------------------+
|  💰 Income    💸 Expenses  🏦 Savings  |  <- Stats Cards
+----------------------------------+
|                                  |
|   📈 Income vs Expenses (6mo)    |  <- Line Chart
|                                  |
+------------------+---------------+
|  🥧 Expenses by  |  📋 Recent    |
|     Category     |  Transactions |
+------------------+---------------+
```

#### Service Methods Needed:
```csharp
Task<DashboardStats> GetDashboardStats(string userId, DateTime? startDate = null, DateTime? endDate = null);

public class DashboardStats
{
    public decimal TotalIncome { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal NetSavings { get; set; }
    public List<MonthlyData> MonthlyTrend { get; set; }
    public List<CategoryData> ExpensesByCategory { get; set; }
    public List<Transaction> RecentTransactions { get; set; }
}

public class MonthlyData
{
    public string Month { get; set; }
    public decimal Income { get; set; }
    public decimal Expenses { get; set; }
}

public class CategoryData
{
    public string CategoryName { get; set; }
    public string Icon { get; set; }
    public decimal Amount { get; set; }
    public decimal Percentage { get; set; }
}
```

#### Chart.js Integration:
- Install: `PSC.Blazor.Components.Chartjs`
- Create reusable chart components
- Configure colors, legends, tooltips

---

### 🚀 Phase 4: Polish & Deployment (TODO)
**Status:** Not started  
**Time:** 2-3 hours  
**Goal:** Production-ready deployment

#### Tasks:
1. **Docker Setup**
   - Create `Dockerfile`
   - Create `docker-compose.yml`
   - Test local Docker build

2. **Azure Deployment**
   - Create Azure SQL Database
   - Create Azure App Service (B1 tier)
   - Configure connection strings
   - Deploy via GitHub Actions

3. **GitHub Actions CI/CD**
   - `.github/workflows/azure-deploy.yml`
   - Build on every push to main
   - Run tests (if any)
   - Auto-deploy to Azure

4. **Unit Tests (Minimal)**
   - 3-5 basic tests to show you can do it
   - `TransactionServiceTests.cs`
   - Test: GetDashboardStats returns correct totals
   - Test: CreateTransaction validates amount > 0
   - Test: User can only see their own transactions

5. **README.md**
   - Project description
   - Features list
   - Tech stack
   - Screenshots (3-4 images)
   - Local setup instructions
   - Live demo link
   - Architecture overview

6. **UI Polish**
   - Mobile responsive check
   - Loading spinners
   - Error states
   - Empty states ("No transactions yet")
   - Success toasts/alerts

---

## Development Guidelines

### Code Standards
- Clean, readable code
- Proper naming conventions (PascalCase for public, camelCase for private)
- Comments for complex logic only
- Repository pattern for data access
- Service layer for business logic
- Dependency injection throughout

### UI/UX Principles
- Bootstrap 5 utility classes
- Consistent spacing (mb-3, mt-4, etc.)
- Form validation messages
- Loading states for async operations
- Confirmation modals for destructive actions
- Mobile-first responsive design

### Security
- [Authorize] attribute on protected pages
- User can only access their own data (UserId filtering)
- SQL injection prevention (EF parameterized queries)
- XSS prevention (Razor automatic encoding)
- CSRF protection (built-in with Blazor forms)

---

## Current Status

### ✅ Completed
- Project setup (.NET 8 Blazor Server)
- Database schema & migrations
- ASP.NET Identity authentication
- Login/Register/Logout pages
- Protected routing

### 🚧 In Progress
- Phase 2: Transaction Management

### 📋 Next Steps
1. Create TransactionService
2. Build transaction form modal
3. Build transaction list component
4. Wire up CRUD operations
5. Test thoroughly

---

## Portfolio Presentation Notes

**When explaining to recruiters:**

"I built this personal finance dashboard to demonstrate my full-stack .NET skills. It uses Blazor Server with ASP.NET Identity for authentication, Entity Framework Core for data access, and Chart.js for visualizations. The app lets users track income and expenses, categorize transactions, and view spending analytics through interactive charts. It's deployed to Azure with CI/CD via GitHub Actions and includes Docker containerization."

**Key talking points:**
- Full-stack ownership (DB → UI)
- Authentication & authorization
- CRUD operations with validation
- Data visualization
- Cloud deployment
- Production-ready code quality

**Live demo flow:**
1. Show registration/login
2. Add a few transactions (mix of income/expenses)
3. Navigate to dashboard - show charts
4. Filter/search transactions
5. Edit/delete transaction
6. Mention Azure deployment & CI/CD

---

## Time Tracking

- **Phase 1 (Auth):** 3 hours ✅
- **Phase 2 (Transactions):** 2-3 hours (estimate)
- **Phase 3 (Dashboard):** 3-4 hours (estimate)
- **Phase 4 (Deploy):** 2-3 hours (estimate)

**Total:** 10-13 hours over 2-3 days

---

## Resources

- [Blazor Documentation](https://learn.microsoft.com/en-us/aspnet/core/blazor/)
- [Chart.js Blazor Wrapper](https://github.com/mariusmuntean/ChartJs.Blazor)
- [ASP.NET Identity](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity)
- [Azure App Service](https://learn.microsoft.com/en-us/azure/app-service/)

---

## Notes

- Keep scope tight - this is a portfolio piece, not production SaaS
- Prioritize working features over perfect code
- Focus on demonstrating core skills recruiters want to see
- Must be deployable and demo-able by Monday morning
- Budget tracking feature is nice-to-have (Phase 5 if time permits)
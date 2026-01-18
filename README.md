# ?? Finance Dashboard

A full-stack personal finance tracking application built with Blazor Server, ASP.NET Core 8, and SQL Server. Track income, expenses, and visualize spending patterns with interactive charts.

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat&logo=dotnet)
![Blazor](https://img.shields.io/badge/Blazor-Server-512BD4?style=flat&logo=blazor)
![SQL Server](https://img.shields.io/badge/SQL%20Server-LocalDB-CC2927?style=flat&logo=microsoftsqlserver)
![Bootstrap](https://img.shields.io/badge/Bootstrap-5.0-7952B3?style=flat&logo=bootstrap)

## ? Features

- ** Secure Authentication** - User registration and login with ASP.NET Identity
- ** Transaction Management** - Full CRUD operations for income and expenses
- ** Interactive Dashboard** - Real-time statistics and visualizations
- ** Charts & Analytics** - Income vs expenses trends, spending by category
- ** Modern UI** - Responsive Bootstrap 5 design
- ** Data Security** - Users can only access their own data

## ??? Screenshots

### Dashboard
The dashboard displays key financial metrics at a glance:
- Monthly income, expenses, and net savings
- 6-month trend line chart
- Expense breakdown by category (donut chart)
- Recent transactions

### Transactions
Manage all your financial transactions:
- Add/Edit/Delete transactions
- Filter by type (Income/Expense)
- Filter by date range
- Categorized with icons

##  Tech Stack

### Frontend
- **Blazor Server** - Interactive server-side UI
- **Bootstrap 5** - Responsive CSS framework
- **ApexCharts** - Modern chart library

### Backend
- **ASP.NET Core 8** - Web framework
- **Entity Framework Core 8** - ORM
- **ASP.NET Identity** - Authentication & authorization

### Database
- **SQL Server LocalDB** - Development
- **Azure SQL Database** - Production

### DevOps
- **Docker** - Containerization
- **GitHub Actions** - CI/CD pipeline
- **Azure App Service** - Cloud hosting

##  Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server LocalDB](https://docs.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb) or SQL Server
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/Leveq/Finance-Dashboard.git
   cd Finance-Dashboard
   ```

2. **Update the connection string** (if needed)
   
   Edit `FinanceDashboard.Web/appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=FinanceDashboard;Trusted_Connection=True;"
     }
   }
   ```

3. **Apply database migrations**
   ```bash
   cd FinanceDashboard.Web
   dotnet ef database update
   ```

4. **Run the application**
   ```bash
   dotnet run
   ```

5. **Open in browser**
   
   Navigate to `https://localhost:5001` or `http://localhost:5000`

### Running with Docker

```bash
# Build and run with Docker Compose
docker-compose up --build

# Access the app at http://localhost:5000
```

## Project Structure

```
FinanceDashboard/
??? FinanceDashboard.Web/
?   ??? Components/
?   ?   ??? Account/          # Login, Register, Logout
?   ?   ??? Dashboard/        # Stats cards, charts
?   ?   ??? Layout/           # MainLayout, NavMenu
?   ?   ??? Pages/            # Dashboard, Transactions
?   ?   ??? Transactions/     # Form, List components
?   ??? Data/
?   ?   ??? Entities/         # Transaction, Category
?   ?   ??? ApplicationDbContext.cs
?   ??? Models/               # DTOs
?   ??? Services/             # Business logic
?   ??? Program.cs
??? FinanceDashboard.Tests/   # Unit tests
??? Dockerfile
??? docker-compose.yml
??? README.md
```

## Running Tests

```bash
dotnet test
```

## Configuration

### Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `ConnectionStrings__DefaultConnection` | Database connection string | LocalDB |
| `ASPNETCORE_ENVIRONMENT` | Environment (Development/Production) | Development |

### Azure Deployment

1. Create an Azure SQL Database
2. Create an Azure App Service (B1 tier or higher)
3. Add `AZURE_WEBAPP_PUBLISH_PROFILE` secret to GitHub repository
4. Update `AZURE_WEBAPP_NAME` in `.github/workflows/azure-deploy.yml`
5. Push to `main` branch to trigger deployment

## API / Service Methods

```csharp
// Transaction operations
Task<List<Transaction>> GetUserTransactionsAsync(string userId, TransactionType? type, DateTime? startDate, DateTime? endDate);
Task<Transaction> CreateAsync(Transaction transaction);
Task UpdateAsync(Transaction transaction);
Task DeleteAsync(int id, string userId);

// Dashboard statistics
Task<DashboardStats> GetDashboardStatsAsync(string userId);
```

## Security Features

- **Authentication**: ASP.NET Identity with secure password hashing (PBKDF2)
- **Authorization**: `[Authorize]` attribute on protected pages
- **Data Isolation**: Users can only access their own transactions
- **SQL Injection Prevention**: Entity Framework parameterized queries
- **XSS Prevention**: Razor automatic HTML encoding
- **CSRF Protection**: Built-in Blazor form protection

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Author

**Kacy Leveck**
- GitHub: [@Leveq](https://github.com/Leveq)

---

? If you found this project helpful, please give it a star!

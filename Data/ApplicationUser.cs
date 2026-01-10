using FinanceDashboard.Web.Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace FinanceDashboard.Web.Data;

public class ApplicationUser : IdentityUser
{
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
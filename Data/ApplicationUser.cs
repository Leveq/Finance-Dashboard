using FinanceDashboard.Web.Data.Entities;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace FinanceDashboard.Web.Data;

public class ApplicationUser : IdentityUser
{
    [MaxLength(100)]
    public string? FirstName { get; set; }

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
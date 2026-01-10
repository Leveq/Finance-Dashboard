using FinanceDashboard.Web.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace FinanceDashboard.Web.Data;

public class Category
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public TransactionType Type { get; set; }

    [MaxLength(50)]
    public string? Icon { get; set; }

    public string? UserId { get; set; }

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
using FinanceTracker.Helpers;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceTracker.Models;

// Represents an expense category in the finance tracker.
public class Category
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [NotMapped]
    public string DisplayName => CategoryTranslator.Translate(Name);
}
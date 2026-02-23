using System.ComponentModel.DataAnnotations;

namespace FinanceTracker.Models
{
    public class MonthlyBudget
    {
        [Key]
        public int Id { get; set; }

        public int Year { get; set; }
        public int Month { get; set; }

        public decimal Limit { get; set; }
    }
}
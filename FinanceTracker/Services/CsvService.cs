using FinanceTracker.Models;
using FinanceTracker.Services.Interfaces;
using System.Globalization;
using System.IO;
using System.Text;

namespace FinanceTracker.Services;

public class CsvService : ICsvService
{
    public void Export(string filePath, IEnumerable<Expense> expenses, decimal monhtlyLimit)
    {
        var lines = new List<string>
        {
            "Id;Name;Amount;Category;Date;Limit"
        };

        bool first = true;

        foreach (var e in expenses)
        {
            var limitValue = first ? monhtlyLimit.ToString(CultureInfo.InvariantCulture) : "";
            first = false;

            lines.Add($"{e.Id};{e.Name};{e.Amount.ToString(CultureInfo.InvariantCulture)};{e.Category?.Name};{e.Date:yyyy-MM-dd};{limitValue}");
        }

        File.WriteAllLines(filePath, lines, Encoding.UTF8);
    }

    public (List<Expense> expenses, decimal monthlyLimit) Import(string filePath)
    {
        var lines = File.ReadAllLines(filePath, Encoding.UTF8);

        var expenses = new List<Expense>();
        decimal monthlyLimit = 0;

        if (lines.Length <= 1)
            return (expenses, monthlyLimit);

        for (int i = 1; i < lines.Length; i++)
        {
            var parts = lines[i].Split(';');

            if (parts.Length < 6)
                continue;

            if (i == 1 && !string.IsNullOrWhiteSpace(parts[5]))
                monthlyLimit = decimal.Parse(parts[5], CultureInfo.InvariantCulture);

            expenses.Add(new Expense
            {
                Id = int.Parse(parts[0]),
                Name = parts[1],
                Amount = decimal.Parse(parts[2], CultureInfo.InvariantCulture),
                Date = DateTime.ParseExact(parts[4], "yyyy-MM-dd", CultureInfo.InvariantCulture),
                Category = new Category { Name = parts[3] }
            });
        }

        return (expenses, monthlyLimit);
    }
}
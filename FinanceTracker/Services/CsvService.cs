using FinanceTracker.Models;
using FinanceTracker.Services.Interfaces;
using System.Globalization;
using System.IO;
using System.Text;

namespace FinanceTracker.Services;

// Service for importing and exporting expenses to/from CSV files
public class CsvService : ICsvService
{
    // Export a list of expenses and the monthly limit to a CSV file
    public void Export(string filePath, IEnumerable<Expense> expenses, decimal monhtlyLimit)
    {
        // First line is the CSV header
        var lines = new List<string>
        {
            "Id;Name;Amount;Category;Date;Limit"
        };

        bool first = true; // only include the monthly limit in the first row

        foreach (var e in expenses)
        {
            var limitValue = first ? monhtlyLimit.ToString(CultureInfo.InvariantCulture) : "";
            first = false;

            // Create a CSV line with semicolon separator and invariant culture formatting
            lines.Add($"{e.Id};{e.Name};{e.Amount.ToString(CultureInfo.InvariantCulture)};{e.Category?.Name};{e.Date:yyyy-MM-dd};{limitValue}");
        }

        // Write all lines to the file with UTF8 encoding
        File.WriteAllLines(filePath, lines, Encoding.UTF8);
    }

    // Import expenses and monthly limit from a CSV file
    public (List<Expense> expenses, decimal monthlyLimit) Import(string filePath)
    {
        var lines = File.ReadAllLines(filePath, Encoding.UTF8);

        var expenses = new List<Expense>();
        decimal monthlyLimit = 0;

        // If file is empty or only has header, return empty list and zero limit
        if (lines.Length <= 1)
            return (expenses, monthlyLimit);

        // Start from 1 to skip header line
        for (int i = 1; i < lines.Length; i++)
        {
            var parts = lines[i].Split(';');

            // Skip invalid lines
            if (parts.Length < 6)
                continue;

            // Only read monthly limit from the first data line
            if (i == 1 && !string.IsNullOrWhiteSpace(parts[5]))
                monthlyLimit = decimal.Parse(parts[5], CultureInfo.InvariantCulture);

            // Create Expense object from CSV line
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
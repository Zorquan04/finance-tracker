using FinanceTracker.Resources;

namespace FinanceTracker.Helpers;

public static class CategoryTranslator
{
    // Translate internal category code to user-friendly display name
    public static string Translate(string code)
    {
        return code switch
        {
            "All" => AppResources.Category_All,
            "Food" => AppResources.Category_Food,
            "Transport" => AppResources.Category_Transport,
            "Entertainment" => AppResources.Category_Entertainment,
            "Bills" => AppResources.Category_Bills,
            "Other" => AppResources.Category_Other,
            _ => code
        };
    }
}
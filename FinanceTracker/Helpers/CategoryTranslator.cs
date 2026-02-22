using FinanceTracker.Resources;

namespace FinanceTracker.Helpers;

public static class CategoryTranslator
{
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
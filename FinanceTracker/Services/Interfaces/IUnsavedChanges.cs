namespace FinanceTracker.Services.Interfaces;

public interface IUnsavedChanges
{
    bool HasUnsavedChanges { get; }
}
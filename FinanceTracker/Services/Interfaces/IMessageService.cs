namespace FinanceTracker.Services.Interfaces;

public interface IMessageService
{
    void ShowInfo(string message, string title = "Info");
    void ShowWarning(string message, string title = "Warning");
    bool Confirm(string message, string title = "Confirm");
}
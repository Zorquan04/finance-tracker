using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FinanceTracker.ViewModels;

// Base ViewModel implementing INotifyPropertyChanged to support property binding
public class BaseViewModel : INotifyPropertyChanged
{
    // Event raised when a property value changes
    public event PropertyChangedEventHandler? PropertyChanged;

    // Helper method to raise PropertyChanged event
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    // Generic helper method to set a property's value and notify changes if value is different
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false; // No change, no notification needed

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
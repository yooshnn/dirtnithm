using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Dirtnithm.App.Mvvm;

public abstract class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    // Only updates the field and raises the event when the value actually changes.
    // The return value lets callers run follow-up logic conditionally,
    // e.g. if (SetProperty(...)) SaveSettings();
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }
        field = value;
        OnPropertyChanged(name);
        return true;
    }
}

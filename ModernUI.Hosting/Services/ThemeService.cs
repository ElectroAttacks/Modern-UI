using System.ComponentModel;
using System.Runtime.CompilerServices;

using ModernUI.Hosting.Contracts.Services;
using ModernUI.Hosting.Models;

namespace ModernUI.Hosting.Services;

/// <summary>
///     Represents a service that manages the theme of the application.
/// </summary>
public abstract class ThemeService : IThemeService
{

    private AppTheme _selectedTheme;

    private bool _synchronizeAccent;

    private object _accentColor;


    /// <inheritdoc />
    public AppTheme SelectedTheme
    {
        get
        {
            return _selectedTheme;
        }
        set
        {
            if (_selectedTheme != value)
            {
                OnThemeChanging(value);
                OnPropertyChanging();

                _selectedTheme = value;

                OnPropertyChanged();
            }
        }
    }

    /// <inheritdoc />
    public bool SynchronizeAccent
    {
        get
        {
            return _synchronizeAccent;
        }
        set
        {
            if (_synchronizeAccent != value)
            {
                OnPropertyChanging();

                _synchronizeAccent = value;

                OnPropertyChanged();
            }
        }
    }

    /// <inheritdoc />
    public event PropertyChangingEventHandler? PropertyChanging;

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;


    public void SetAccent(object accentColor) => throw new System.NotImplementedException();

    protected void OnPropertyChanging([CallerMemberName] string propertyName = "")
        => PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));

    protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


    protected abstract void OnThemeChanging(AppTheme newValue);
}

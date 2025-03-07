using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

using Microsoft.Extensions.Logging;
using Microsoft.Win32;

using ModernUI.Hosting.Contracts.Services;
using ModernUI.Hosting.Wpf.Contracts.Services;
using ModernUI.Hosting.Wpf.Models;

namespace ModernUI.Hosting.Wpf.Services;

/// <summary>
///     Provides a service to manage the application theme and accent color.
/// </summary>
public sealed class ThemeService : IThemeService
{

    #region Fields & Properties

    private const string LibraryName = "ModernUI";

    private ResourceDictionary? _appThemeDictionary;

    private readonly ILogger _logger;


    /// <inheritdoc/>
    public ApplicationTheme Theme
    {
        get;

        set
        {
            if (field != value)
            {
                OnPropertyChanging();

                LoadTheme(value);

                field = value;

                OnPropertyChanged();
            }
        }
    }

    /// <inheritdoc/>
    public Color AccentColor
    {
        get;

        set
        {
            if (field != value)
            {
                OnPropertyChanging();

                LoadAccent(value);

                field = value;

                OnPropertyChanged();
            }
        }
    }

    /// <inheritdoc/>
    public bool UseSystemAccent
    {
        get;

        set
        {
            if (field != value)
            {
                OnPropertyChanging();

                LoadAccent(AccentColor);

                field = value;

                OnPropertyChanged();
            }
        }
    }

    #endregion


    #region Events & Delegates

    /// <inheritdoc/>
    public event PropertyChangingEventHandler? PropertyChanging;

    /// <summary>
    ///     Raises the <see cref="PropertyChanging"/> event.
    /// </summary>
    /// <param name="propertyName">
    ///     The name of the property that is changing.
    /// </param>
    private void OnPropertyChanging([CallerMemberName] string propertyName = "")
        => PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));


    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    ///     Raises the <see cref="PropertyChanged"/> event.
    /// </summary>
    /// <param name="propertyName">
    ///     The name of the property that has changed.
    /// </param>
    private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


    /// <summary>
    ///     Handles the <see cref="SystemEvents.UserPreferenceChanged"/> event.
    /// </summary>
    /// <param name="sender">
    ///     The source of the event.
    /// </param>
    /// <param name="e">
    ///     The event data.
    /// </param>
    private void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
    {
        if (e.Category is UserPreferenceCategory.General) // Theme & Accent changes are categorized as General.
        {
            LoadTheme(Theme);
            LoadAccent(AccentColor);
        }
    }

    #endregion



    /// <summary>
    ///     Initializes a new instance of the <see cref="ThemeService"/> class.
    /// </summary>
    /// <param name="logger">
    ///     The logger to use for logging.
    /// </param>
    /// <param name="localSettingService">
    ///     The local setting service to use for persisting and restoring settings.
    /// </param>
    public ThemeService(ILogger<ThemeService> logger, ILocalSettingService localSettingService)
    {
        _logger = logger;

        localSettingService.GetOrAdd(nameof(ThemeService), nameof(Theme), ApplicationTheme.SystemDefault)
            .SubscribeAndExecute(value => Theme = value);

        localSettingService.GetOrAdd(nameof(ThemeService), nameof(AccentColor), SystemColors.AccentColor)
            .SubscribeAndExecute(value => AccentColor = value);

        localSettingService.GetOrAdd(nameof(ThemeService), nameof(UseSystemAccent), true)
            .SubscribeAndExecute(value => UseSystemAccent = value);

        SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
    }


    /// <summary>
    ///     Loads the specified accent color into the application resources.
    /// </summary>
    /// <param name="accent">
    ///     The accent color to load.
    /// </param>
    private void LoadAccent(Color accent)
    {
        try
        {
            if (UseSystemAccent)
            {
                accent = SystemColors.AccentColor;
            }

            Application.Current.Resources[nameof(AccentColor)] = accent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ServiceName}: An unexpected error occurred while loading the accent color.", nameof(ThemeService));
        }
    }


    /// <summary>
    ///     Loads the specified theme into the application resources.
    /// </summary>
    /// <param name="theme">
    ///     The theme to load.
    /// </param>
    private void LoadTheme(ApplicationTheme theme)
    {
        try
        {
            if (Theme is ApplicationTheme.SystemDefault)
            {
                theme = GetSystemTheme();
            }

            if (Application.Current.Resources.MergedDictionaries is Collection<ResourceDictionary> { Count: > 0 } mergedDictionaries)
            {
                if (_appThemeDictionary is not null)
                {
                    mergedDictionaries.Remove(_appThemeDictionary);
                }

                var newThemeDictionary = new ResourceDictionary()
                {
                    Source = new Uri($"pack://application:,,,/{LibraryName};component/Resources/Themes/{theme}Theme.xaml")
                };

                mergedDictionaries.Add(newThemeDictionary);

                _appThemeDictionary = newThemeDictionary;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ServiceName}: An unexpected error occurred while loading the theme.", nameof(ThemeService));
        }
    }


    /// <summary>
    ///     Gets the current system theme.
    /// </summary>
    /// <returns>
    ///     A <see cref="ApplicationTheme"/> value that represents the current system theme.
    /// </returns>
    private ApplicationTheme GetSystemTheme()
    {
        try
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");

            if (key is not null)
            {
                int value = (int)key.GetValue("AppsUseLightTheme", 1);

                return value is 0 ? ApplicationTheme.Dark : ApplicationTheme.Light;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ServiceName}: An unexpected error occurred while reading the system theme.", nameof(ThemeService));
        }

        return ApplicationTheme.Light;
    }

}

namespace ModernUI.Abstractions.Contracts.Models;

/// <summary>
///     Specifies the contract for navigation-aware view-models.
/// </summary>
public interface INavigationAware
{

    /// <summary>
    ///     Invoked by the navigation service before the view-model is navigated to.
    /// </summary>
    /// <param name="parameter">
    ///     Optional parameter that can be used to pass information to the view-model being navigated to.
    /// </param>
    void OnNavigatingTo(object parameter);

    /// <summary>
    ///     Invoked by the navigation service after the view-model is navigated away from.
    /// </summary>
    void OnNavigatedFrom();
}

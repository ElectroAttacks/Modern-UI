namespace ModernUI.Abstractions.Contracts.Models;

/// <summary>
///     Specifies the contract for navigable views.
/// </summary>
/// <typeparam name="TViewModel">
///     The type of the view-model associated with the view.
/// </typeparam>
public interface INavigableView<TViewModel>
{

    /// <summary>
    ///     Gets the view-model associated with the view.
    /// </summary>
    TViewModel ViewModel { get; }
}

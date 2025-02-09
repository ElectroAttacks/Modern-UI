using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

using ModernUI.Hosting.Contracts.Models;

namespace ModernUI.Hosting.Models;

/// <summary>
///     Represents a setting that notifies when its value changes and can be serialized.
/// </summary>
/// <typeparam name="T">
///     The type of value the setting holds.
/// </typeparam>
[Serializable]
public sealed class LocalSetting<T> : INotifyPropertyChanging, INotifyPropertyChanged, ILocalSetting
{

    #region Fields & Properties

    /// <summary>
    ///     Gets or sets the value of the setting.
    /// </summary>
    [AllowNull]
    [field: AllowNull]
    public T Value
    {
        get;

        set
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return;

            OnPropertyChanging();

            field = value;

            OnPropertyChanged();
        }
    }

    /// <inheritdoc />
    [JsonIgnore]
    bool ILocalSetting.IsDefaultValue => EqualityComparer<T>.Default.Equals(Value, default);

    #endregion


    #region Events & Delegates

    /// <inheritdoc />
    public event PropertyChangingEventHandler? PropertyChanging;

    /// <summary>
    ///     Raises the <see cref="PropertyChanging" /> event.
    /// </summary>
    /// <param name="propertyName">
    ///     The name of the property that is changing.
    /// </param>
    private void OnPropertyChanging([CallerMemberName] string propertyName = "")
        => PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));


    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    ///     Raises the <see cref="PropertyChanged" /> event.
    /// </summary>
    /// <param name="propertyName">
    ///     The name of the property that has changed.
    /// </param>
    private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    #endregion


    #region Constructors

    /// <summary>
    ///     Initializes a new instance of the <see cref="LocalSetting{T}" /> class.
    /// </summary>
    /// <param name="value">
    ///     The value to initialize the setting with.
    /// </param>
    [JsonConstructor]
    public LocalSetting(T? value = default)
    {
        Value = value;
    }

    #endregion


    /// <summary>
    ///     Adds the specified action to the <see cref="PropertyChanged" /> event and invokes it immediately.
    /// </summary>
    /// <param name="action">
    ///     The action to add to the <see cref="PropertyChanged" /> event and invoke immediately.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when the specified action is <see langword="null" />.
    /// </exception>
    public void SubscribeAndExecute(Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        PropertyChanged += (_, _) => action(Value);
        action(Value);
    }


    /// <summary>
    ///     Implicitly converts a setting to its value.
    /// </summary>
    /// <param name="setting">
    ///     The setting to retrieve the value from.
    /// </param>
    [return: MaybeNull]
    public static implicit operator T(LocalSetting<T> setting) => setting.Value;

}

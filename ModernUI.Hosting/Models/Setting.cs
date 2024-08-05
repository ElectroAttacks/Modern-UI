using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
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
public record Setting<T> : INotifyPropertyChanging, INotifyPropertyChanged, ISetting
{

    [AllowNull]
    private T _value;

    /// <summary>
    ///     Gets or sets the value of the setting.
    /// </summary>
    public T Value
    {
        get
        {
            return _value;
        }
        set
        {
            if (EqualityComparer<T>.Default.Equals(_value, value)) return;

            PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(nameof(Value)));

            _value = value;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
        }
    }

    /// <inheritdoc />
    public bool IsRegistered { get; private set; } = true;

    /// <inheritdoc />
    bool ISetting.IsDefaultValue
    {
        get
        {
            return EqualityComparer<T>.Default.Equals(_value, default!);
        }
    }


    /// <inheritdoc />
    public event PropertyChangingEventHandler? PropertyChanging;

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;



    /// <summary>
    ///     Initializes a new instance of the <see cref="Setting{T}" /> class.
    /// </summary>
    /// <param name="initialValue">
    ///     The value to initialize the setting with.
    /// </param>
    [JsonConstructor]
    public Setting(T? initialValue = default)
    {
        _value = initialValue;
    }



    /// <summary>
    ///     Adds the specified action to the <see cref="PropertyChanged" /> event and invokes it immediately.
    /// </summary>
    /// <param name="action">
    ///     The action to add to the <see cref="PropertyChanged" /> event and invoke immediately.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when the specified action is <see langword="null" />.
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining), Pure]
    public void SubscribeAndInvoke(Action action)
    {
        if (action is null) throw new ArgumentNullException(nameof(action));

        PropertyChanged += (_, _) => action();
        action();
    }


    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining), Pure]
    public override string ToString() => Value?.ToString() ?? string.Empty;


    /// <summary>
    ///     Implicitly converts a setting to its value.
    /// </summary>
    /// <param name="setting">
    ///     The setting to retrieve the value from.
    /// </param>
    [return: MaybeNull]
    [MethodImpl(MethodImplOptions.AggressiveInlining), Pure]
    public static implicit operator T(Setting<T> setting) => setting.Value;


    /// <summary>
    ///     Gets a setting that is not registered in the cache.
    /// </summary>
    public static Setting<T> Unregistered => new() { IsRegistered = false };
}
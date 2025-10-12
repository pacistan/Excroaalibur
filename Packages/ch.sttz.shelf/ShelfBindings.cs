/* --------------------------------------------------------
 * The Shelf v3
 * --------------------------------------------------------
 * Use of this script is subject to the Unity Asset Store
 * End User License Agreement:
 *
 * https://unity.com/legal/as-terms
 *
 * Use of this script for any other purpose than outlined
 * in the EULA linked above is prohibited.
 * --------------------------------------------------------
 * © 2024 Adrian Stutz (adrian@sttz.ch)
 */

using System;
using UnityEngine.UIElements;

#if !UNITY_2023_2_OR_NEWER
using sttz.TheShelf.Backwards;
#endif

namespace sttz.TheShelf {

/// <summary>
/// Simple helper class to work around the binding system
/// not being available before Unity 2023.2.
/// </summary>
public static class ShelfBindings
{
    /// <summary>
    /// The unbinder delegate to call to undo the binding.
    /// </summary>
    public delegate void Unbinder();

    /// <summary>
    /// Generic binding method that registers with a <see cref="INotifyBindablePropertyChanged"/>
    /// and calls the <paramref name="updater"/> delegate immediately and wether the
    /// source triggers a change of the specified property.
    /// </summary>
    /// <typeparam name="TSource">The type of the source to bind to, must implement <see cref="INotifyBindablePropertyChanged"/></typeparam>
    /// <param name="source">The source to bind to</param>
    /// <param name="propertyName">The name of the property on the source</param>
    /// <param name="updater">The method called wether the property changes</param>
    /// <returns>The unbinding delegate</returns>
    public static Unbinder Bind<TSource>(TSource source, string propertyName, Action<TSource> updater) 
        where TSource : INotifyBindablePropertyChanged
    {
        EventHandler<BindablePropertyChangedEventArgs> handler = (s, e) => {
            if (e.propertyName == propertyName) {
                updater(source);
            }
        };
        source.propertyChanged += handler;
        updater(source);
        return () => source.propertyChanged -= handler;
    }

    /// <summary>
    /// Generic binding method, like <see cref="Bind"/>, but stores the <see cref="Unbinder"/>
    /// on the <paramref name="visualElement"/>'s <see cref="VisualElement.userData"/> field.
    /// </summary>
    /// <remarks>
    /// Call <see cref="UnbindWithUserData"/> on the visual element
    /// to undo the binding.
    /// </remarks>
    /// <typeparam name="TSource">The type of the source to bind to, must implement <see cref="INotifyBindablePropertyChanged"/></typeparam>
    /// <param name="visualElement">The visual element to set the `userData` on</param>
    /// <param name="source">The source to bind to</param>
    /// <param name="propertyName">The name of the property on the source</param>
    /// <param name="updater">The method called wether the property changes</param>
    public static void BindWithUserData<TSource>(this VisualElement visualElement, TSource source, string propertyName, Action<TSource> updater)
        where TSource : INotifyBindablePropertyChanged
    {
        visualElement.userData = Bind(source, propertyName, updater);
    }

    /// <summary>
    /// Binding method for <see cref="BaseField{TValueType}"/>.
    /// In contrast to <see cref="BindWithUserData{TSource}"/>, this method
    /// automatically binds to the field's value. Instead of an updater
    /// delegate, this method therefore only requires a <paramref name="fieldGetter"/>.
    /// </summary>
    /// <remarks>
    /// Call <see cref="UnbindWithUserData"/> on the visual element
    /// to undo the binding.
    /// </remarks>
    /// <typeparam name="TSource">The type of the source to bind to, must implement <see cref="INotifyBindablePropertyChanged"/></typeparam>
    /// <typeparam name="TProperty">The type of the field property (the <paramref name="fieldGetter"/> can convert)</typeparam>
    /// <param name="field">The field to bind</param>
    /// <param name="source">The source to bind to</param>
    /// <param name="propertyName">The name of the property on the source</param>
    /// <param name="fieldGetter">The delegate to read the property on the source</param>
    public static void BindWithUserData<TSource, TProperty>(
        this BaseField<TProperty> field,
        TSource source,
        string propertyName,
        Func<TSource, TProperty> fieldGetter
    )
        where TSource : INotifyBindablePropertyChanged
    {
        // Update field from source change
        EventHandler<BindablePropertyChangedEventArgs> targetHandler = (s, e) => {
            if (e.propertyName == propertyName) {
                field.SetValueWithoutNotify(fieldGetter(source));
            }
        };
        source.propertyChanged += targetHandler;
        field.SetValueWithoutNotify(fieldGetter(source));

        field.userData = new Unbinder(() => source.propertyChanged -= targetHandler);
    }

    /// <summary>
    /// Two-way binding method for <see cref="BaseField{TValueType}"/>.
    /// This binding syncs changes from the source to the field and back.
    /// </summary>
    /// <typeparam name="TSource">The type of the source to bind to, must implement <see cref="INotifyBindablePropertyChanged"/></typeparam>
    /// <typeparam name="TProperty">The type of the field property (the <paramref name="fieldGetter"/> can convert)</typeparam>
    /// <param name="field">The field to bind</param>
    /// <param name="source">The source to bind to</param>
    /// <param name="propertyName">The name of the property on the source</param>
    /// <param name="fieldGetter">The delegate to read the property on the source</param>
    /// <param name="fieldSetter">The delegate to set the property on the source</param>
    public static void BindTwoWayWithUserData<TSource, TProperty>(
        this BaseField<TProperty> field,
        TSource source,
        string propertyName,
        Func<TSource, TProperty> fieldGetter,
        Action<TSource, TProperty> fieldSetter
    )
        where TSource : INotifyBindablePropertyChanged
    {
        // Update field from source change
        EventHandler<BindablePropertyChangedEventArgs> targetHandler = (s, e) => {
            if (e.propertyName == propertyName) {
                field.SetValueWithoutNotify(fieldGetter(source));
            }
        };
        source.propertyChanged += targetHandler;
        field.SetValueWithoutNotify(fieldGetter(source));

        // Update source from field change
        EventCallback<ChangeEvent<TProperty>> sourceHandler = e => {
            fieldSetter(source, e.newValue);
        };
        field.RegisterValueChangedCallback(sourceHandler);

        // Set unbinder
        field.userData = new Unbinder(() => {
            source.propertyChanged -= targetHandler;
            field.UnregisterValueChangedCallback(sourceHandler);
        });
    }

    /// <summary>
    /// Helper to call the unbinding delegate when stored in <see cref="VisualElement.userData"/>.
    /// </summary>
    /// <remarks>
    /// Checks if <see cref="VisualElement.userData"/> is a <see cref="Unbinder"/>, calls it
    /// and sets `userData` to `null`.
    /// </remarks>
    public static void UnbindWithUserData(this VisualElement visualElement)
    {
        if (visualElement.userData is Unbinder unbinder) {
            unbinder();
            visualElement.userData = null;
        }
    }
}

}

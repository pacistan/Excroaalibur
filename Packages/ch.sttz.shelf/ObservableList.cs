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
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;

#if HAS_UNITY_PROPERTIES || UNITY_2022_3_OR_NEWER
using Unity.Properties;
#endif

#if !UNITY_2023_2_OR_NEWER
using sttz.TheShelf.Backwards;
#endif

namespace sttz.TheShelf {

/// <summary>
/// Companion to <see cref="INotifyBindablePropertyChanged"/> but triggered
/// before the property actually changes, useful for undo handling.
/// </summary>
public interface INotifyBindablePropertyWillChange
{
    event EventHandler<BindablePropertyChangedEventArgs> propertyWillChange;
}

/// <summary>
/// Wrapper around a <see cref="List{T}"/> that raises <see cref="OnListChanged"/>
/// whenever the list items are changed.
/// </summary>
/// <remarks>
/// This is used to bind to <see cref="ListView"/> and being able to track
/// changes made in the UI.
/// </remarks>
public class ObservableList<T> : IList<T>, IList
{
    // ---------- API ----------

    /// <summary>
    /// Signature of the <see cref="OnListChanged"/> event.
    /// </summary>
    /// <param name="index">The index that was changed or `-1` if the whole list was changed (replaced or cleared)</param>
    /// <param name="oldValue">The old value or `default` if not applicable (e.g. new item added)</param>
    /// <param name="newValue">The new value or `default` if not applicable (e.g. item removed)</param>
    public delegate void ListChangeEventHandler(ObservableList<T> wrapper, int index, T oldValue, T newValue);

    /// <summary>
    /// Event triggered right before the list will be changed through this wrapper.
    /// </summary>
    public event ListChangeEventHandler OnBeforeListChange;

    /// <summary>
    /// Event triggered when the list has been changed through this wrapper.
    /// </summary>
    public event ListChangeEventHandler OnListChanged;

    /// <summary>
    /// The source list this wrapper tracks changes of.
    /// Changing this property does not trigger the <see cref="OnListChanged"/> event.
    /// </summary>
    public IList<T> SourceList {
        get => _sourceList;
        set => _sourceList = value;
    }

    /// <summary>
    /// Create a new wrapper without a list,
    /// <see cref="SourceList"/> needs to be assigned before the 
    /// wrapper can be used.
    /// </summary>
    public ObservableList() {}

    /// <summary>
    /// Create a new wrapper with the given list.
    /// </summary>
    public ObservableList(IList<T> sourceList)
    {
        _sourceList = sourceList;
    }

    // ---------- IList ----------

    [CreateProperty]
    public T this[int index] { 
        get => _sourceList[index];
        set {
            var oldItem = _sourceList[index];
            OnBeforeListChange?.Invoke(this, index, oldItem, value);
            _sourceList[index] = value;
            OnListChanged?.Invoke(this, index, oldItem, value);
        }
    }

    object IList.this[int index] {
        get => _sourceList[index];
        set {
            var typedValue = CheckType(value);
            var oldItem = _sourceList[index];
            OnBeforeListChange?.Invoke(this, index, oldItem, typedValue);
            _sourceList[index] = typedValue;
            OnListChanged?.Invoke(this, index, oldItem, typedValue);
        }
    }

    public int Count => _sourceList.Count;

    public bool IsReadOnly => _sourceList.IsReadOnly;

    public bool IsFixedSize => (_sourceList as IList).IsFixedSize;

    public bool IsSynchronized => (_sourceList as IList).IsSynchronized;

    public object SyncRoot => (_sourceList as IList).SyncRoot;

    public void Add(T item)
    {
        OnBeforeListChange?.Invoke(this, _sourceList.Count, default, item);
        _sourceList.Add(item);
        OnListChanged?.Invoke(this, _sourceList.Count, default, item);
    }

    public void Clear()
    {
        OnBeforeListChange?.Invoke(this, -1, default, default);
        _sourceList.Clear();
        OnListChanged?.Invoke(this, -1, default, default);
    }

    public bool Contains(T item)
    {
        return _sourceList.Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        _sourceList.CopyTo(array, arrayIndex);
    }

    public int IndexOf(T item)
    {
        return _sourceList.IndexOf(item);
    }

    public void Insert(int index, T item)
    {
        OnBeforeListChange?.Invoke(this, index, default, item);
        _sourceList.Insert(index, item);
        OnListChanged?.Invoke(this, index, default, item);
    }

    public bool Remove(T item)
    {
        var index = _sourceList.IndexOf(item);
        if (index < 0) return false;

        RemoveAt(index);
        return true;
    }

    public void RemoveAt(int index)
    {
        var item = _sourceList[index];
        OnBeforeListChange?.Invoke(this, index, item, default);
        _sourceList.RemoveAt(index);
        OnListChanged?.Invoke(this, index, item, default);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _sourceList.GetEnumerator();
    }

    public IEnumerator<T> GetEnumerator()
    {
        return _sourceList.GetEnumerator();
    }

    public int Add(object value)
    {
        var typedValue = CheckType(value);
        OnBeforeListChange?.Invoke(this, _sourceList.Count, default, typedValue);
        _sourceList.Add(typedValue);
        OnListChanged?.Invoke(this, _sourceList.Count, default, typedValue);
        return _sourceList.Count;
    }

    public bool Contains(object value)
    {
        var typedValue = CheckType(value);
        return _sourceList.Contains(typedValue);
    }

    public int IndexOf(object value)
    {
        var typedValue = CheckType(value);
        return _sourceList.IndexOf(typedValue);
    }

    public void Insert(int index, object value)
    {
        var typedValue = CheckType(value);
        OnBeforeListChange?.Invoke(this, index, default, typedValue);
        _sourceList.Insert(index, typedValue);
        OnListChanged?.Invoke(this, index, default, typedValue);
    }

    public void Remove(object value)
    {
        var typedValue = CheckType(value);

        var index = _sourceList.IndexOf(typedValue);
        if (index < 0) return;

        RemoveAt(index);
    }

    public void CopyTo(Array array, int index)
    {
        (_sourceList as IList).CopyTo(array, index);
    }

    // ---------- Implementation ----------

#if HAS_UNITY_PROPERTIES || UNITY_2022_3_OR_NEWER
    static ObservableList()
    {
        PropertyBag.Register(new ObservableListPropertyBag<T>());
    }
#endif

    IList<T> _sourceList;

    T CheckType(object value)
    {
        if (value is not T typedValue)
            throw new ArgumentException($"Invalid item type, got '{value.GetType().Name}' but require '{typeof(T).Name}'");
        return typedValue;
    }
}

#if HAS_UNITY_PROPERTIES || UNITY_2022_3_OR_NEWER

/// <summary>
/// Custom property bag to be able to bind to items in <see cref="ObservableList{T}"/>.
/// </summary>
/// <remarks>
/// It seems Unity's binding system should be able to natively bind to <see cref="IList"/>
/// but it kept throwing exceptions for me, this is a workaround.
/// Based on https://github.com/needle-mirror/com.unity.serialization/blob/f204ef9af220038a2822928ff4ba1f341df9a7b1/Runtime/Unity.Serialization/Json/Views/Internal/SerializedArrayViewPropertyBag.cs
/// </remarks>
class ObservableListPropertyBag<T> : PropertyBag<ObservableList<T>>, IListPropertyBag<ObservableList<T>, T>
{
    static readonly Property k_Property = new Property();

    public override PropertyCollection<ObservableList<T>> GetProperties()
        => PropertyCollection<ObservableList<T>>.Empty;
    
    public override PropertyCollection<ObservableList<T>> GetProperties(ref ObservableList<T> collectionContainer)
        => PropertyCollection<ObservableList<T>>.Empty;

    class Property : Property<ObservableList<T>, T>, IListElementProperty
    {
        // ReSharper disable once InconsistentNaming
        internal int m_Index;
        
        // ReSharper disable once InconsistentNaming
        internal ObservableList<T> m_Collection;
        
        /// <inheritdoc/>
        public int Index => m_Index;

        /// <inheritdoc/>
        public override string Name => m_Index.ToString();
        
        /// <inheritdoc/>
        public override bool IsReadOnly => m_Collection.IsReadOnly;

        public override T GetValue(ref ObservableList<T> container)
            => container[m_Index];

        public override void SetValue(ref ObservableList<T> container, T value)
            => container[m_Index] = value;
    }

    struct Enumerator : IEnumerator<IProperty<ObservableList<T>>>
    {
        int m_Index;

        ObservableList<T> m_Container;

        public Enumerator(ObservableList<T> container)
        {
            m_Index = -1;
            m_Container = container;
        }

        public bool MoveNext()
        {
            m_Index++;
            return m_Index >= m_Container.Count;
        }

        public void Reset()
        {
            m_Index = -1;
        }

        public void Dispose()
        {
            m_Container = null;
        }

        object IEnumerator.Current => Current;

        public IProperty<ObservableList<T>> Current
        {
            get {
                k_Property.m_Index = m_Index;
                k_Property.m_Collection = m_Container;
                return k_Property;
            }
        }
    }

    readonly struct Enumerable : IEnumerable<IProperty<ObservableList<T>>>
    {
        readonly ObservableList<T> m_Container;

        public Enumerable(ObservableList<T> container) 
            => m_Container = container;

        public IEnumerator<IProperty<ObservableList<T>>> GetEnumerator()
            => new Enumerator(m_Container);

        IEnumerator IEnumerable.GetEnumerator()
            => new Enumerator(m_Container);
    }

    public bool TryGetProperty(ref ObservableList<T> container, int index, out IProperty<ObservableList<T>> property)
    {
        if (index < 0 || index >= container.Count) {
            property = default;
            return false;
        }

        property = new Property() { m_Index = index, m_Collection = container };
        return true;
    }

    public void Accept(ICollectionPropertyBagVisitor visitor, ref ObservableList<T> container)
    {
        visitor.Visit(this, ref container);
    }

    public void Accept(IListPropertyBagVisitor visitor, ref ObservableList<T> container)
    {
        visitor.Visit(this, ref container);
    }

    public void Accept<TContainer>(IListPropertyVisitor visitor, Property<TContainer, ObservableList<T>> property, ref TContainer container, ref ObservableList<T> list)
    {
        visitor.Visit<TContainer, ObservableList<T>, T>(property, ref container, ref list);
    }
}

#endif

}

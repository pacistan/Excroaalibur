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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

#if HAS_UNITY_PROPERTIES || UNITY_2022_3_OR_NEWER
using Unity.Properties;
#endif

#if !UNITY_2023_2_OR_NEWER
using sttz.TheShelf.Backwards;
#endif

using UObject = UnityEngine.Object;

namespace sttz.TheShelf {

/// <summary>
/// The shelf data scriptable object.
/// Contains many <see cref="Shelves"/> that hold project references.
/// </summary>
/// <remarks>
/// Shelf racks can be created as regular assets inside
/// the `Assets` folder. There's also a local "User Rack"
/// that lives inside the `UserSettings` folder, which is
/// intended to be excluded from version control.
/// </remarks>
[CreateAssetMenu(menuName = "The Shelf/Rack", order = 0)]
public class ShelfRack : ScriptableObject, INotifyBindablePropertyChanged, INotifyBindablePropertyWillChange
{
    // ---------- Shelf ----------

    /// <summary>
    /// A single shelf on a <see cref="ShelfRack"/>,
    /// containing a list of project references.
    /// </summary>
    [Serializable]
    public class Shelf : INotifyBindablePropertyChanged, INotifyBindablePropertyWillChange
    {
        // ---------- API ----------

        /// <summary>
        /// The name of the shelf.
        /// </summary>
        [CreateProperty]
        public string Name
        {
            get => name;
            set {
                if (name == value) return;
                NotifyWillChange();
                name = value;
                NotifyHasChanged();
            }
        }

        /// <summary>
        /// The icon shown in the shelf header.
        /// Must be one of `Texture2D`, `RenderTexture`, `Sprite` or `VectorImage`.
        /// </summary>
        [CreateProperty]
        public UObject Icon
        {
            get => icon;
            set {
                if (icon == value) return;
                NotifyWillChange();
                icon = value;
                NotifyHasChanged();
            }
        }

        /// <summary>
        /// The text color of the shelf tab header.
        /// </summary>
        /// <remarks>
        /// If the red component of the color is negative,
        /// the default color will be used.
        /// </remarks>
        [CreateProperty]
        public Color HeaderTextColor
        {
            get => headerTextColor;
            set {
                if (headerTextColor == value) return;
                NotifyWillChange();
                headerTextColor = value;
                NotifyHasChanged();
            }
        }

        /// <summary>
        /// The background color of the shelf tab header.
        /// </summary>
        /// <remarks>
        /// If the red component of the color is negative,
        /// the default color will be used.
        /// </remarks>
        [CreateProperty]
        public Color HeaderBackgroundColor
        {
            get => headerBackgroundColor;
            set {
                if (headerBackgroundColor == value) return;
                NotifyWillChange();
                headerBackgroundColor = value;
                NotifyHasChanged();
            }
        }

        /// <summary>
        /// The project references on the shelf.
        /// </summary>
        /// <remarks>
        /// The property can only be set to a `List<T>` instance,
        /// setting it to another `IList` implementation will raise an exception.
        /// This is due to Unity only being able to serialize `List<T>` by default.
        /// This property returns an <see cref="ObservableList{T}"/> wrapper to
        /// track changes to the references.
        /// </remarks>
        [CreateProperty]
        public IList<UObject> Items
        {
            get {
                if (items == null) return null;

                if (observableItems == null) {
                    observableItems = new();
                    observableItems.OnBeforeListChange += OnBeforeItemsChanged;
                    observableItems.OnListChanged += OnItemsChanged;
                }
                if (observableItems.SourceList != items) {
                    observableItems.SourceList = items;
                }
                return observableItems;
            }
            set {
                if (value is not List<UObject> itemsList)
                    throw new ArgumentException($"Items can only be set to a List<UnityEngine.Object>");

                if (items == itemsList) return;

                NotifyWillChange();

                items = itemsList;

                if (observableItems != null) {
                    observableItems.SourceList = itemsList;
                }

                NotifyHasChanged();
            }
        }

        /// <summary>
        /// The display size for items on this shelf.
        /// </summary>
        /// <remarks>
        /// When set to <see cref="ShelfItemSize.Default"/>,
        /// <see cref="ShelfSettings.ItemSize"/> will be used.
        /// </remarks>
        [CreateProperty]
        public ShelfItemSize ItemSize {
            get => itemSize;
            set {
                if (itemSize == value) return;
                NotifyWillChange();
                itemSize = value;
                NotifyHasChanged();
            }
        }

        /// <summary>
        /// Create a new shelf instance with empty defaults.
        /// </summary>
        public Shelf() : base() { }

        /// <summary>
        /// Create a new shelf with the given name, 
        /// optionally fill it with the given items.
        /// </summary>
        public Shelf(string name, IEnumerable<UObject> items = null) : base()
        {
            Name = name;

            this.items = new();

            if (items != null) {
                InsertItems(0, items);
            }
        }

        /// <summary>
        /// Insert a shelf item at the given index.
        /// </summary>
        /// <remarks>
        /// This method should be used instead of directly
        /// inserting items into the <see cref="Items"/> list,
        /// as this will run the shelf item processors to properly
        /// store scene or prefab objects.
        /// </remarks>
        public void InsertItem(int index, UObject item)
        {
            item = ShelfApi.ProcessShelfItem(item);
            if (item == null) return;

            Items.Insert(index, item);
        }

        /// <summary>
        /// Insert shelf items at the given index.
        /// </summary>
        /// <remarks>
        /// This method should be used instead of directly
        /// inserting items into the <see cref="Items"/> list,
        /// as this will run the shelf item processors to properly
        /// store scene or prefab objects.
        /// </remarks>
        public void InsertItems(int index, IEnumerable<UObject> items)
        {
            var insertIndex = index;
            foreach (var item in ShelfApi.ProcessShelfItems(items)) {
                Items.Insert(insertIndex++, item);
            }
        }

        /// <summary>
        /// Create a clone copy of this shelf.
        /// </summary>
        public Shelf Clone()
        {
            return new Shelf() {
                Name = Name,
                ItemSize = ItemSize,
                items = new List<UObject>(ShelfApi.ProcessShelfItems(items))
            };
        }

        // ---------- INotifyBindablePropertyChanged ----------

        public event EventHandler<BindablePropertyChangedEventArgs> propertyWillChange;

        void NotifyWillChange([CallerMemberName] string property = "")
        {
            propertyWillChange?.Invoke(this, new BindablePropertyChangedEventArgs(property));
        }

        // ---------- INotifyBindablePropertyChanged ----------

        public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;

        void NotifyHasChanged([CallerMemberName] string property = "")
        {
            propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(property));
        }

        // ---------- Internals ----------

        [SerializeField, DontCreateProperty] string name;
        [SerializeField, DontCreateProperty] UObject icon;
        [SerializeField, DontCreateProperty] List<UObject> items;
        [SerializeField, DontCreateProperty] ShelfItemSize itemSize;
        [SerializeField, DontCreateProperty] Color headerTextColor = new Color(-1, -1, -1, 1);
        [SerializeField, DontCreateProperty] Color headerBackgroundColor = new Color(-1, -1, -1, 1);
        ObservableList<UObject> observableItems;

        void OnBeforeItemsChanged(ObservableList<UObject> sender, int index, UObject removed, UObject added)
        {
            NotifyWillChange(nameof(Items));
        }

        void OnItemsChanged(ObservableList<UObject> sender, int index, UObject removed, UObject added)
        {
            NotifyHasChanged(nameof(Items));
        }

        public void OnUndoRedo()
        {
            NotifyHasChanged(nameof(Name));
            NotifyHasChanged(nameof(Items));
            NotifyHasChanged(nameof(Icon));
            NotifyHasChanged(nameof(HeaderTextColor));
            NotifyHasChanged(nameof(HeaderBackgroundColor));
            NotifyHasChanged(nameof(ItemSize));
        }
    }

    // ---------- Rack ----------

    /// <summary>
    /// The shelves on this rack.
    /// </summary>
    /// <remarks>
    /// The property can only be set to a `List<T>` instance,
    /// setting it to another `IList` implementation will raise an exception.
    /// This is due to Unity only being able to serialize `List<T>` by default.
    /// This property returns an <see cref="ObservableList{T}"/> wrapper to
    /// track changes to the shelves.
    /// </remarks>
    [CreateProperty]
    public IList<Shelf> Shelves
    {
        get {
            if (shelves == null) return null;

            if (observableShelves == null) {
                observableShelves = new();
                observableShelves.OnBeforeListChange += OnShelvesWillChange;
                observableShelves.OnListChanged += OnShelvesChanged;
            }
            if (observableShelves.SourceList != shelves) {
                observableShelves.SourceList = shelves;
            }
            return observableShelves;
        }
        set {
            if (value is not List<Shelf> shelvesList)
                throw new ArgumentException($"Shelves can only be set to a List<Shelves>");

            if (shelves == shelvesList) return;

            NotifyWillChange();

            shelves = shelvesList;

            if (observableShelves != null) {
                observableShelves.SourceList = shelvesList;
            }

            NotifyHasChanged();
        }
    }

    /// <summary>
    /// Event triggered when the rack or any of its shelves has changed.
    /// </summary>
    public event Action<ShelfRack> OnRackChanged;
    /// <summary>
    /// Event triggered when the rack's `OnDisable` method is called.
    /// </summary>
    public event Action<ShelfRack> OnRackUnloading;

    /// <summary>
    /// Get the objects that need to be persisted when saving this rack.
    /// </summary>
    /// <remarks>
    /// The to be persisted objects are non-asset objects on shelves,
    /// that need to be saved together with the rack. Either in the
    /// asset inside the `Assets` folder or in the asset in `UserSettings`.
    /// These objects need to implement <see cref="IShelfItem"/>
    /// and must <see cref="IShelfItem.SaveInRackAsset"/> return `true`.
    /// </remarks>
    /// <param name="target">The list to add the objects to</param>
    public void GetObjectsToPersist(ICollection<UObject> target)
    {
        if (Shelves == null) return;

        foreach (var shelf in Shelves) {
            if (shelf?.Items == null) continue;
            foreach (var item in shelf.Items) {
                if (item is not IShelfItem shelfItem) continue;
                if (!shelfItem.SaveInRackAsset) continue;
                target.Add(item);
            }
        }
    }

    // ---------- INotifyBindablePropertyWillChange ----------

    public event EventHandler<BindablePropertyChangedEventArgs> propertyWillChange;

    void NotifyWillChange([CallerMemberName] string property = "")
    {
        propertyWillChange?.Invoke(this, new BindablePropertyChangedEventArgs(property));
    }

    // ---------- INotifyBindablePropertyChanged ----------

    public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;

    void NotifyHasChanged([CallerMemberName] string property = "")
    {
        propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(property));
    }

    // ---------- Internals ----------

    void OnEnable()
    {
        Undo.undoRedoPerformed += OnUndoRedo;

        if (shelves == null) {
            shelves = new();
        }

        foreach (var shelf in shelves) {
            if (shelf == null) continue;
            shelf.propertyWillChange += OnShelfPropertyWillChange;
            shelf.propertyChanged += OnShelfPropertyHasChanged;
        }
    }

    void Reset()
    {
        if (shelves == null || shelves.Count == 0) {
            // Add default shelves when rack is created or reset
            shelves ??= new();
            Shelves.Add(new Shelf("First"));
            Shelves.Add(new Shelf("Second"));
            Shelves.Add(new Shelf("Third"));
        }
    }

    void OnDisable()
    {
        Undo.undoRedoPerformed -= OnUndoRedo;

        if (shelves != null) {
            foreach (var shelf in shelves) {
                if (shelf == null) continue;
                shelf.propertyWillChange -= OnShelfPropertyWillChange;
                shelf.propertyChanged -= OnShelfPropertyHasChanged;
            }
        }

        OnRackUnloading?.Invoke(this);
    }

    void OnShelvesWillChange(ObservableList<Shelf> sender, int index, Shelf removed, Shelf added)
    {
        NotifyWillChange(nameof(Shelves));
        RecordUndo($"Rack {name} shelves changed");
    }

    void OnShelvesChanged(ObservableList<Shelf> sender, int index, Shelf removed, Shelf added)
    {
        NotifyHasChanged(nameof(Shelves));

        if (removed != null) {
            removed.propertyWillChange -= OnShelfPropertyWillChange;
            removed.propertyChanged -= OnShelfPropertyHasChanged;
        }
        if (added != null) {
            added.propertyWillChange += OnShelfPropertyWillChange;
            added.propertyChanged += OnShelfPropertyHasChanged;
        }

        OnRackChanged?.Invoke(this);

        EditorUtility.SetDirty(this);
    }

    void OnShelfPropertyWillChange(object sender, BindablePropertyChangedEventArgs args)
    {
        RecordUndo($"Shelf {name}/{((Shelf)sender).Name} changed");
    }

    void OnShelfPropertyHasChanged(object sender, BindablePropertyChangedEventArgs args)
    {
        OnRackChanged?.Invoke(this);

        if (args.propertyName == nameof(Shelf.Items)) {
            PersistItems();
        }

        EditorUtility.SetDirty(this);
    }

    void RecordUndo(string reason)
    {
        Undo.RecordObject(this, reason);
    }

    void OnUndoRedo()
    {
        NotifyHasChanged(nameof(Shelves));

        if (shelves != null) {
            foreach (var shelf in shelves) {
                if (shelf == null) continue;
                shelf.OnUndoRedo();

                // Re-register event handlers in case the underlying data changed
                shelf.propertyWillChange -= OnShelfPropertyWillChange;
                shelf.propertyWillChange += OnShelfPropertyWillChange;
                shelf.propertyChanged -= OnShelfPropertyHasChanged;
                shelf.propertyChanged += OnShelfPropertyHasChanged;
            }
        }
    }

    static HashSet<UObject> saveSet;

    void PersistItems()
    {
        if (!EditorUtility.IsPersistent(this))
            return;

        var path = AssetDatabase.GetAssetPath(this);
        if (string.IsNullOrEmpty(path))
            return;

        var subAssets = AssetDatabase.LoadAllAssetsAtPath(path);

        saveSet ??= new();
        saveSet.Clear();
        GetObjectsToPersist(saveSet);

        // Remove outdated sub-assets
        for (int i = 0; i < subAssets.Length; i++) {
            var existing = subAssets[i];
            if (existing == this || AssetDatabase.IsMainAsset(existing)) continue;
            if (!saveSet.Contains(existing)) {
                AssetDatabase.RemoveObjectFromAsset(existing);
                subAssets[i] = null;
            }
        }

        // Add new assets
        foreach (var newAsset in saveSet) {
            if (newAsset == null) continue;
            if (!subAssets.Contains(newAsset)) {
                newAsset.hideFlags = HideFlags.HideInHierarchy;
                AssetDatabase.AddObjectToAsset(newAsset, this);
            }
        }

        //AssetDatabase.SaveAssets();

        saveSet.Clear();
    }

    [SerializeField, DontCreateProperty] List<Shelf> shelves;
    ObservableList<Shelf> observableShelves;
}

}

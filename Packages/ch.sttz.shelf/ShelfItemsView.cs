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
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

#if !UNITY_2023_2_OR_NEWER
using sttz.TheShelf.Backwards;
using Button = sttz.TheShelf.Backwards.IconButton;
#endif

using UObject = UnityEngine.Object;

namespace sttz.TheShelf {

/// <summary>
/// Main list view of references on a shelf.
/// </summary>
public class ShelfItemsView : ListView
{
    // ---------- Static API ----------

    /// <summary>
    /// Returns wether a shelf can accept the given references.
    /// </summary>
    public static bool CanAccept(IEnumerable<UObject> references)
    {
        return references != null && references.Any();
    }

    /// <summary>
    /// Gets the source shelf of the drag operation (if any).
    /// </summary>
    public static ShelfItemsView GetDragSourceShelf()
    {
        return DragAndDrop.GetGenericData(ShelfSourceKey) as ShelfItemsView;
    }

#if UNITY_2023_2_OR_NEWER
    /// <summary>
    /// Gets the source shelf of the drag operation (if any).
    /// </summary>
    /// <param name="dragAndDropData">The data to get the source from or null to use <see cref="DragAndDrop"/></param>
    public static ShelfItemsView GetDragSourceShelf(DragAndDropData dragAndDropData = null)
    {
        if (dragAndDropData != null) {
            return dragAndDropData.GetGenericData(ShelfSourceKey) as ShelfItemsView;
        } else {
            return DragAndDrop.GetGenericData(ShelfSourceKey) as ShelfItemsView;
        }
    }
#endif

    /// <summary>
    /// Gets the indices of the items on the source shelf of the drag operation (if any).
    /// </summary>
    public static int[] GetDragSourceShelfIndices()
    {
        return DragAndDrop.GetGenericData(ShelfSourceIndicesKey) as int[];
    }

#if UNITY_2023_2_OR_NEWER
    /// <summary>
    /// Gets the indices of the items on the source shelf of the drag operation (if any).
    /// </summary>
    /// <param name="dragAndDropData">The data to get the source from or null to use <see cref="DragAndDrop"/></param>
    public static int[] GetDragSourceShelfIndices(DragAndDropData dragAndDropData = null)
    {
        if (dragAndDropData != null) {
            return dragAndDropData.GetGenericData(ShelfSourceIndicesKey) as int[];
        } else {
            return DragAndDrop.GetGenericData(ShelfSourceIndicesKey) as int[];
        }
    }
#endif

    /// <summary>
    /// Wether the drag operation moves or copies the references.
    /// </summary>
    /// <param name="target">Target shelf items view</param>
    /// <param name="dragAndDropData">The data to get the source from or null to use <see cref="DragAndDrop"/></param>
    public static bool IsMove(ShelfItemsView target)
    {
        return IsMove(GetDragSourceShelf(), target);
    }

#if UNITY_2023_2_OR_NEWER
    /// <summary>
    /// Wether the drag operation moves or copies the references.
    /// </summary>
    /// <param name="target">Target shelf items view</param>
    /// <param name="dragAndDropData">The data to get the source from or null to use <see cref="DragAndDrop"/></param>
    public static bool IsMove(ShelfItemsView target, DragAndDropData dragAndDropData = null)
    {
        var source = GetDragSourceShelf(dragAndDropData);
        return IsMove(source, target);
    }
#endif

    /// <summary>
    /// Wether the drag operation moves or copies the references.
    /// </summary>
    /// <param name="source">The target shelf items view</param>
    /// <param name="target">The source shelf items view</param>
    public static bool IsMove(ShelfItemsView source, ShelfItemsView target)
    {
        return source != null && (source == target || Event.current?.alt != true);
    }

    /// <summary>
    /// Get the unity object reference to use for dragging,
    /// unpacking <see cref="IShelfItem"/> where necessary.
    /// </summary>
    public static UObject GetDragReference(UObject item)
    {
        if (item is IShelfItem shelfItem) {
            return shelfItem.Reference;
        }

        return item;
    }

    /// <summary>
    /// Get the title to use for the drag operation of the given
    /// shelf items.
    /// </summary>
    /// <param name="items">The shelf items, not the drag references</param>
    public static string GetDragTitle(IEnumerable<UObject> items)
    {
        var title = string.Empty;

        var itemCount = 0;
        var nullReferences = 0;
        foreach (var item in items) {
            itemCount++;

            if (item is IShelfItem shelfItem) {
                var reference = shelfItem.Reference;
                if (reference != null) {
                    title = reference.name;
                } else {
                    title = shelfItem.Name;
                    nullReferences++;
                }
            } else if (item != null) {
                title = item.name;
            }
        }

        if (itemCount > 1)
            title = "<Multiple>";

        if (nullReferences > 0) {
            if (nullReferences == itemCount) {
                title += $" (unloaded item{(nullReferences > 1 ? "s" : "")})";
            } else {
                title += $" (contains unloaded item{(nullReferences > 1 ? "s" : "")})";
            }
        }

        return title;
    }

    /// <summary>
    /// Get the icon texture for the given reference.
    /// </summary>
    public static Texture GetIconTexture(UObject item, Type type = null)
    {
        // Get thumbnail from live reference
        if (item != null) {
            var preview = AssetPreview.GetAssetPreview(item);
            if (preview != null)
                return preview;
            
            var thumbnail = AssetPreview.GetMiniThumbnail(item);
            if (thumbnail != null)
                return thumbnail;
        }

        // Get type-based icon for unloaded references
        if (type != null) {
            return AssetPreview.GetMiniTypeThumbnail(type);
        }

        // Use generic icon for null objects
        return AssetPreview.GetMiniTypeThumbnail(typeof(UObject));
    }

    /// <summary>
    /// Try to get the active variant for the given icon texture.
    /// </summary>
    /// <remarks>
    /// This calls the Unity internal method `EditorUtility.GetIconInActiveState`
    /// to get the active state icon variant for built-in Unity icons.
    /// </remarks>
    public static Texture GetActiveTexture(Texture original)
    {
        if (_GetIconInActiveStateMethod == null && !_GetIconInActiveStateMethodWarnedOnce) {
            _GetIconInActiveStateMethod = typeof(EditorUtility).GetMethod(
                "GetIconInActiveState", 
                BindingFlags.Static | BindingFlags.NonPublic
            );
            if (_GetIconInActiveStateMethod == null && !_GetIconInActiveStateMethodWarnedOnce) {
                _GetIconInActiveStateMethodWarnedOnce = true;
                Debug.LogError($"ShelfRackView: Failed to get method 'EditorUtility.GetIconInActiveState'");
            }
        }

        if (_GetIconInActiveStateMethod == null)
            return null;

        Texture active = null;
        try {
            _GetIconInActiveStateMethodArgs[0] = original;
            active = _GetIconInActiveStateMethod.Invoke(null, _GetIconInActiveStateMethodArgs) as Texture;
        } catch (Exception e) {
            if (!_GetIconInActiveStateMethodWarnedOnce) {
                _GetIconInActiveStateMethodWarnedOnce = true;
                Debug.LogError($"Failed to call 'EditorUtility.GetIconInActiveState': {e.Message}");
            }
        }

        return active;
    }

    static bool _GetIconInActiveStateMethodWarnedOnce;
    static MethodInfo _GetIconInActiveStateMethod;
    static object[] _GetIconInActiveStateMethodArgs = new object[1];

    // ---------- API ----------

    /// <summary>
    /// Rack this view's shelf is part of.
    /// </summary>
    public ShelfRack Rack => rack;
    /// <summary>
    /// Shelf this view is presenting.
    /// </summary>
    public ShelfRack.Shelf Shelf => shelf;

    /// <summary>
    /// Event triggered before an action of a shelf item is executed.
    /// </summary>
    public event Action<ShelfItemEvent> OnShelfItemAction;

    /// <summary>
    /// Accept the current <see cref="DragAndDrop"/> operation and 
    /// add its references to the shelf.
    /// </summary>
    public void AcceptDrag()
    {
        if (DragAndDrop.objectReferences == null || DragAndDrop.objectReferences.Length == 0)
            return;

        DragAndDrop.AcceptDrag();

        InsertItems(Shelf.Items.Count, DragAndDrop.objectReferences, GetDragSourceShelf(), GetDragSourceShelfIndices());
    }

    // ---------- Internals ----------

    /// <summary>
    /// The key used to store the source shelf items view in the drag operation.
    /// </summary>
    public const string ShelfSourceKey = "ShelfSource";
    /// <summary>
    /// The key used to store the indices of the dragged items on the source shelf.
    /// </summary>
    public const string ShelfSourceIndicesKey = "ShelfSourceIndices";

    ShelfRack rack;
    ShelfRack.Shelf shelf;

    public ShelfItemsView(ShelfRack rack, ShelfRack.Shelf shelf) : base()
    {
        this.rack = rack;
        this.shelf = shelf;
        itemsSource = (IList)shelf.Items;

        virtualizationMethod = CollectionVirtualizationMethod.FixedHeight;
        selectionType = SelectionType.Multiple;
        showAlternatingRowBackgrounds = AlternatingRowBackground.None;
        reorderable = true;
        reorderMode = ListViewReorderMode.Simple;

        makeItem = MakeListItem;
        bindItem = BindItem;

    #if UNITY_2023_2_OR_NEWER
        setupDragAndDrop += ItemSetupDrag;
        dragAndDropUpdate += ItemDragUpdate;
        handleDrop += ItemHandleDrop;
        makeNoneElement += MakeNoneElement;
    #else
        RegisterCallback<DragUpdatedEvent>(OnDragUpdated);
        RegisterCallback<DragPerformEvent>(OnDragPerform);
    #endif

        RegisterCallback<AttachToPanelEvent>(OnAttachToPanelEvent);
        RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanelEvent);
        RegisterCallback<NavigationSubmitEvent>(OnNavigationSubmit, TrickleDown.TrickleDown);

        SetSizeClass();
    }

    void OnAttachToPanelEvent(AttachToPanelEvent args)
    {
        ShelfSettings.Project.propertyChanged += OnSettingsChanged;

        if (shelf != null) {
            shelf.propertyChanged += OnShelfPropertyChanged;
        }
    }

    void OnDetachFromPanelEvent(DetachFromPanelEvent args)
    {
        ShelfSettings.Project.propertyChanged -= OnSettingsChanged;

        if (shelf != null) {
            shelf.propertyChanged -= OnShelfPropertyChanged;
        }
    }

    void OnSettingsChanged(object sender, BindablePropertyChangedEventArgs args)
    {
        if (args.propertyName == nameof(ShelfSettings.ItemSize) && shelf.ItemSize == ShelfItemSize.Default) {
            SetSizeClass();
        }
    }

    void OnShelfPropertyChanged(object sender, BindablePropertyChangedEventArgs args)
    {
        if (args.propertyName == nameof(ShelfRack.Shelf.Items)) {
            // Sync changes made to underlying data
            RefreshItems();
        } else if (args.propertyName == nameof(ShelfRack.Shelf.ItemSize)) {
            SetSizeClass();
        }
    }

    VisualElement MakeNoneElement()
    {
        var container = new VisualElement();
        container.AddToClassList("empty");

        var title = new Label("<b>Shelf is empty</b><br>Drag assets or scene objects here to put them on the shelf");
        container.Add(title);

        return container;
    }

    VisualElement MakeListItem()
    {
        var container = new VisualElement();
        container.AddToClassList("shelf-item");
        container.AddManipulator(new ContextualMenuManipulator(OnPopulateContextMenu));
        
        var singleOrDoubleManip = new SingleDoubleClickManipulator(
            (evt) => OnSingleClick(evt, (int)container.userData),
            (evt) => OnDoubleClick(evt, (int)container.userData)
        );
        // Add activation filter to also handle alt-clicks
        singleOrDoubleManip.activators.Add(
            new ManipulatorActivationFilter { button = MouseButton.LeftMouse, modifiers = EventModifiers.Alt }
        );
        container.AddManipulator(singleOrDoubleManip);

        var box = new Box();

        var normalIcon = new Image();
        normalIcon.AddToClassList("shelf-item-icon");
        normalIcon.AddToClassList("shelf-item-icon__normal");
        box.Add(normalIcon);

        var selectedIcon = new Image();
        selectedIcon.AddToClassList("shelf-item-icon");
        selectedIcon.AddToClassList("shelf-item-icon__selected");
        box.Add(selectedIcon);

        var label = new Label();
        label.AddToClassList("shelf-item-name");
        box.Add(label);

        var accessoryContainer = new VisualElement();
        accessoryContainer.AddToClassList("shelf-item-accessory");
        box.Add(accessoryContainer);

        var button = new Button();
        button.AddToClassList("remove-button");
        button.tooltip = "Remove from shelf";
        button.iconImage = ShelfAssets.BackgroundFromObject(ShelfAssets.Shared.removeIcon);
        button.text = "x";
        button.clicked += () => ItemRemove(container);
        box.Add(button);

        container.Add(box);
        return container;
    }

    void BindItem(VisualElement element, int index)
    {
        element.userData = index;

        var accessoryContainer = element.Q(className: "shelf-item-accessory");
        for (int i = accessoryContainer.childCount - 1; i >= 0; i--) {
            accessoryContainer.RemoveAt(i);
        }

        var normalIcon = element.Q<Image>(className: "shelf-item-icon__normal");
        var selectedIcon = element.Q<Image>(className: "shelf-item-icon__selected");
        var label = element.Q<Label>();

        var item = shelf.Items[index];
        if (item == null) {
            // Reference became invalid
            ShelfAssets.ApplyObjectToImage(normalIcon, ShelfAssets.Shared.warningIcon);
            selectedIcon.image = GetActiveTexture(normalIcon.image);
            label.text = "<Not Found>";
            return;
        }

        if (item is IShelfItem shelfItem) {
            // Let IShelfItem handle presentation
            normalIcon.image = shelfItem.Icon;
            selectedIcon.image = shelfItem.GetActiveIcon(normalIcon.image) ?? normalIcon.image;
            label.text = shelfItem.Name;

            var accessory = shelfItem.Accessory;
            if (accessory != null) {
                accessoryContainer.Add(accessory);
            }
            return;
        }

        // Default handling
        normalIcon.image = GetIconTexture(item);
        selectedIcon.image = GetActiveTexture(normalIcon.image) ?? normalIcon.image;
        label.text = item.name;
    }

    void SetSizeClass()
    {
        var size = ShelfSettings.Project.ResolveSize(shelf.ItemSize);
        EnableInClassList("item-size-tiny",   size == ShelfItemSize.Tiny);
        EnableInClassList("item-size-small",  size == ShelfItemSize.Small);
        EnableInClassList("item-size-medium", size == ShelfItemSize.Medium);
        EnableInClassList("item-size-large",  size == ShelfItemSize.Large);
        EnableInClassList("item-size-huge",   size == ShelfItemSize.Huge);
    }

    void OnNavigationSubmit(NavigationSubmitEvent evt)
    {
        var altKey = false;
    #if UNITY_2022_2_OR_NEWER
        altKey = evt.altKey;
    #endif

        var type = ShelfItemEventType.TypeSubmit | (
            altKey 
                ? ShelfItemEventType.ModifierAlt
                : ShelfItemEventType.ModifierNone
        );

        foreach (var index in selectedIndices) {
            TriggerShelfAction(index, type);
        }
    }

    void InsertItems(int insertAt, IEnumerable<UObject> objects, ShelfItemsView sourceShelf, int[] sourceIndices)
    {
        if (sourceShelf != null && sourceIndices != null) {
            // Get the original shelf items, the drag itself contains the unwrapped ones
            objects = sourceIndices.Select(index => sourceShelf.Shelf.Items[index]);
        }

        shelf.InsertItems(insertAt, objects);

        if (IsMove(sourceShelf, this) && sourceIndices != null) {
            var sourceItems = sourceShelf.Shelf.Items;
            foreach (var index in sourceIndices.OrderBy(i => -i)) {
                if (index < 0 || index >= sourceItems.Count) continue;
                sourceItems.RemoveAt(index);
            }
        }
    }

    void ItemRemove(VisualElement itemRoot)
    {
        if (itemRoot.userData is not int index)
            return;

        if (index < 0 || index >= shelf.Items.Count)
            return;

        shelf.Items.RemoveAt(index);
    }

    void OnSingleClick(ClickEvent evt, int index)
    {
        if (index < 0 || index >= shelf.Items.Count)
            return;

        var type = ShelfItemEventType.TypeClick | (
            evt.altKey 
                ? ShelfItemEventType.ModifierAlt
                : ShelfItemEventType.ModifierNone
        );

        TriggerShelfAction(index, type);
    }

    void OnDoubleClick(ClickEvent evt, int index)
    {
        if (index < 0 || index >= shelf.Items.Count)
            return;

        var type = ShelfItemEventType.TypeDoubleClick | (
            evt.altKey 
                ? ShelfItemEventType.ModifierAlt
                : ShelfItemEventType.ModifierNone
        );

        TriggerShelfAction(index, type);
    }

    void TriggerShelfAction(int index, ShelfItemEventType eventType)
    {
        var evt = new ShelfItemEvent() {
            eventType = eventType,
            item = new ShelfItem(this, index, shelf.Items[index])
        };

        OnShelfItemAction?.Invoke(evt);
        if (evt.eventType == ShelfItemEventType.None || evt.item?.itemsView == null || evt.item.itemIndex < 0) return;

        if (!ShelfApi.ProcessAction(evt)) {
            Debug.LogWarning($"TheShelf: No handler registered to handle click on item '{evt.item.reference}' (for {eventType})", evt.item.reference);
        }
    }

    void OnPopulateContextMenu(ContextualMenuPopulateEvent args)
    {
        if (args.currentTarget is not VisualElement container)
            return;

        if (container.userData is not int clickIndex)
            return;

        contextMenuTempItems ??= new();
        contextMenuTempItems.Clear();

        if (!selectedIndices.Contains(clickIndex)) {
            // Clicked on non-selected item, open context only for clicked item
            if (clickIndex >= 0 && clickIndex < shelf.Items.Count) {
                contextMenuTempItems.Add(new ShelfItem(this, clickIndex, shelf.Items[clickIndex]));
            }
        } else {
            // Clicked on selection, open context for all selected items
            foreach (var index in selectedIndices) {
                if (index < 0 || index >= shelf.Items.Count)
                    continue;
                contextMenuTempItems.Add(new ShelfItem(this, index, shelf.Items[index]));
            }
        }

        if (contextMenuTempItems.Count > 0) {
            ShelfApi.ProcessContextMenu(contextMenuTempItems, args);
        }
    }

    static List<ShelfItem> contextMenuTempItems;

#if UNITY_2023_2_OR_NEWER
    StartDragArgs ItemSetupDrag(SetupDragAndDropArgs args)
    {
        var shelfItems = args.selectedIds.Select(index => shelf.Items[index]);
        var title = GetDragTitle(shelfItems);
        var references = shelfItems.Select(item => GetDragReference(item));

        var startDragArgs = new StartDragArgs(title, DragVisualMode.Move);
        startDragArgs.SetUnityObjectReferences(references);
        startDragArgs.SetGenericData(ShelfSourceKey, this);
        startDragArgs.SetGenericData(ShelfSourceIndicesKey, selectedIndices.ToArray());

        EditorApplication.delayCall = () => {
            this.ReleasePointer(PointerId.mousePointerId);
        };

        return startDragArgs;
    }

    DragVisualMode ItemDragUpdate(HandleDragAndDropArgs args)
    {
        var sourceShelf = GetDragSourceShelf(args.dragAndDropData);

        if (sourceShelf != null && sourceShelf == this)
            return DragVisualMode.None;

        if (args.dragAndDropData.unityObjectReferences == null)
            return DragVisualMode.None;

        return IsMove(sourceShelf, this) ? DragVisualMode.Move : DragVisualMode.Copy;
    }

    DragVisualMode ItemHandleDrop(HandleDragAndDropArgs args)
    {
        var sourceShelf = GetDragSourceShelf(args.dragAndDropData);

        if (sourceShelf != null && sourceShelf == this)
            return DragVisualMode.None;

        if (args.dragAndDropData.unityObjectReferences == null)
            return DragVisualMode.None;

        var sourceIndices = GetDragSourceShelfIndices(args.dragAndDropData);
        InsertItems(args.insertAtIndex, args.dragAndDropData.unityObjectReferences, sourceShelf, sourceIndices);
        return IsMove(sourceShelf, this) ? DragVisualMode.Move : DragVisualMode.Copy;
    }
#else
    const string dragSourceKey = "__unity-drag-and-drop__source-view";

    void OnDragUpdated(DragUpdatedEvent args)
    {
        var dragSource = DragAndDrop.GetGenericData(dragSourceKey);

        // Extend drags that originate from this shelf
        if (DragAndDrop.objectReferences.Length == 0) {
            if (dragSource != null && dragSource != this)
                return;

            DragAndDrop.objectReferences = selectedIndices.Select(index => GetDragReference(shelf.Items[index])).ToArray();
            DragAndDrop.SetGenericData(ShelfSourceKey, this);
            DragAndDrop.SetGenericData(ShelfSourceIndicesKey, selectedIndices.ToArray());

            EditorApplication.delayCall = () => {
                this.ReleasePointer(PointerId.mousePointerId);
            };

        // Handle drags from outside
        } else {
            if (dragSource != null && dragSource == this)
                return;

            DragAndDrop.SetGenericData(dragSourceKey, this);
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
        }
    }

    void OnDragPerform(DragPerformEvent args)
    {
        var sourceShelf = GetDragSourceShelf();
        if (sourceShelf != null && sourceShelf == this)
            return;

        if (DragAndDrop.objectReferences == null || DragAndDrop.objectReferences.Length == 0)
            return;

        var insertIndex = this.GetInsertPosition(args.mousePosition);
        if (insertIndex < 0) {
            Debug.LogWarning($"Could not get insert position!");
            insertIndex = shelf.Items.Count;
        }

        var sourceIndices = GetDragSourceShelfIndices();
        InsertItems(insertIndex, DragAndDrop.objectReferences, sourceShelf, sourceIndices);
    }
#endif

    /// <summary>
    /// Manipulator to expose single and double click events on the same element.
    /// A double click timeout will be used to delay the single click and
    /// distinguish between single and double clicks.
    /// </summary>
    public class SingleDoubleClickManipulator : PointerManipulator
    {
        /// <summary>
        /// The delay (in ms) after a pointer up event to determine if it's a single
        /// click or if a second click will follow.
        /// </summary>
        /// <remarks>
        /// This effectively delays the single click event after mouse up for this duration.
        /// A typical OS default seems to be 500ms. To minimize the single click delay,
        /// we're using a lower default value here.
        /// It's possible to customize the delay using an `InitializeOnLoad` class,
        /// overwriting this value.
        /// </remarks>
        public static long DoubleClickInterval = 250;

        public event Action<ClickEvent> singleClicked;
        public event Action<ClickEvent> doubleClicked;

        public SingleDoubleClickManipulator(Action<ClickEvent> singleClick, Action<ClickEvent> doubleClick) : this()
        {
            singleClicked += singleClick;
            doubleClicked += doubleClick;
        }

        public SingleDoubleClickManipulator()
        {
            activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<PointerDownEvent>(OnPointerDown);
            target.RegisterCallback<PointerUpEvent>(OnPointerUp);
            target.RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
            target.RegisterCallback<PointerCancelEvent>(OnPointerCancel);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
            target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
            target.UnregisterCallback<PointerLeaveEvent>(OnPointerLeave);
            target.UnregisterCallback<PointerCancelEvent>(OnPointerCancel);
            Reset();
        }

        bool active;
        int clickCount;
        ClickEvent copiedEvent;
        IVisualElementScheduledItem clickDelay;

        void OnPointerDown(PointerDownEvent evt)
        {
            if (!CanStartManipulation(evt)) return;

            active = true;

            // Do not stop propagation to allow drag to start
            //evt.StopImmediatePropagation();
        }

        void OnPointerUp(PointerUpEvent evt)
        {
            if (!active || !CanStopManipulation(evt)) return;

            active = false;
            evt.StopPropagation();

            ProcessPointerUp(evt);
        }

        void OnPointerLeave(PointerLeaveEvent evt)
        {
            if (!active || !CanStopManipulation(evt)) return;

            Reset();
            evt.StopPropagation();
        }

        void OnPointerCancel(PointerCancelEvent evt)
        {
            if (!active || !CanStopManipulation(evt)) return;

            Reset();
            evt.StopPropagation();
        }

        void ProcessPointerUp(PointerUpEvent evt)
        {
            clickCount++;

            if (clickCount == 1) {
                // Start delay for single click
                clickDelay ??= target.schedule.Execute(TriggerSingleClick);
                clickDelay.ExecuteLater(DoubleClickInterval);

                // Copy event since it's only valid for the duration of the event handler
                // but we want to deliver it when the click interval expires
                copiedEvent = ClickEvent.GetPooled(evt);

            } else if (clickCount == 2) {
                // Handle double click
                if (doubleClicked != null) {
                    using (var clickEvent = ClickEvent.GetPooled(evt)) {
                        try {
                            doubleClicked?.Invoke(clickEvent);
                        } catch (Exception e) {
                            Debug.LogException(e);
                        }
                    }
                }
                Reset();
            }
        }

        void TriggerSingleClick()
        {
            if (active) return;

            try {
                singleClicked?.Invoke(copiedEvent);
            } catch (Exception e) {
                Debug.LogException(e);
            }
            Reset();
        }

        void Reset()
        {
            active = false;
            clickCount = 0;
            clickDelay?.Pause();
            copiedEvent?.Dispose();
            copiedEvent = null;
        }
    }
}

}

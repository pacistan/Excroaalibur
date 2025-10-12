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
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UIElements;

#if !UNITY_2023_2_OR_NEWER
using sttz.TheShelf.Backwards;
using Button = sttz.TheShelf.Backwards.IconButton;
#endif

using UObject = UnityEngine.Object;

namespace sttz.TheShelf {

/// <summary>
/// View that presents a single <see cref="ShelfRack"/>.
/// </summary>
public class ShelfRackView : VisualElement
{
    // ---------- API ----------

    /// <summary>
    /// The rack that's shown in this view.
    /// </summary>
    public ShelfRack Rack {
        get => _rack;
        set {
            if (_rack == value) return;

            if (_rack != null) {
                _rack.propertyChanged -= RackPropertyChanged;
            }

            _rack = value;

            if (_rack != null) {
                _rack.propertyChanged += RackPropertyChanged;
            }

            if (_rack != null) {
                CreateSettings();
            }
            SyncShelvesAndTabs();
        }
    }
    ShelfRack _rack;

    /// <summary>
    /// The index of the currently active shelf or settings tab.
    /// </summary>
    public int ActiveTabIndex {
        get => _activeTabIndex;
        set {
            if (_activeTabIndex == value) return;
            _activeTabIndex = value;
            SyncShelvesAndTabs();
        }
    }
    int _activeTabIndex;

    /// <summary>
    /// Focus the shelf items list of the active shelf tab.
    /// </summary>
    public void FocusActiveShelfItemsList()
    {
        Tab activeTab = null;
        if (_activeTabIndex >= 0 && _activeTabIndex < tabs.childCount) {
            activeTab = tabs[_activeTabIndex] as Tab;
        }

        if (activeTab?.userData is not ShelfItemsView itemsView)
            return;

        itemsView.Focus();
    }

    /// <summary>
    /// Wether the view is contained in a popup window.
    /// </summary>
    public bool IsPopup {
        get => _isPopup;
        set {
            if (_isPopup == value) return;
            _isPopup = value;

            if (_isPopup) {
                CreateDockButton();
                if (_rack != null) {
                    SyncShelvesAndTabs();
                }
            } else if (dockButton != null) {
                dockButton.RemoveFromHierarchy();
            }
        }
    }
    bool _isPopup;

    /// <summary>
    /// Event triggered before an action of an item on any of this rack's shelves is executed.
    /// </summary>
    public event Action<ShelfItemEvent> OnShelfItemAction;

    /// <summary>
    /// Event triggered when a new tab is added to the rack view.
    /// </summary>
    public event Action<Tab> OnTabAdded;
    /// <summary>
    /// Event triggered when a tab is removed from the rack view.
    /// </summary>
    public event Action<Tab> OnTabRemoved;

    /// <summary>
    /// Event triggered when a popup window should be docked.
    /// </summary>
    public event Action OnPopupDock;

    // ---------- Internals ----------

    TabView tabs;
    Tab settingsTab;
    bool shelfOrderDirty;
    VisualElement headerContainer;
    VisualElement dockButton;

    public ShelfRackView()
    {
        SetStyleSheets();

        RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
        RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);

        tabs = new TabView();
        tabs.activeTabChanged += OnActiveTabChanged;
        tabs.contentContainer.RegisterCallback<DragUpdatedEvent>(DragBackgroundUpdated);
        tabs.contentContainer.RegisterCallback<DragPerformEvent>(DragBackgroundPerform);

        headerContainer = tabs.Q(className: TabView.headerContainerClassName);
        if (headerContainer != null) {
            headerContainer.RegisterCallback<DragUpdatedEvent>(DragHeaderBackgroundUpdated);
            headerContainer.RegisterCallback<DragPerformEvent>(DragHeaderBackgroundPerform);
            headerContainer.RegisterCallback<DragExitedEvent>(DragHeaderBackgroundExited);
            headerContainer.RegisterCallback<DragLeaveEvent>(DragHeaderBackgroundLeave);
        }

    #if UNITY_2023_2_OR_NEWER
        focusable = true;
        delegatesFocus = true;

        tabs.focusable = true;
        tabs.delegatesFocus = true;

        tabs.contentContainer.focusable = true;
        tabs.contentContainer.delegatesFocus = true;
    #endif

        Add(tabs);
    }

    void OnAttachToPanel(AttachToPanelEvent args)
    {
        ShelfSettings.Project.propertyChanged += OnSettingsChanged;

        if (_rack != null) {
            _rack.propertyChanged += RackPropertyChanged;
        }

        if (shelfOrderDirty) {
            shelfOrderDirty = false;
            if (_rack != null) {
                SyncShelvesAndTabs();
            }
        }
    }

    void OnDetachFromPanel(DetachFromPanelEvent args)
    {
        ShelfSettings.Project.propertyChanged -= OnSettingsChanged;

        if (_rack != null) {
            _rack.propertyChanged -= RackPropertyChanged;
        }
    }

    void OnSettingsChanged(object sender, BindablePropertyChangedEventArgs args)
    {
        if (args.propertyName == nameof(ShelfSettings.CustomStyleSheet)) {
            SetStyleSheets();
        }
    }

    void SetStyleSheets()
    {
        styleSheets.Clear();

    #if !UNITY_2023_2_OR_NEWER
        if (ShelfAssets.Shared.styleSheetBackwards != null) {
            styleSheets.Add(ShelfAssets.Shared.styleSheetBackwards);
        }
    #endif

        if (ShelfAssets.Shared.styleSheet != null) {
            styleSheets.Add(ShelfAssets.Shared.styleSheet);
        }
        if (ShelfSettings.Project.CustomStyleSheet != null) {
            styleSheets.Add(ShelfSettings.Project.CustomStyleSheet);
        }
    }

    Tab CreateTab(ShelfRack rack, ShelfRack.Shelf shelf)
    {
        var tab = new Tab();

        var shelfView = new ShelfItemsView(rack, shelf);
        tab.Add(shelfView);
        tab.userData = shelfView;

    #if UNITY_2023_2_OR_NEWER
        tab.delegatesFocus = true;
        tab.contentContainer.delegatesFocus = true;
    #endif

        var manipulator = new TabHeaderDragAndDropManipulator(shelfView);
        manipulator.dragActivate += () => {
            tabs.activeTab = tab;
        };
        tab.tabHeader.AddManipulator(manipulator);

        var tabEmptySpace = new VisualElement();
        tabEmptySpace.AddToClassList("shelf-tab-empty-space");
        tab.Add(tabEmptySpace);

        // Before Unity 2023.2, the click event wrongly has the target
        // set to the value of currentTarget, making it impossible to know
        // which element was clicked.
        // We register click event here as a workaround.
        tabEmptySpace.RegisterCallback<ClickEvent>(args => {
            shelfView.selectedIndex = -1;
        });

        shelfView.OnShelfItemAction += OnIndividualShelfItemAction;
        shelf.propertyChanged += OnShelfPropertyChanged;

        ApplyShelfSettings(shelf, tab);

        return tab;
    }

    void UnbindTab(Tab tab)
    {
        if (tab.userData is ShelfItemsView shelfView) {
            shelfView.OnShelfItemAction -= OnIndividualShelfItemAction;
            shelfView.Shelf.propertyChanged -= OnShelfPropertyChanged;
        }
    }

    void OnIndividualShelfItemAction(ShelfItemEvent evt)
    {
        OnShelfItemAction?.Invoke(evt);
    }

    void OnShelfPropertyChanged(object sender, BindablePropertyChangedEventArgs args)
    {
        if (args.propertyName == nameof(ShelfRack.Shelf.Name) 
                || args.propertyName == nameof(ShelfRack.Shelf.Icon)
                || args.propertyName == nameof(ShelfRack.Shelf.HeaderTextColor)
                || args.propertyName == nameof(ShelfRack.Shelf.HeaderBackgroundColor)) {
            for (int i = tabs.childCount - 1; i >= 0; i--) {
                if (tabs[i] is not Tab tab) continue;
                if (tab.userData is ShelfItemsView shelfView && shelfView.Shelf == sender) {
                    ApplyShelfSettings(shelfView.Shelf, tab);
                }
            }
        }
    }

    void ApplyShelfSettings(ShelfRack.Shelf shelf, Tab tab)
    {
        tab.label = shelf.Name;
        tab.iconImage = ShelfAssets.BackgroundFromObject(shelf.Icon);

        tab.tabHeader.EnableInClassList("shelf-tab-header-has-icon", tab.iconImage != default);

        if (shelf.HeaderTextColor.r < 0) {
            tab.tabHeader.style.color = new StyleColor(StyleKeyword.Null);
        } else {
            tab.tabHeader.style.color = new StyleColor(shelf.HeaderTextColor);
        }

        if (shelf.HeaderBackgroundColor.r < 0) {
            tab.tabHeader.style.backgroundColor = new StyleColor(StyleKeyword.Null);
        } else {
            tab.tabHeader.style.backgroundColor = new StyleColor(shelf.HeaderBackgroundColor);
        }
    }

    void CreateSettings()
    {
        if (settingsTab != null) {
            settingsTab.RemoveFromHierarchy();
            settingsTab = null;
        }

        settingsTab = new ShelfSettingsTab(_rack);
    }

    void CreateDockButton()
    {
        if (dockButton != null) return;

        dockButton = new VisualElement();
        dockButton.AddToClassList(Tab.tabHeaderUssClassName);
        dockButton.AddToClassList("shelf-dock-button");

        var image = new Image();
        if (!ShelfAssets.ApplyObjectToImage(image, ShelfAssets.Shared.dockIcon)) {
            image.AddToClassList(Tab.tabHeaderEmptyImageUssClassName);
        }
        image.AddToClassList(Tab.tabHeaderImageUssClassName);
        dockButton.Add(image);

        var label = new Label();
        label.AddToClassList(Tab.tabHeaderLabelUssClassName);
        //label.AddToClassList(Tab.tabHeaderEmptyLabeUssClassName);
        dockButton.Add(label);

        var underline = new VisualElement();
        underline.AddToClassList(Tab.tabHeaderUnderlineUssClassName);
        dockButton.Add(underline);

        dockButton.RegisterCallback<ClickEvent>(args => {
            OnPopupDock?.Invoke();
        });

        headerContainer.Add(dockButton);
    }

    void DragHeaderBackgroundUpdated(DragUpdatedEvent args)
    {
        var sourceShelf = ShelfItemsView.GetDragSourceShelf();
        if (sourceShelf == null || sourceShelf.Rack == _rack) return;

        var sourceIndices = ShelfItemsView.GetDragSourceShelfIndices();
        if (sourceIndices == null || sourceIndices.Length != sourceShelf.Shelf.Items.Count) return;

        if (args.currentTarget != args.target) {
            headerContainer.RemoveFromClassList("shelf-tab-background-dragging");
            DragAndDrop.visualMode = DragAndDropVisualMode.None;
            return;
        }

        headerContainer.AddToClassList("shelf-tab-background-dragging");

        DragAndDrop.visualMode = Event.current?.alt != true
            ? DragAndDropVisualMode.Move
            : DragAndDropVisualMode.Copy;
    }

    void DragHeaderBackgroundLeave(DragLeaveEvent args)
    {
        headerContainer.RemoveFromClassList("shelf-tab-background-dragging");
    }

    void DragHeaderBackgroundExited(DragExitedEvent args)
    {
        headerContainer.RemoveFromClassList("shelf-tab-background-dragging");
    }

    void DragHeaderBackgroundPerform(DragPerformEvent args)
    {
        var sourceShelf = ShelfItemsView.GetDragSourceShelf();
        if (sourceShelf == null || sourceShelf.Rack == _rack) return;

        var insertShelf = sourceShelf.Shelf;

        var isMove = Event.current?.alt != true;
        if (isMove) {
            sourceShelf.Rack.Shelves.Remove(sourceShelf.Shelf);
        } else {
            insertShelf = insertShelf.Clone();
        }

        _rack.Shelves.Add(insertShelf);

        DragAndDrop.AcceptDrag();

        headerContainer.RemoveFromClassList("shelf-tab-background-dragging");
    }

    void DragBackgroundUpdated(DragUpdatedEvent args)
    {
        var target = tabs.userData as ShelfItemsView;
        DragAndDrop.visualMode = ShelfItemsView.IsMove(target)
            ? DragAndDropVisualMode.Move
            : DragAndDropVisualMode.Copy;
    }

    void DragBackgroundPerform(DragPerformEvent args)
    {
        if (tabs.activeTab.userData is not ShelfItemsView shelfView)
            return;

        var sourceView = ShelfItemsView.GetDragSourceShelf();
        if (sourceView == shelfView)
            return;

        shelfView.AcceptDrag();
    }

    void RackPropertyChanged(object sender, BindablePropertyChangedEventArgs args)
    {
        if ((ShelfRack)sender != _rack)
            return;

        if (args.propertyName == nameof(ShelfRack.Shelves) && !shelfOrderDirty) {
            shelfOrderDirty = true;
            EditorApplication.delayCall += SyncShelvesAndTabs;
        }
    }

    void OnActiveTabChanged(Tab oldTab, Tab newTab)
    {
        _activeTabIndex = tabs.selectedTabIndex;

        if (oldTab != null) {
            oldTab.focusable = false;
            oldTab.contentContainer.focusable = false;
        }

        if (newTab != null) {
            newTab.focusable = true;
            newTab.contentContainer.focusable = true;

            if (newTab.userData is ShelfItemsView itemsView) {
                // Focus the items view when changing tabs,
                // delay is required for view to become ready and focusable
                EditorApplication.delayCall += itemsView.Focus;
            }
        }
    }

    List<Tab> existingTabs;

    void SyncShelvesAndTabs()
    {
        shelfOrderDirty = false;

        var activeIndex = _activeTabIndex;
        if (activeIndex < 0) {
            activeIndex = tabs.selectedTabIndex;
        }

        Tab activeTab = null;
        if (activeIndex >= 0 && activeIndex < tabs.childCount) {
            activeTab = tabs[activeIndex] as Tab;
        }

        // Remove all existing tabs
        // This is required because inserting tabs appends their header
        // instead of inserting it at the correct position
        existingTabs ??= new();
        for (int i = tabs.childCount - 1; i >= 0; i--) {
            if (tabs[i] is Tab tab) {
                existingTabs.Add(tab);
            #if UNITY_2023_2_OR_NEWER
                tabs.Remove(tab);
            #else
                tabs.RemoveTab(tab);
            #endif
            }
        }

        // Re-add tabs in order
        int index;
        for (index = 0; index < _rack.Shelves.Count; index++) {
            var shelf = _rack.Shelves[index];

            Tab existing = null;
            for (int i = 0; i < tabs.childCount; i++) {
                var vo = tabs[i];

                if (vo is not Tab tab)
                    continue;
                if (tab.userData is not ShelfItemsView itemsView)
                    continue;

                if (itemsView.Shelf == shelf) {
                    existing = tab;
                    existingTabs.RemoveAt(i);
                    break;
                }
            }

            if (existing != null) {
                // Tab already exists
            #if UNITY_2023_2_OR_NEWER
                tabs.Add(existing);
            #else
                tabs.AddTab(existing);
            #endif
            } else {
                // No tab for shelf, create one
                existing = CreateTab(_rack, shelf);

            #if UNITY_2023_2_OR_NEWER
                tabs.Add(existing);
            #else
                tabs.AddTab(existing);
            #endif

                OnTabAdded?.Invoke(existing);
            }
        }

        foreach (var removed in existingTabs) {
            UnbindTab(removed);
            OnTabRemoved?.Invoke(removed);
        }
        existingTabs.Clear();

        // Add settings tab last
        if (settingsTab != null) {
        #if UNITY_2023_2_OR_NEWER
            tabs.Add(settingsTab);
        #else
            tabs.AddTab(settingsTab);
        #endif
        }

        if (dockButton != null && dockButton.parent != null) {
            dockButton.RemoveFromHierarchy();
            headerContainer.Add(dockButton);
        }

        // Restore selected tab
        if (activeTab != null && tabs.IndexOf(activeTab) >= 0) {
            tabs.activeTab = activeTab;
        } else if (activeIndex >= 0 && activeIndex < tabs.childCount) {
            tabs.activeTab = tabs[activeIndex] as Tab;
        }
    }

    /// <summary>
    /// Manipulator to support dragging references on tab headers
    /// and dragging away references from tab headers.
    /// </summary>
    class TabHeaderDragAndDropManipulator : PointerManipulator
    {
        /// <summary>
        /// Distance to start a drag when dragging from the tab header.
        /// </summary>
        public static float startDragDistance = 5f;
        /// <summary>
        /// Timeout when dragging on a tab before it is activated.
        /// </summary>
        /// <remarks>
        /// Note that Unity only updates when receiving input,
        /// so when the mouse is held still, this timeout
        /// cannot be checked until it is moved.
        /// </remarks>
        public static float dragActivateTabTimeout = 1.3f;

        /// <summary>
        /// Event called when the tab should be activated.
        /// </summary>
        public Action dragActivate;

        ShelfItemsView itemsView;
        bool isMoving;
        Vector2 startPos;
        double dragEnterTime;
        IVisualElementScheduledItem scheduleItem;

        public TabHeaderDragAndDropManipulator(ShelfItemsView itemsView)
        {
            this.itemsView = itemsView;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<PointerDownEvent>(OnPointerDown);
            target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            target.RegisterCallback<PointerUpEvent>(OnPointerUp);
            target.RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
            target.RegisterCallback<DragEnterEvent>(OnDragEnter);
            target.RegisterCallback<DragLeaveEvent>(OnDragLeave);
            target.RegisterCallback<DragUpdatedEvent>(OnDragUpdate);
            target.RegisterCallback<DragPerformEvent>(OnDragPerform);
            target.RegisterCallback<DragExitedEvent>(OnDragExit);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
            target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
            target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
            target.UnregisterCallback<PointerLeaveEvent>(OnPointerLeave);
            target.UnregisterCallback<DragEnterEvent>(OnDragEnter);
            target.UnregisterCallback<DragLeaveEvent>(OnDragLeave);
            target.UnregisterCallback<DragUpdatedEvent>(OnDragUpdate);
            target.UnregisterCallback<DragPerformEvent>(OnDragPerform);
            target.UnregisterCallback<DragExitedEvent>(OnDragExit);
        }

        void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.currentTarget is not VisualElement ve) return;

            isMoving = true;
            startPos = ve.ChangeCoordinatesTo(target, evt.localPosition);
        }

        void OnPointerMove(PointerMoveEvent evt)
        {
            if (!isMoving) return;

            if (evt.currentTarget is not VisualElement ve) return;

            var currentPos = ve.ChangeCoordinatesTo(target, evt.localPosition);
            if (Vector2.Distance(startPos, currentPos) < startDragDistance)
                return;

            var items = itemsView.Shelf.Items;
            if (items.Count == 0)
                return;

            DragAndDrop.PrepareStartDrag();

            DragAndDrop.objectReferences = items.Select(i => ShelfItemsView.GetDragReference(i)).ToArray();
            DragAndDrop.SetGenericData(ShelfItemsView.ShelfSourceKey, itemsView);
            DragAndDrop.SetGenericData(ShelfItemsView.ShelfSourceIndicesKey, Enumerable.Range(0, items.Count).ToArray());

            var count = DragAndDrop.objectReferences.Length;
            DragAndDrop.StartDrag($"{itemsView.Shelf.Name} ({count} item{(count > 1 ? "s" : "")})");
        }

        void OnPointerUp(PointerUpEvent evt)
        {
            isMoving = false;
        }

        void OnPointerLeave(PointerLeaveEvent evt)
        {
            isMoving = false;
        }

        void OnDragEnter(DragEnterEvent _)
        {
            BeginDragOnTab();

            if (GetVisualMode() == DragAndDropVisualMode.None)
                return;

            target.AddToClassList("shelf-tab-dragging");
        }

        void OnDragLeave(DragLeaveEvent _)
        {
            isMoving = false;
            EndDragOnTab();
            target.RemoveFromClassList("shelf-tab-dragging");
        }

        void OnDragUpdate(DragUpdatedEvent _)
        {
            DragAndDrop.visualMode = GetVisualMode();
        }

        void OnDragPerform(DragPerformEvent _)
        {
            isMoving = false;
            EndDragOnTab();

            if (GetVisualMode() == DragAndDropVisualMode.None)
                return;

            itemsView.AcceptDrag();

            target.RemoveFromClassList("shelf-tab-dragging");
        }

        void OnDragExit(DragExitedEvent _)
        {
            isMoving = false;
            EndDragOnTab();
            target.RemoveFromClassList("shelf-tab-dragging");
        }

        void BeginDragOnTab()
        {
            EndDragOnTab();

            dragEnterTime = EditorApplication.timeSinceStartup;

            scheduleItem = target.schedule.Execute(CheckDragTime);
            scheduleItem.ExecuteLater((long)(dragActivateTabTimeout * 1000));
        }

        void EndDragOnTab()
        {
            if (scheduleItem != null) {
                scheduleItem.Pause();
                scheduleItem = null;
            }
        }

        void CheckDragTime()
        {
            var elapsed = EditorApplication.timeSinceStartup - dragEnterTime;
            if (elapsed > dragActivateTabTimeout) {
                dragActivate?.Invoke();
                EndDragOnTab();
            }
        }

        DragAndDropVisualMode GetVisualMode()
        {
            if (!ShelfItemsView.CanAccept(DragAndDrop.objectReferences))
                return DragAndDropVisualMode.None;

            var sourceShelf = ShelfItemsView.GetDragSourceShelf();
            if (sourceShelf != null && sourceShelf == itemsView)
                return DragAndDropVisualMode.None;

            return ShelfItemsView.IsMove(itemsView)
                ? DragAndDropVisualMode.Move
                : DragAndDropVisualMode.Copy;
        }
    }
}

}

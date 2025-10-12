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
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

using UObject = UnityEngine.Object;

namespace sttz.TheShelf {

/// <summary>
/// Interface for objects to customize their appearance
/// when put on shelves.
/// </summary>
public interface IShelfItem
{
    /// <summary>
    /// The display name of the item.
    /// </summary>
    string Name { get; }
    /// <summary>
    /// The icon or preview of the item.
    /// </summary>
    Texture Icon { get; }
    /// <summary>
    /// The icon to use when the item is selected.
    /// </summary>
    Texture GetActiveIcon(Texture icon) => ShelfItemsView.GetActiveTexture(icon);
    /// <summary>
    /// Optional visual element that will be added
    /// to the item view on the shelf.
    /// </summary>
    VisualElement Accessory => null;

    /// <summary>
    /// The reference to use when dragging or clicking the item.
    /// Return `null` to mark the item as unavailable.
    /// </summary>
    UObject Reference { get; }
    /// <summary>
    /// The asset the is containing the reference.
    /// E.g. the scene that contains a GameObject or Component.
    /// </summary>
    UObject ContainerAsset => null;

    /// <summary>
    /// Wether this object should be persisted as part of the rack asset.
    /// </summary>
    bool SaveInRackAsset => true;
    /// <summary>
    /// Create a copy of this item.
    /// </summary>
    UObject Clone();
}

/// <summary>
/// Momentary information about a shelf item,
/// used for callbacks.
/// </summary>
public class ShelfItem
{
    // ---------- Constructor ----------

    // <summary>
    /// Create a new placed item instance.
    /// </summary>
    public ShelfItem(ShelfItemsView itemsView, int index, UObject item)
    {
        this.itemsView = itemsView;
        itemIndex = index;
        reference = ShelfApi.UnwrapShelfItem(item, out shelfItem);
    }

    // ---------- Properties ----------

    /// <summary>
    /// The view the items are shown with, provides access to the rack and shelf instances.
    /// </summary>
    public ShelfItemsView itemsView;
    /// <summary>
    /// The index of the item on the shelf, i.e. in <see cref="ShelfRack.Shelf.Items"/>.
    /// </summary>
    public int itemIndex;
    /// <summary>
    /// The resolved item reference.
    /// </summary>
    public UObject reference;
    /// <summary>
    /// The wrapper that is used to store the item on the shelf, or `null` if the item is placed directly.
    /// </summary>
    public IShelfItem shelfItem;
}

/// <summary>
/// <see cref="ShelfItemEvent"/> flags.
/// </summary>
[Flags]
public enum ShelfItemEventType
{
    None,

    /// <summary>
    /// The event was triggered by a single click.
    /// </summary>
    TypeClick = 1<<0,
    /// <summary>
    /// The event was triggered by a double click.
    /// </summary>
    TypeDoubleClick = 1<<1,
    /// <summary>
    /// The event was triggered by pressing enter.
    /// </summary>
    TypeSubmit = 1<<2,
    /// <summary>
    /// Mask for events that should open the item, double click and submit.
    /// </summary>
    TypeOpen = TypeDoubleClick | TypeSubmit,
    /// <summary>
    /// Mask for any of the event types.
    /// </summary>
    TypeAny = TypeClick | TypeDoubleClick | TypeSubmit, 

    /// <summary>
    /// No modifier key was pressed during the event.
    /// </summary>
    ModifierNone = 1<<10,
    /// <summary>
    /// The alt key was pressed during the event.
    /// </summary>
    ModifierAlt = 1<<11,
    /// <summary>
    /// Mask for both no modifier or alt key pressed.
    /// </summary>
    ModifierAny = ModifierNone | ModifierAlt,

    /// <summary>
    /// The default event mask,
    /// execute on double click and submit when not pressing
    /// any modifier keys.
    /// </summary>
    Default = TypeDoubleClick | TypeSubmit | ModifierNone,
    /// <summary>
    /// Mask covering all events.
    /// </summary>
    All = -1,
}

/// <summary>
/// Shelf item event.
/// </summary>
public class ShelfItemEvent
{
    /// <summary>
    /// The event flags.
    /// </summary>
    public ShelfItemEventType eventType;
    /// <summary>
    /// The item related to the event.
    /// </summary>
    public ShelfItem item;
}

/// <summary>
/// APIs to customize the behavior of The Shelf.
/// </summary>
public static class ShelfApi
{
    /// <summary>
    /// The Shelf version number.
    /// </summary>
    public static readonly Version Version = new Version(3, 0, 0, 6);

    // ---------- Item Processors ----------

    /// <summary>
    /// Delegate for methods processing items put on the shelf.
    /// </summary>
    /// <param name="item">The item passed through the processor chain</param>
    /// <returns>The item to continue with / put on the shelf</returns>
    public delegate UObject ShelfItemProcessor(UObject item);

    /// <summary>
    /// Register a handler that processes items put on the shelf.
    /// </summary>
    /// <param name="processor">Handler to register</param>
    /// <param name="priority"><Priority of the handler (higher = called first)/param>
    public static void RegisterItemProcessor(ShelfItemProcessor processor, int priority = 0)
    {
        itemProcessors ??= new();
        itemProcessors.RemoveAll(e => e.processor == processor);
        itemProcessors.Add(new ItemProcessor() {
            processor = processor,
            priority = priority,
        });
        itemProcessors.Sort((a, b) => b.priority.CompareTo(a.priority));
    }

    /// <summary>
    /// Unregister a shelf item processor.
    /// </summary>
    public static void RemoveItemProcessor(ShelfItemProcessor processor)
    {
        if (itemProcessors == null) return;
        itemProcessors.RemoveAll(h => h.processor == processor);
    }

    // ---------- Click Handlers ----------

    /// <summary>
    /// Delegate for shelf item click handlers.
    /// </summary>
    /// <param name="item">The information about the item to act upon</param>
    /// <returns>`true` if the click is handled, `false` to fall back to another handler</returns>
    public delegate bool ShelfItemAction(ShelfItemEvent item);

    /// <summary>
    /// Register a handler to process clicks on shelf items.
    /// </summary>
    /// <param name="handler">Handler to register</param>
    /// <param name="eventMask">Mask to filter which events the handler gets called for</param>
    /// <param name="priority">Priority of the handler (higher = called first)</param>
    public static void RegisterAction(ShelfItemAction handler, ShelfItemEventType eventMask = ShelfItemEventType.Default, int priority = 0)
    {
        actionHandlers ??= new();
        actionHandlers.RemoveAll(e => e.handler == handler);
        actionHandlers.Add(new ActionHandler() {
            handler = handler,
            eventMask = eventMask,
            priority = priority,
        });
        actionHandlers.Sort((a, b) => b.priority.CompareTo(a.priority));
    }

    /// <summary>
    /// Unregister a shelf item click handler.
    /// </summary>
    public static void RemoveAction(ShelfItemAction handler)
    {
        if (actionHandlers == null) return;
        actionHandlers.RemoveAll(h => h.handler == handler);
    }

    // ---------- Context Menus ----------

    /// <summary>
    /// Add a context menu action for items of the given type.
    /// </summary>
    /// <remarks>
    /// This overload takes an <paramref name="action"/> with a single item as 
    /// parameter. If <paramref name="multiSelection"/> is enabled, the action
    /// will be called multiple times for each selected item.
    /// </remarks>
    /// <typeparam name="TItem">The item type to add the action for</typeparam>
    /// <param name="name">The name of the action in the context menu</param>
    /// <param name="action">The delegate to execute the action</param>
    /// <param name="status">Delegate called to determine the status of the menu item</param>
    /// <param name="multiSelection">Wether to include the action when multiple items are selected</param>
    /// <param name="sortOrder">Order influencing how the action is sorted with other actions</param>
    public static void RegisterContextMenuAction<TItem>(string name, Action<TItem> action, Func<TItem, DropdownMenuAction.Status> status = null, bool multiSelection = true, int sortOrder = 0)
        where TItem : UObject
    {
        RegisterContextMenuActionInternal(name, action, null, status, multiSelection, sortOrder);
    }

    /// <summary>
    /// Add a context menu action for items of the given type.
    /// </summary>
    /// <remarks>
    /// This overload takes a <paramref name="multiAction"/> with an enumerable
    /// of all selected items. The action will only be called once and
    /// multi-selection is always enabled.
    /// </remarks>
    /// <typeparam name="TItem">The item type to add the action for</typeparam>
    /// <param name="name">The name of the action in the context menu</param>
    /// <param name="multiAction">The delegate to execute the action</param>
    /// <param name="status">Delegate called to determine the status of the menu item</param>
    /// <param name="sortOrder">Order influencing how the action is sorted with other actions</param>
    public static void RegisterContextMenuAction<TItem>(string name, Action<IEnumerable<TItem>> multiAction, Func<TItem, DropdownMenuAction.Status> status = null, int sortOrder = 0)
        where TItem : UObject
    {
        RegisterContextMenuActionInternal(name, null, multiAction, status, true, sortOrder);
    }

    static void RegisterContextMenuActionInternal<TItem>(string name, Action<TItem> action, Action<IEnumerable<TItem>> multiAction, Func<TItem, DropdownMenuAction.Status> status, bool multiSelection, int sortOrder)
        where TItem : UObject
    {
        dropdownActions ??= new();
        dropdownActions.RemoveAll(e => ReferenceEquals(e.originalAction, action));

        Func<UObject, DropdownMenuAction.Status> statusWrapper = null;
        if (status != null) {
            statusWrapper = item => status((TItem)item);
        }

        var entry = new DropdownAction() {
            name = name,
            status = statusWrapper,
            multiSelection = multiSelection,
            sortOrder = sortOrder,
        };

        if (action != null) {
            entry.originalAction = action;
            entry.action = item => action((TItem)item);
        } else if (multiAction != null) {
            entry.originalAction = multiAction;
            entry.multiAction = items => multiAction(items.OfType<TItem>());
        }

        dropdownActions.Add(entry);
        dropdownActions.Sort((a, b) => b.sortOrder.CompareTo(a.sortOrder));
    }

    /// <summary>
    /// Remove a previously registered context menu action.
    /// </summary>
    public static void RemoveContextMenuAction<TItem>(Action<TItem> action)
        where TItem : UObject
    {
        if (dropdownActions == null) return;
        dropdownActions.RemoveAll(e => ReferenceEquals(e.originalAction, action));
    }

    /// <summary>
    /// Delegate for context menu populators.
    /// </summary>
    /// <param name="item">The shelf item to populate the menu for (this could be an <see cref="IShelfItem"/> wrapper.</param>
    /// <param name="populateEvent">The populate event to use for creating the menu</param>
    public delegate void ContextMenuPopulator(IEnumerable<ShelfItem> items, ContextualMenuPopulateEvent populateEvent);

    /// <summary>
    /// Register a listener for the context menu population event.
    /// </summary>
    /// <param name="populator">Callback called when a context menu is built</param>
    /// <param name="priority">Priority of this populator (higher = called first)</param>
    public static void RegisterContextMenuPopulator(ContextMenuPopulator populator, int priority = 0)
    {
        dropdownPopulators ??= new();
        dropdownPopulators.RemoveAll(e => e.populator == populator);
        dropdownPopulators.Add(new() {
            populator = populator,
            priority = priority,
        });
        dropdownPopulators.Sort((a, b) => b.priority.CompareTo(a.priority));
    }

    /// <summary>
    /// Remove a context menu populator.
    /// </summary>
    public static void RemoveContextMenuPopulator(ContextMenuPopulator populator)
    {
        if (dropdownPopulators == null) return;
        dropdownPopulators.RemoveAll(e => e.populator == populator);
    }

    // ---------- Misc ----------

    /// <summary>
    /// Handle items that are wrapped using <see cref="IShelfItem"/>.
    /// Returns the item as-is if it is not wrapped.
    /// If it is wrapped, returns <see cref="IShelfItem.Reference"/>
    /// and sets <paramref name="wrapper"/> to the cast wrapper.
    /// </summary>
    /// <param name="item">The item to unwrap</param>
    /// <param name="wrapper">The wrapper if the item was wrapped, null otherwise</param>
    /// <returns>The original unwrapped item or the wrapped item</returns>
    public static UObject UnwrapShelfItem(UObject item, out IShelfItem wrapper)
    {
        wrapper = null;

        if (item is IShelfItem shelfItem) {
            wrapper = shelfItem;
            return wrapper.Reference;
        }

        return item;
    }

    // ---------- Internals ----------

    /// <summary>
    /// Run the shelf item processors for the given item.
    /// </summary>
    public static UObject ProcessShelfItem(UObject item)
    {
        if (ReferenceEquals(item, null))
            return null;

        if (item is IShelfItem shelfItem && shelfItem.SaveInRackAsset) {
            // Clone IShelfItem contained inside racks
            item = shelfItem.Clone();
        }

        if (itemProcessors != null) {
            // Send unity objects through item processors
            foreach (var entry in itemProcessors) {
                if (entry.processor == null)
                    continue;

                try {
                    item = entry.processor(item);
                } catch (Exception e) {
                    Debug.LogException(e);
                }

                if (item == null)
                    return null;
            }
        }

        if (!EditorUtility.IsPersistent(item) && item is not IShelfItem) {
            Debug.LogWarning($"The Shelf: Added non-persitent item '{item.name}' ('{item.GetType().Name}') to a shelf. "
                + $"This item will probably not survive domain reload and become invalid.");
        }

        return item;
    }

    /// <summary>
    /// Run the shelf item processors for the given list of references.
    /// </summary>
    public static IEnumerable<UObject> ProcessShelfItems(IEnumerable<UObject> items)
    {
        if (items == null)
            yield break;

        foreach (var obj in items) {
            var item = ProcessShelfItem(obj);
            if (item != null) {
                yield return item;
            }
        }
    }

    /// <summary>
    /// Call the click handlers with the given item,
    /// until the first handler returns `true`.
    /// </summary>
    public static bool ProcessAction(ShelfItemEvent evt)
    {
        if (actionHandlers == null)
            return false;

        foreach (var entry in actionHandlers) {
            if (entry.handler == null) continue;

            // Check if any matching type flags are set in the mask
            var typeMask = entry.eventMask & ShelfItemEventType.TypeAny;
            if ((evt.eventType & typeMask) == ShelfItemEventType.None)
                continue;

            // Check if matching modifier flags are set, default to ModifierNone
            var modifierMask = entry.eventMask & ShelfItemEventType.ModifierAny;
            if (modifierMask == ShelfItemEventType.None)
                modifierMask = ShelfItemEventType.ModifierNone;
            if ((evt.eventType & modifierMask) == ShelfItemEventType.None)
                continue;

            try {
                if (entry.handler(evt)) {
                    return true;
                }
            } catch (Exception e) {
                Debug.LogException(e);
            }
        }

        return false;
    }

    /// <summary>
    /// Call the context menu populators with the given item.
    /// </summary>
    public static void ProcessContextMenu(IEnumerable<ShelfItem> items, ContextualMenuPopulateEvent args)
    {
        if (dropdownPopulators == null)
            return;

        foreach (var pop in dropdownPopulators) {
            try {
                pop.populator(items, args);
            } catch (Exception e) {
                Debug.LogException(e);
            }
        }
    }

    /// <summary>
    /// Add the context menu actions to the menu for the given item.
    /// </summary>
    public static void ProcessContextMenuActions(IEnumerable<ShelfItem> items, ContextualMenuPopulateEvent args)
    {
        if (dropdownActions == null)
            return;

        var itemCount = items.Count();
        var addedSeparator = false;
        foreach (var action in dropdownActions) {
            if (itemCount > 1 && !action.multiSelection)
                continue;

            Type actionType = null;
            if (action.action != null) {
                // Extract type from Action<T>
                actionType = action.originalAction.GetType().GenericTypeArguments[0];
            } else {
                // Extract type from Action<IEnumerable<T>>
                actionType = action.originalAction.GetType().GenericTypeArguments[0].GenericTypeArguments[0];
            }
            var matchingReferences = items.Select(i => i.reference)
                .Where(r => r != null && actionType.IsAssignableFrom(r.GetType()))
                .ToList();

            var status = DropdownMenuAction.Status.None;
            for (int i = matchingReferences.Count - 1; i >= 0; i--) {
                var itemStatus = action.status != null
                    ? action.status(matchingReferences[i])
                    : DropdownMenuAction.Status.Normal;
                if (itemCount > 1 &&
                        (itemStatus == DropdownMenuAction.Status.None 
                        || itemStatus.HasFlag(DropdownMenuAction.Status.Hidden)
                        || itemStatus.HasFlag(DropdownMenuAction.Status.Disabled))) {
                    matchingReferences.RemoveAt(i);
                    continue;
                }
                status |= itemStatus;
            }

            if (matchingReferences.Count == 0)
                continue;

            if (!addedSeparator) {
                addedSeparator = true;
                args.menu.AppendSeparator();
            }

            var menuName = action.name;
            if (itemCount > 1) {
                menuName = $"{menuName} ({matchingReferences.Count} item{(matchingReferences.Count > 1 ? "s" : "")})";
            }
            Action<DropdownMenuAction> menuAction = a => {
                if (action.multiAction != null) {
                    action.multiAction(matchingReferences);
                } else {
                    foreach (var reference in matchingReferences) {
                        action.action(reference);
                    }
                }
            };
            Func<DropdownMenuAction, DropdownMenuAction.Status> menuStatus = a => status;

            args.menu.AppendAction(menuName, menuAction, a => status);
        }
    }

    static ShelfApi()
    {
        // Register defaults, these lists need to be sorted by descending priority
        itemProcessors = new() {
            new() { processor = ShelfDefaultHandlers.ItemProcessorPrefabObject,             priority =  -100 },
            new() { processor = ShelfDefaultHandlers.ItemProcessorSceneObject,              priority =  -101 },
            new() { processor = ShelfDefaultHandlers.ItemProcessorNonPersistent,            priority =  -102 },
        };
        actionHandlers = new() {
            new() { handler =   ShelfDefaultHandlers.ActionOpenObjectInsideScene,           priority =  -100, eventMask = ShelfItemEventType.TypeOpen | ShelfItemEventType.ModifierAny },
            new() { handler =   ShelfDefaultHandlers.ActionOpenObjectInsidePrefab,          priority =  -100, eventMask = ShelfItemEventType.TypeOpen },
            new() { handler =   ShelfDefaultHandlers.ActionSelectFolder,                    priority =  -100, eventMask = ShelfItemEventType.TypeClick | ShelfItemEventType.TypeOpen },
            new() { handler =   ShelfDefaultHandlers.ActionSelect,                          priority = -9998, eventMask = ShelfItemEventType.TypeClick },
            new() { handler =   ShelfDefaultHandlers.ActionSelectContainer,                 priority = -9999, eventMask = ShelfItemEventType.TypeClick },
            new() { handler =   ShelfDefaultHandlers.ActionOpenSceneAdditively,             priority = -9999, eventMask = ShelfItemEventType.TypeOpen | ShelfItemEventType.ModifierAlt },
            new() { handler =   ShelfDefaultHandlers.ActionOpen,                            priority = -9999, eventMask = ShelfItemEventType.TypeOpen },
        };
        dropdownPopulators = new() {
            new() { populator = ShelfDefaultHandlers.ContextMenuPopulatorItemManagement,    priority =   100 },
            new() { populator = ShelfDefaultHandlers.ContextMenuPopulatorActions,           priority =  -100 },
        };

        // Use the method for actions because it generates wrappers
        RegisterContextMenuAction<UObject>(
            "Ping", 
            ShelfDefaultHandlers.ContextMenuActionPing,
            multiSelection: false
        );
        RegisterContextMenuAction<UObject>(
            "Select", 
            ShelfDefaultHandlers.ContextMenuActionSelect
        );
        RegisterContextMenuAction<UObject>(
            "Open", 
            ShelfDefaultHandlers.ContextMenuActionOpenAsset,
            ShelfDefaultHandlers.ContextMenuActionOpenAssetStatus
        );
        RegisterContextMenuAction<UObject>(
            Environment.OSVersion.Platform == PlatformID.MacOSX
                    || Environment.OSVersion.Platform == PlatformID.Unix
                ? "Reveal in Finder"
                : "Show in Explorer", 
            ShelfDefaultHandlers.ContextMenuActionRevealInFinder,
            ShelfDefaultHandlers.ContextMenuActionRevealInFinderStatus
        );
        RegisterContextMenuAction<SceneAsset>(
            "Open Single", 
            ShelfDefaultHandlers.ContextMenuActionOpenSceneSingle
        );
        RegisterContextMenuAction<SceneAsset>(
            "Open Additive", 
            ShelfDefaultHandlers.ContextMenuActionOpenSceneAdditive
        );
        RegisterContextMenuAction<UObject>(
            "Properties...", 
            ShelfDefaultHandlers.ContextMenuActionProperties,
            ShelfDefaultHandlers.ContextMenuActionPropertiesStatus
        );
        RegisterContextMenuAction<MonoScript>(
            "Add Component", 
            ShelfDefaultHandlers.ContextMenuActionAddComponent,
            ShelfDefaultHandlers.ContextMenuActionAddComponentStatus
        );
        RegisterContextMenuAction<MonoScript>(
            "Create Asset", 
            ShelfDefaultHandlers.ContextMenuActionCreateAsset,
            ShelfDefaultHandlers.ContextMenuActionCreateAssetStatus
        );
    }

    struct ItemProcessor
    {
        public ShelfItemProcessor processor;
        public int priority;
    }

    struct ActionHandler
    {
        public ShelfItemAction handler;
        public ShelfItemEventType eventMask;
        public int priority;
    }

    struct DropdownAction
    {
        public string name;
        public MulticastDelegate originalAction;
        public Action<UObject> action;
        public Action<IEnumerable<UObject>> multiAction;
        public Func<UObject, DropdownMenuAction.Status> status;
        public bool multiSelection;
        public int sortOrder;
    }

    struct DropdownPopulator
    {
        public ContextMenuPopulator populator;
        public int priority;
    }

    static List<ItemProcessor> itemProcessors;
    static List<ActionHandler> actionHandlers;
    static List<DropdownAction> dropdownActions;
    static List<DropdownPopulator> dropdownPopulators;
}

/// <summary>
/// Default handlers pre-registered with the <see cref="ShelfApi"/>.
/// </summary>
public static class ShelfDefaultHandlers
{
    // ---------- Item Processors ----------

    /// <summary>
    /// Item processor that wraps non-persistent refernces so that
    /// a warning can be displayed.
    /// </summary>
    public static UObject ItemProcessorNonPersistent(UObject item)
    {
        if (EditorUtility.IsPersistent(item) || item is IShelfItem)
            return item;

        var wrapper = ScriptableObject.CreateInstance<ShelfNonPersistentObject>();
        wrapper.Reference = item;

        return wrapper;
    }

    /// <summary>
    /// Item processor that wraps scene references in <see cref="ShelfSceneObject"/>,
    /// so they can remain on the shelf permanently.
    /// </summary>
    public static UObject ItemProcessorSceneObject(UObject item)
    {
        if (EditorUtility.IsPersistent(item) || item is IShelfItem)
            return item;

        var wrapper = ScriptableObject.CreateInstance<ShelfSceneObject>();
        wrapper.Reference = item;
        if (!wrapper.IsSet)
            return item;

        return wrapper;
    }

    /// <summary>
    /// Item processor that wraps game objects and components from inside prefabs
    /// in <see cref="ShelfPrefabObject"/>.
    /// Note that dragging those objects will only drag a reference to the 
    /// root prefab asset, only clicking jumps to the child inside the prefab.
    /// </summary>
    public static UObject ItemProcessorPrefabObject(UObject item)
    {
        // Check reference is a game object or component
        GameObject gameObject;
        if (item is GameObject go) {
            gameObject = go;
        } else if (item is Component comp) {
            gameObject = comp.gameObject;
        } else {
            return item;
        }

        // Check we're editing a prefab and reference is part of it
        var stage = PrefabStageUtility.GetCurrentPrefabStage();
        if (stage?.IsPartOfPrefabContents(gameObject) != true)
            return item;

        // Try to get guid and file id
        var guid = AssetDatabase.AssetPathToGUID(stage.assetPath);
        if (string.IsNullOrEmpty(guid)) {
            EditorUtility.DisplayDialog(
                "The Shelf: Failed to Put Prefab Object",
                $"'{item.name}' is part of a prefab stage but could not get GUID for its asset path ('{stage.assetPath}')",
                "OK"
            );
            return null;
        }

        var fileId = ShelfPrefabObject.GetOrGenerateFileIDHint(item);
        if (fileId == 0) {
            EditorUtility.DisplayDialog(
                "Failed to Put Prefab Object",
                $"'{item.name}' is part of a prefab stage but it does not have a File ID.\n\n"
                + $"This might be because it's a new object that hasn't been saved before.\n\n"
                + $"Save the prefab and try again.",
                "OK"
            );
            return null;
        }

        var wrapper = ScriptableObject.CreateInstance<ShelfPrefabObject>();
        wrapper.Assign(guid, fileId);

        return wrapper;
    }

    // ---------- Action Handlers ----------

    /// <summary>
    /// Handler that selects the clicked item.
    /// </summary>
    public static bool ActionSelect(ShelfItemEvent evt)
    {
        if (evt.item.reference == null)
            return false;

        Selection.activeObject = evt.item.reference;
        return true;
    }

    /// <summary>
    /// Handler that selects the container asset of <see cref="IShelfItem"/>.
    /// In case the item's reference isn't loaded.
    /// </summary>
    public static bool ActionSelectContainer(ShelfItemEvent evt)
    {
        if (evt.item.shelfItem?.ContainerAsset == null)
            return false;

        Selection.activeObject = evt.item.shelfItem.ContainerAsset;
        return true;
    }

    /// <summary>
    /// Handler that opens the clicked item (either in Unity or in an external editor).
    /// </summary>
    public static bool ActionOpen(ShelfItemEvent evt)
    {
        if (evt.item.reference == null)
            return false;

        return AssetDatabase.OpenAsset(evt.item.reference);
    }

    /// <summary>
    /// Handler that opens a scene additively.
    /// The <see cref="ActionOpen"/> already handles opening scenes in single mode.
    /// </summary>
    public static bool ActionOpenSceneAdditively(ShelfItemEvent evt)
    {
        if (evt.item.reference is not SceneAsset sceneAsset)
            return false;

        var result = OpenSceneWithMode(OpenSceneMode.Additive, sceneAsset);
        return result != OpenSceneResult.Failure;
    }

    /// <summary>
    /// Action that selects a folder in project view.
    /// </summary>
    /// <remarks>
    /// With an open event type, the action also opens and focuses the project tab.
    /// This action does extra handling to try to not only select the folder
    /// but also show its contents in both one- and two-column layouts.
    /// </remarks>
    public static bool ActionSelectFolder(ShelfItemEvent evt)
    {
        if (evt.item.reference == null)
            return false;

        // Verify asset is a folder
        var path = AssetDatabase.GetAssetPath(evt.item.reference);
        if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
            return false;

        var error = ProjectBrowserUtility.GetLastInteracted(out var browser);
        if (error != null) {
            error.LogOnce();
            return false;
        }

        if (browser == null) {
            if ((evt.eventType & ShelfItemEventType.TypeOpen) != 0) {
                // Open project tab with double-click
                ProjectBrowserUtility.Open();
            } else {
                // Nothing more to do for single-click
                return true;
            }
        }

        // Focus project tab with double-click
        if ((evt.eventType & ShelfItemEventType.TypeOpen) != 0) {
            EditorUtility.FocusProjectWindow();
        }

        // Reveal folder contents
        // Must be called after FocusProjectWindow for two-column layout,
        // otherwise the focus overwrites the show folder conents
        error = ProjectBrowserUtility.ShowFolderContents(evt.item.reference.GetInstanceID());
        if (error != null) {
            error.LogOnce();
            return false;
        }

        return true;
    }

    /// <summary>
    /// Handler that opens scenes and selects scene items inside of them.
    /// </summary>
    public static bool ActionOpenObjectInsideScene(ShelfItemEvent evt)
    {
        // Scene is loaded, let other actions process reference
        if (evt.item.reference != null)
            return false;

        if (evt.item.shelfItem?.ContainerAsset is not SceneAsset sceneAsset)
            return false;

        if (!evt.eventType.HasFlag(ShelfItemEventType.ModifierAlt)) {
            if (!AssetDatabase.OpenAsset(sceneAsset))
                return true;
        } else {
            var result = OpenSceneWithMode(OpenSceneMode.Additive, sceneAsset);
            if (result != OpenSceneResult.Success)
                return result != OpenSceneResult.Failure;
        }

        var reference = evt.item.shelfItem.Reference;
        if (reference == null) {
            Debug.LogWarning($"TheShelf: Could not resolve reference after opening scene (looking for '{evt.item.shelfItem.Name}' in '{sceneAsset.name}')");
        } else {
            Selection.activeObject = reference;
        }

        return true;
    }

    /// <summary>
    /// Handler that opens prefabs and selects child items inside of them.
    /// </summary>
    public static bool ActionOpenObjectInsidePrefab(ShelfItemEvent evt)
    {
        if (evt.item.shelfItem is not ShelfPrefabObject prefabObject)
            return false;

        if (prefabObject.ContainerAsset == null)
            return true;

        var path = AssetDatabase.GetAssetPath(prefabObject.ContainerAsset);
        if (string.IsNullOrEmpty(path))
            return true;

        var stage = PrefabStageUtility.OpenPrefab(path);
        if (stage == null)
            return true;

        var child = ShelfPrefabObject.FindFileIdentifierInPrefabContents(stage.prefabContentsRoot, prefabObject.LocalFileId);
        if (child == null) {
            Debug.LogWarning($"The Shelf: Could not find child with local file identifier '{prefabObject.LocalFileId}' inside prefab '{prefabObject.ContainerAsset}'", prefabObject.ContainerAsset);
            return true;
        }

        Selection.activeObject = child;
        return true;
    }

    // ---------- Context Menus ----------

    /// <summary>
    /// Context menu populator that adds item management options.
    /// </summary>
    public static void ContextMenuPopulatorItemManagement(IEnumerable<ShelfItem> items, ContextualMenuPopulateEvent populateEvent)
    {
        var sourceView = items.FirstOrDefault()?.itemsView;
        if (sourceView == null) return;

        var menu = populateEvent.menu;
        menu.AppendAction("Remove", a => {
            // Sort items in descending index order to remove them from the back 
            // and not shift indexes during removal
            var toRemove = items.ToList();
            toRemove.Sort((a, b) => b.itemIndex.CompareTo(a.itemIndex));
            foreach (var item in toRemove) {
                sourceView.Shelf.Items.RemoveAt(item.itemIndex);
            }
            // Deselect after removing items
            sourceView.selectedIndex = -1;
        });

        foreach (var shelf in sourceView.Rack.Shelves) {
            if (sourceView.Shelf == shelf)
                continue;

            menu.AppendAction($"Move to Shelf/{shelf.Name}", a => {
                // First add items to new shelf in original order
                foreach (var item in items) {
                    if (item.shelfItem != null) {
                        shelf.Items.Add((UObject)item.shelfItem);
                    } else {
                        shelf.Items.Add(item.reference);
                    }
                }

                // Sort items in descending index order to remove them from the back 
                // and not shift indexes during removal
                var toMove = items.ToList();
                toMove.Sort((a, b) => b.itemIndex.CompareTo(a.itemIndex));
                foreach (var item in toMove) {
                    sourceView.Shelf.Items.RemoveAt(item.itemIndex);
                }

                // Deselect after removing items
                sourceView.selectedIndex = -1;
            });
        }

        if (items.Count() == 1 && items.First().shelfItem is UObject wrapperObject) {
            menu.AppendAction("Inspect Wrapper", a => {
                Selection.activeObject = wrapperObject;
            });
        }
    }

    /// <summary>
    /// Context menu populator that adds the API item actions.
    /// </summary>
    public static void ContextMenuPopulatorActions(IEnumerable<ShelfItem> items, ContextualMenuPopulateEvent populateEvent)
    {
        ShelfApi.ProcessContextMenuActions(items, populateEvent);
    }

    // ---------- Context Actions ----------

    /// <summary>
    /// Context menu action to ping an asset or scene object.
    /// </summary>
    public static void ContextMenuActionPing(UObject item)
    {
        EditorGUIUtility.PingObject(item);
    }

    /// <summary>
    /// Context menu action to select an asset or scene object.
    /// </summary>
    public static void ContextMenuActionSelect(IEnumerable<UObject> item)
    {
        Selection.objects = item.ToArray();
    }

    /// <summary>
    /// Status callback for action to ping asset.
    /// </summary>
    public static DropdownMenuAction.Status ContextMenuActionPropertiesStatus(UObject item)
    {
        return DropdownMenuAction.Status.Normal;
    }

    /// <summary>
    /// Context menu action to ping an asset or scene object.
    /// </summary>
    public static void ContextMenuActionProperties(UObject item)
    {
        EditorUtility.OpenPropertyEditor(item);
    }

    /// <summary>
    /// Status callback for action to open asset.
    /// </summary>
    public static DropdownMenuAction.Status ContextMenuActionOpenAssetStatus(UObject item)
    {
        if (!AssetDatabase.Contains(item))
            return DropdownMenuAction.Status.None;

        return DropdownMenuAction.Status.Normal;
    }

    /// <summary>
    /// Context menu action to open asset (to edit in Unity or external editor).
    /// </summary>
    public static void ContextMenuActionOpenAsset(UObject item)
    {
        AssetDatabase.OpenAsset(item);
    }

    /// <summary>
    /// Status callback for action to reveal asset in Finder / Explorer.
    /// </summary>
    public static DropdownMenuAction.Status ContextMenuActionRevealInFinderStatus(UObject item)
    {
        if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(item)))
            return DropdownMenuAction.Status.None;

        return DropdownMenuAction.Status.Normal;
    }

    /// <summary>
    /// Context menu action to reveal asset in Finder / Explorer.
    /// </summary>
    public static void ContextMenuActionRevealInFinder(UObject item)
    {
        EditorUtility.RevealInFinder(AssetDatabase.GetAssetPath(item));
    }

    /// <summary>
    /// Status callback for action to add scripts to the selected game object.
    /// </summary>
    public static DropdownMenuAction.Status ContextMenuActionAddComponentStatus(MonoScript item)
    {
        // Hide if script is not a Component
        var type = item.GetClass();
        if (!typeof(Component).IsAssignableFrom(type))
            return DropdownMenuAction.Status.None;

        // Disable if no target GO selected
        if (Selection.activeGameObject == null)
            return DropdownMenuAction.Status.Disabled;

        return DropdownMenuAction.Status.Normal;
    }

    /// <summary>
    /// Context menu action to add scripts to the selected game object.
    /// </summary>
    public static void ContextMenuActionAddComponent(MonoScript item)
    {
        var scriptType = item.GetClass();
        Undo.AddComponent(Selection.activeGameObject, scriptType);
    }

    /// <summary>
    /// Status callback for action to create assets of a scriptable object.
    /// </summary>
    public static DropdownMenuAction.Status ContextMenuActionCreateAssetStatus(MonoScript item)
    {
        // Hide if script is not a ScriptableObject
        var type = item.GetClass();
        if (!typeof(ScriptableObject).IsAssignableFrom(type))
            return DropdownMenuAction.Status.None;

        return DropdownMenuAction.Status.Normal;
    }

    /// <summary>
    /// Context menu action to create assets of a scriptable object.
    /// </summary>
    public static void ContextMenuActionCreateAsset(MonoScript item)
    {
        var scriptType = item.GetClass();
        var instance = ScriptableObject.CreateInstance(scriptType);

        string fileName;
        var attr = scriptType.GetCustomAttribute<CreateAssetMenuAttribute>(true);
        if (attr != null && attr.fileName != null) {
            fileName = attr.fileName;
        } else {
            fileName = $"{scriptType.Name}.asset";
        }

        ProjectWindowUtil.CreateAsset(instance, fileName);
    }

    /// <summary>
    /// Open the scene in single mode.
    /// </summary>
    public static void ContextMenuActionOpenSceneSingle(SceneAsset item)
    {
        OpenSceneWithMode(OpenSceneMode.Single, item);
    }

    /// <summary>
    /// Open the scene in additive mode.
    /// </summary>
    public static void ContextMenuActionOpenSceneAdditive(SceneAsset item)
    {
        OpenSceneWithMode(OpenSceneMode.Additive, item);
    }

    // ---------- Helpers ----------

    /// <summary>
    /// Open a `SceneAsset` with the given mode, asking the user
    /// if modified scenes should be saved if the mode is `Single`.
    /// </summary>
    /// <param name="mode">Mode to open the scene with</param>
    /// <param name="sceneAsset">Scene asset to open</param>
    /// <returns>Wether opening the scene was successful</returns>
    public static OpenSceneResult OpenSceneWithMode(OpenSceneMode mode, SceneAsset sceneAsset)
    {
        if (mode == OpenSceneMode.Single && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return OpenSceneResult.Canceled;

        var scenePath = AssetDatabase.GetAssetPath(sceneAsset);
        if (string.IsNullOrEmpty(scenePath)) {
            Debug.LogError($"TheShelf: Could not get asset path for scene asset '{sceneAsset}'", sceneAsset);
            return OpenSceneResult.Failure;
        }

        return EditorSceneManager.OpenScene(scenePath, mode).IsValid()
            ? OpenSceneResult.Success
            : OpenSceneResult.Failure;
    }

    /// <summary>
    /// Result of <see cref="OpenSceneWithMode"/>.
    /// </summary>
    public enum OpenSceneResult
    {
        /// <summary>
        /// Scene was opened successfully.
        /// </summary>
        Success,
        /// <summary>
        /// The user has canceled opening the scene (when overwriting unsaved changes).
        /// </summary>
        Canceled,
        /// <summary>
        /// There was an error opening the scene.
        /// </summary>
        Failure
    }
}

/// <summary>
/// Utility to interact with the project browser,
/// especially to reveal the contents of a folder.
/// </summary>
/// <remarks>
/// The project browser is very old and very private, not
/// even its window type is public so that it can be opened.
/// 
/// This class calls internal APIs using reflection. It caches
/// the lookups and handles errors
/// </remarks>
public static class ProjectBrowserUtility
{
    /// <summary>
    /// Open the project browser window.
    /// </summary>
    /// <returns></returns>
    public static ReflectionError Open()
    {
        var error = ReflectBasic();
        if (error != null) return error;

        var browser = ScriptableObject.CreateInstance(projectBrowserType) as EditorWindow;
        browser.Show();

        return null;
    }

    /// <summary>
    /// Get the project browser instance last interacted with,
    /// or null if no project browser is open.
    /// </summary>
    /// <returns>Wether the internal calls where successful</returns>
    public static ReflectionError GetLastInteracted(out EditorWindow projectBrowser)
    {
        projectBrowser = null;

        var error = ReflectLastInteracted();
        if (error != null) return error;

        projectBrowser = lastInteractedProjectBrowserField.GetValue(null) as EditorWindow;
        return null;
    }

    /// <summary>
    /// Reveal a folder's contents in the project browser.
    /// </summary>
    /// <remarks>
    /// In one-column layout, this method will select the folder and expand it.
    /// In two-column layout, this method will select the folder in the left column
    /// and show its contents in the right column (without selecting it).
    /// </remarks>
    /// <returns></returns>
    public static ReflectionError ShowFolderContents(int folderInstanceID)
    {
        var error = ReflectShowFolder();
        if (error != null) return error;

        error = GetLastInteracted(out var browser);
        if (error != null) return error;

        var viewMode = (int)viewModeField.GetValue(browser);

        // One-column layout
        if (viewMode == 0) {
            Selection.activeObject = EditorUtility.InstanceIDToObject(folderInstanceID);

            var assetTree = assetTreeField.GetValue(browser);
            if (assetTree == null) {
                return ReflectionError.Create(out errorShowFolder, "Could not get ProjectBrowser.m_AssetTree value");
            }

            var data = dataProperty.GetValue(assetTree);
            if (data == null) {
                return ReflectionError.Create(out errorShowFolder, "Could not get TreeViewController.data value");
            }

            twoArguments[0] = folderInstanceID;
            twoArguments[1] = true;
            setExpandedMethod.Invoke(data, twoArguments);

        // Two-column layout
        } else {
            twoArguments[0] = folderInstanceID;
            twoArguments[1] = true;
            ShowFolderContentsMethod.Invoke(browser, twoArguments);
        }

        return null;
    }

    // ---------- Errors ----------

    /// <summary>
    /// Class representing reflection lookup errors.
    /// </summary>
    /// <remarks>
    /// This error object is cached and subsequent calls to the
    /// utility methods will return the same error object.
    /// Therefore, calls to <see cref="LogOnce"> will only write
    /// a log message the first time it's called, until the 
    /// domain is reloaded.
    /// </remarks>
    public class ReflectionError
    {
        /// <summary>
        /// Create a new error, assign it to the given variable and return it.
        /// </summary>
        public static ReflectionError Create(out ReflectionError assignTo, string message)
        {
            assignTo = new ReflectionError(message);
            return assignTo;
        }

        /// <summary>
        /// The error message.
        /// </summary>
        public string Message { get; private set; }
        /// <summary>
        /// Wether the error has been logged before.
        /// </summary>
        public bool HasLoggedError { get; private set; }

        public ReflectionError(string message)
        {
            Message = message;
            HasLoggedError = false;
        }

        /// <summary>
        /// Log the error, each time this method is called.
        /// </summary>
        public void Log()
        {
            if (Message == null) return;

            HasLoggedError = true;

            Debug.LogError(
                $"ProjectBrowserUtility: "
              + $"Failed to reflect required types or members, functionality might be limited\n"
              + Message);
        }

        /// <summary>
        /// Log the error, do not log again if called repeatedly.
        /// </summary>
        public void LogOnce()
        {
            if (Message == null) return;
            if (HasLoggedError) return;
            Log();
        }
    }

    // ---------- Internals ----------

    static Assembly editorAssembly;
    // internal class ProjectBrowser : EditorWindow, IHasCustomMenu, ISearchableContainer
    static Type projectBrowserType;
    // public static ProjectBrowser s_LastInteractedProjectBrowser;
    static FieldInfo lastInteractedProjectBrowserField;
    // ViewMode m_ViewMode (enum ViewMode { OneColumn, TwoColumns })
    static FieldInfo viewModeField;
    // void ShowFolderContents(int folderInstanceID, bool revealAndFrameInFolderTree)
    static MethodInfo ShowFolderContentsMethod;
    // TreeViewController m_AssetTree
    static FieldInfo assetTreeField;
    // internal class TreeViewController
    static Type treeViewControllerType;
    // public ITreeViewDataSource data { get; set; }
    static PropertyInfo dataProperty;
    // internal abstract class TreeViewDataSource : ITreeViewDataSource
    static Type treeViewDataSourceType;
    // virtual public bool SetExpanded(int id, bool expand)
    static MethodInfo setExpandedMethod;

    static object[] twoArguments;

    static ReflectionError errorBasic;
    static ReflectionError errorLastInteracted;
    static ReflectionError errorShowFolder;

    static ReflectionError ReflectBasic()
    {
        if (errorBasic != null)
            return errorBasic;

        editorAssembly = typeof(EditorWindow).Assembly;

        projectBrowserType = editorAssembly.GetType("UnityEditor.ProjectBrowser");
        if (projectBrowserType == null) {
            return ReflectionError.Create(out errorBasic, "Could not get ProjectBrowser type");
        }

        return null;
    }

    static ReflectionError ReflectLastInteracted()
    {
        if (errorLastInteracted != null)
            return errorLastInteracted;

        var error = ReflectBasic();
        if (error != null) return error;

        lastInteractedProjectBrowserField = projectBrowserType.GetField("s_LastInteractedProjectBrowser", BindingFlags.Public | BindingFlags.Static);
        if (lastInteractedProjectBrowserField == null) {
            return ReflectionError.Create(out errorLastInteracted, "Could not get ProjectBrowser.s_LastInteractedProjectBrowser field");
        }

        return null;
    }

    static ReflectionError ReflectShowFolder()
    {
        if (errorShowFolder != null)
            return errorShowFolder;

        var error = ReflectBasic();
        if (error != null) return error;

        viewModeField = projectBrowserType.GetField("m_ViewMode", BindingFlags.NonPublic | BindingFlags.Instance);
        if (viewModeField == null) {
            return ReflectionError.Create(out errorShowFolder, "Could not get ProjectBrowser.m_ViewMode field");
        }

        ShowFolderContentsMethod = projectBrowserType.GetMethod("ShowFolderContents", BindingFlags.NonPublic | BindingFlags.Instance);
        if (ShowFolderContentsMethod == null) {
            return ReflectionError.Create(out errorShowFolder, "Could not get ProjectBrowser.ShowFolderContents method");
        }

        assetTreeField = projectBrowserType.GetField("m_AssetTree", BindingFlags.NonPublic | BindingFlags.Instance);
        if (assetTreeField == null) {
            return ReflectionError.Create(out errorShowFolder, "Could not get ProjectBrowser.m_AssetTree field");
        }

        treeViewControllerType = editorAssembly.GetType("UnityEditor.IMGUI.Controls.TreeViewController");
        if (treeViewControllerType == null) {
            return ReflectionError.Create(out errorShowFolder, "Could not get TreeViewController type");
        }

        dataProperty = treeViewControllerType.GetProperty("data", BindingFlags.Public | BindingFlags.Instance);
        if (dataProperty == null) {
            return ReflectionError.Create(out errorShowFolder, "Could not get TreeViewController.data property");
        }

        treeViewDataSourceType = editorAssembly.GetType("UnityEditor.IMGUI.Controls.TreeViewDataSource");
        if (treeViewDataSourceType == null) {
            return ReflectionError.Create(out errorShowFolder, "Could not get TreeViewDataSource type");
        }

        setExpandedMethod = treeViewDataSourceType.GetMethod(
            "SetExpanded",
            BindingFlags.Public | BindingFlags.Instance,
            null,
            new[] { typeof(int), typeof(bool) },
            null
        );
        if (setExpandedMethod == null) {
            return ReflectionError.Create(out errorShowFolder, "Could not get TreeViewDataSource.SetExpanded(int,bool) method");
        }

        twoArguments = new object[2];

        return null;
    }
}

}

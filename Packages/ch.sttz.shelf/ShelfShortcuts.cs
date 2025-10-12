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

using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

using UObject = UnityEngine.Object;

namespace sttz.TheShelf {

/// <summary>
/// The Shelf menu shortcuts.
/// </summary>
public static class ShelfShortcuts
{
    // ---------- Helpers ----------

    /// <summary>
    /// Check if there's a Unity selection that can be put on a shelf.
    /// </summary>
    public static bool HasSelection()
    {
        return Selection.activeObject != null;
    }

    /// <summary>
    /// Get the target rack for shortcuts.
    /// Uses the rack of an opened and focused window
    /// or the active rack otherwise.
    /// </summary>
    public static ShelfRack GetTargetRack()
    {
        if (EditorWindow.focusedWindow is ShelfWindow shelfWindow) {
            return shelfWindow.Rack;
        }

        return ShelfSettings.Project.ActiveRack;
    }

    /// <summary>
    /// Check if a target rack exists and if it has a shelf with the given index.
    /// </summary>
    public static bool HasShelfInTargetRack(int index)
    {
        var rack = GetTargetRack();
        if (rack?.Shelves == null) return false;

        return index >= 0 && index < rack.Shelves.Count;
    }

    /// <summary>
    /// Open the shelf at the given index from the current target rack.
    /// </summary>
    public static void OpenShelf(int index)
    {
        var rack = GetTargetRack();
        if (index < 0 || index >= rack.Shelves.Count)
            return;

        OpenRackOnShelf(rack, index);
    }

    /// <summary>
    /// Open the settings tab fro the current target rack.
    /// </summary>
    public static void OpenRackSettings()
    {
        var rack = GetTargetRack();
        OpenRackOnShelf(rack, rack.Shelves.Count);
    }

    /// <summary>
    /// Put the current Unity selection on the rack at the given index from
    /// the current target rack.
    /// </summary>
    public static void PutSelectionOnShelf(int index)
    {
        var rack = GetTargetRack();
        if (index < 0 || index >= rack.Shelves.Count)
            return;

        var selection = Selection.GetFiltered(typeof(UObject), SelectionMode.Unfiltered);
        if (selection.Length == 0)
            return;

        var shelf = rack.Shelves[index];
        shelf.InsertItems(shelf.Items.Count, selection);
    }

    /// <summary>
    /// Open a rack and show the shelf at the given index.
    /// If <see cref="ShelfSettings.OpenActiveRackAsPopup"/> is enabled,
    /// the rack will be opened in a popup window.
    /// </summary>
    public static void OpenRackOnShelf(ShelfRack rack, int index)
    {
        ShelfWindow window;
        if (ShelfSettings.Project.OpenActiveRackAsPopup) {
            window = ShelfWindow.PopupRack(rack);
        } else {
            window = ShelfWindow.OpenRack(rack);
        }
        window.RackView.ActiveTabIndex = index;
        window.RackView.FocusActiveShelfItemsList();
    }

    /// <summary>
    /// Handle double-clicking on rack assets to open them in a window.
    /// </summary>
    [OnOpenAsset]
    static bool OnOpenAsset(int instanceID, int line)
    {
        var obj = EditorUtility.InstanceIDToObject(instanceID);
        if (obj is ShelfRack rack) {
            ShelfWindow.OpenRack(rack);
            return true;
        }

        return false;
    }

    // ---------- Rack Context Menu ----------

    const string ContextMenuToggleActiveRack = "CONTEXT/ShelfRack/Toggle Rack Active";

    /// <summary>
    /// Activate the rack of the focused rack window
    /// or the selected rack asset.
    /// </summary>
    [MenuItem(ContextMenuToggleActiveRack, false, 1000)]
    public static void ActivateRack(MenuCommand cmd)
    {
        var targetRack = cmd.context as ShelfRack;

        if (targetRack != null) {
            ShelfSettings.Project.ToggleRackActive(targetRack);
        }
    }

    [MenuItem(ContextMenuToggleActiveRack, true)]
    static bool ValidateActivateRack(MenuCommand cmd)
    {
        var targetRack = cmd.context as ShelfRack;
        return targetRack != null && ShelfSettings.Project.CanToggleRackActive(targetRack);
    }

    // ---------- Main Shortcuts ----------

    /// <summary>
    /// Base path of the shelf main menu.
    /// </summary>
    public const string TheShelfMenuPath = "Window/The Shelf/";
    /// <summary>
    /// Default modifier keys to use for open shortcuts (modifiers + number).
    /// </summary>
    public const string TheShelfOpenShortcut = " #";
    /// <summary>
    /// Default modifier keys to use for put selection on shelf shortcuts (modifiers + number).
    /// </summary>
    public const string TheShelfSelectionShortcut = " #&";

    const string MenuOpenUserRack = TheShelfMenuPath + "Open User Rack";

    /// <summary>
    /// Open a shelf window for the user rack.
    /// </summary>
    [MenuItem(MenuOpenUserRack, priority = 10)]
    public static void OpenUserRack()
    {
        ShelfWindow.OpenRack(ShelfSettings.UserRack);
    }

    const string MenuOpenActiveRack = TheShelfMenuPath + "Open Active Rack";

    /// <summary>
    /// Open a shelf window for the <see cref="ShelfSettings.ActiveRack"/>.
    /// </summary>
    [MenuItem(MenuOpenActiveRack, priority = 11)]
    public static void OpenActiveRack()
    {
        ShelfWindow.OpenRack(ShelfSettings.Project.ActiveRack);
    }

    const string MenuToggleActiveRack = TheShelfMenuPath + "Toggle Rack Active";

    /// <summary>
    /// Activate the rack of the focused rack window
    /// or the selected rack asset.
    /// </summary>
    [MenuItem(MenuToggleActiveRack, false, 50)]
    public static void ActivateRack()
    {
        ShelfRack targetRack = null;

        if (EditorWindow.focusedWindow is ShelfWindow shelfWindow) {
            targetRack = shelfWindow.Rack;
        } else if (Selection.activeObject is ShelfRack shelfRack) {
            targetRack = shelfRack;
        }

        if (targetRack != null) {
            ShelfSettings.Project.ToggleRackActive(targetRack);
        }
    }

    [MenuItem(MenuToggleActiveRack, true)]
    static bool ValidateActivateRack()
    {
        ShelfRack targetRack = null;

        if (EditorWindow.focusedWindow is ShelfWindow shelfWindow) {
            targetRack = shelfWindow.Rack;
        } else if (Selection.activeObject is ShelfRack shelfRack) {
            targetRack = shelfRack;
        }

        Menu.SetChecked(MenuToggleActiveRack, targetRack != null && ShelfSettings.Project.ActiveRack == targetRack);

        return targetRack != null && ShelfSettings.Project.CanToggleRackActive(targetRack);
    }

    const string MenuPopupActiveRack = TheShelfMenuPath + "Popup Active Rack";

    [MenuItem(MenuPopupActiveRack, false, 51)]
    public static void PopupActivateRack()
    {
        ShelfSettings.Project.OpenActiveRackAsPopup = !ShelfSettings.Project.OpenActiveRackAsPopup;
    }

    [MenuItem(MenuPopupActiveRack, true)]
    public static bool ValidatePopupActivateRack()
    {
        Menu.SetChecked(MenuPopupActiveRack, ShelfSettings.Project.OpenActiveRackAsPopup);
        return true;
    }

    // ---------- Shelf 1 Shortcuts ----------

    const string MenuOpenShelf1 = TheShelfMenuPath + "Shelf 1" + TheShelfOpenShortcut + "1";

    [MenuItem(MenuOpenShelf1, true)]
    static bool ValidateOpenShelf1()
    {
        return HasShelfInTargetRack(0);
    }

    [MenuItem(MenuOpenShelf1, false, 100)]
    static void OpenShelf1()
    {
        OpenShelf(0);
    }

    const string MenuPutSelectionOnShelf1 = TheShelfMenuPath + "Put Selection on Shelf 1" + TheShelfSelectionShortcut + "1";

    [MenuItem(MenuPutSelectionOnShelf1, true)]
    static bool ValidatePutSelectionOnShelf1()
    {
        return HasSelection() && HasShelfInTargetRack(0);
    }

    [MenuItem(MenuPutSelectionOnShelf1, false, 200)]
    static void PutSelectionOnShelf1()
    {
        PutSelectionOnShelf(0);
    }

    // ---------- Shelf 2 Shortcuts ----------

    const string MenuOpenShelf2 = TheShelfMenuPath + "Shelf 2" + TheShelfOpenShortcut + "2";

    [MenuItem(MenuOpenShelf2, true)]
    static bool ValidateOpenShelf2()
    {
        return HasShelfInTargetRack(1);
    }

    [MenuItem(MenuOpenShelf2, false, 100)]
    static void OpenShelf2()
    {
        OpenShelf(1);
    }

    const string MenuPutSelectionOnShelf2 = TheShelfMenuPath + "Put Selection on Shelf 2" + TheShelfSelectionShortcut + "2";

    [MenuItem(MenuPutSelectionOnShelf2, true)]
    static bool ValidatePutSelectionOnShelf2()
    {
        return HasSelection() && HasShelfInTargetRack(1);
    }

    [MenuItem(MenuPutSelectionOnShelf2, false, 200)]
    static void PutSelectionOnShelf2()
    {
        PutSelectionOnShelf(1);
    }

    // ---------- Shelf 3 Shortcuts ----------

    const string MenuOpenShelf3 = TheShelfMenuPath + "Shelf 3" + TheShelfOpenShortcut + "3";

    [MenuItem(MenuOpenShelf3, true)]
    static bool ValidateOpenShelf3()
    {
        return HasShelfInTargetRack(2);
    }

    [MenuItem(MenuOpenShelf3, false, 100)]
    static void OpenShelf3()
    {
        OpenShelf(2);
    }

    const string MenuPutSelectionOnShelf3 = TheShelfMenuPath + "Put Selection on Shelf 3" + TheShelfSelectionShortcut + "3";

    [MenuItem(MenuPutSelectionOnShelf3, true)]
    static bool ValidatePutSelectionOnShelf3()
    {
        return HasSelection() && HasShelfInTargetRack(2);
    }

    [MenuItem(MenuPutSelectionOnShelf3, false, 200)]
    static void PutSelectionOnShelf3()
    {
        PutSelectionOnShelf(2);
    }

    // ---------- Shelf 4 Shortcuts ----------

    const string MenuOpenShelf4 = TheShelfMenuPath + "Shelf 4" + TheShelfOpenShortcut + "4";

    [MenuItem(MenuOpenShelf4, true)]
    static bool ValidateOpenShelf4()
    {
        return HasShelfInTargetRack(3);
    }

    [MenuItem(MenuOpenShelf4, false, 100)]
    static void OpenShelf4()
    {
        OpenShelf(3);
    }

    const string MenuPutSelectionOnShelf4 = TheShelfMenuPath + "Put Selection on Shelf 4" + TheShelfSelectionShortcut + "4";

    [MenuItem(MenuPutSelectionOnShelf4, true)]
    static bool ValidatePutSelectionOnShelf4()
    {
        return HasSelection() && HasShelfInTargetRack(3);
    }

    [MenuItem(MenuPutSelectionOnShelf4, false, 200)]
    static void PutSelectionOnShelf4()
    {
        PutSelectionOnShelf(3);
    }

    // ---------- Shelf 5 Shortcuts ----------

    const string MenuOpenShelf5 = TheShelfMenuPath + "Shelf 5" + TheShelfOpenShortcut + "5";

    [MenuItem(MenuOpenShelf5, true)]
    static bool ValidateOpenShelf5()
    {
        return HasShelfInTargetRack(4);
    }

    [MenuItem(MenuOpenShelf5, false, 100)]
    static void OpenShelf5()
    {
        OpenShelf(4);
    }

    const string MenuPutSelectionOnShelf5 = TheShelfMenuPath + "Put Selection on Shelf 5" + TheShelfSelectionShortcut + "5";

    [MenuItem(MenuPutSelectionOnShelf5, true)]
    static bool ValidatePutSelectionOnShelf5()
    {
        return HasSelection() && HasShelfInTargetRack(4);
    }

    [MenuItem(MenuPutSelectionOnShelf5, false, 200)]
    static void PutSelectionOnShelf5()
    {
        PutSelectionOnShelf(4);
    }

    // ---------- Shelf 6 Shortcuts ----------

    const string MenuOpenShelf6 = TheShelfMenuPath + "Shelf 6" + TheShelfOpenShortcut + "6";

    [MenuItem(MenuOpenShelf6, true)]
    static bool ValidateOpenShelf6()
    {
        return HasShelfInTargetRack(5);
    }

    [MenuItem(MenuOpenShelf6, false, 100)]
    static void OpenShelf6()
    {
        OpenShelf(5);
    }

    const string MenuPutSelectionOnShelf6 = TheShelfMenuPath + "Put Selection on Shelf 6" + TheShelfSelectionShortcut + "6";

    [MenuItem(MenuPutSelectionOnShelf6, true)]
    static bool ValidatePutSelectionOnShelf6()
    {
        return HasSelection() && HasShelfInTargetRack(5);
    }

    [MenuItem(MenuPutSelectionOnShelf6, false, 200)]
    static void PutSelectionOnShelf6()
    {
        PutSelectionOnShelf(5);
    }

    // ---------- Shelf 7 Shortcuts ----------

    const string MenuOpenShelf7 = TheShelfMenuPath + "Shelf 7" + TheShelfOpenShortcut + "7";

    [MenuItem(MenuOpenShelf7, true)]
    static bool ValidateOpenShelf7()
    {
        return HasShelfInTargetRack(6);
    }

    [MenuItem(MenuOpenShelf7, false, 100)]
    static void OpenShelf7()
    {
        OpenShelf(6);
    }

    const string MenuPutSelectionOnShelf7 = TheShelfMenuPath + "Put Selection on Shelf 7" + TheShelfSelectionShortcut + "7";

    [MenuItem(MenuPutSelectionOnShelf7, true)]
    static bool ValidatePutSelectionOnShelf7()
    {
        return HasSelection() && HasShelfInTargetRack(6);
    }

    [MenuItem(MenuPutSelectionOnShelf7, false, 200)]
    static void PutSelectionOnShelf7()
    {
        PutSelectionOnShelf(6);
    }

    // ---------- Shelf 8 Shortcuts ----------

    const string MenuOpenShelf8 = TheShelfMenuPath + "Shelf 8" + TheShelfOpenShortcut + "8";

    [MenuItem(MenuOpenShelf8, true)]
    static bool ValidateOpenShelf8()
    {
        return HasShelfInTargetRack(7);
    }

    [MenuItem(MenuOpenShelf8, false, 100)]
    static void OpenShelf8()
    {
        OpenShelf(7);
    }

    const string MenuPutSelectionOnShelf8 = TheShelfMenuPath + "Put Selection on Shelf 8" + TheShelfSelectionShortcut + "8";

    [MenuItem(MenuPutSelectionOnShelf8, true)]
    static bool ValidatePutSelectionOnShelf8()
    {
        return HasSelection() && HasShelfInTargetRack(7);
    }

    [MenuItem(MenuPutSelectionOnShelf8, false, 200)]
    static void PutSelectionOnShelf8()
    {
        PutSelectionOnShelf(7);
    }

    // ---------- Shelf 9 Shortcuts ----------

    const string MenuOpenShelf9 = TheShelfMenuPath + "Shelf 9" + TheShelfOpenShortcut + "9";

    [MenuItem(MenuOpenShelf9, true)]
    static bool ValidateOpenShelf9()
    {
        return HasShelfInTargetRack(8);
    }

    [MenuItem(MenuOpenShelf9, false, 100)]
    static void OpenShelf9()
    {
        OpenShelf(8);
    }

    const string MenuPutSelectionOnShelf9 = TheShelfMenuPath + "Put Selection on Shelf 9" + TheShelfSelectionShortcut + "9";

    [MenuItem(MenuPutSelectionOnShelf9, true)]
    static bool ValidatePutSelectionOnShelf9()
    {
        return HasSelection() && HasShelfInTargetRack(8);
    }

    [MenuItem(MenuPutSelectionOnShelf9, false, 200)]
    static void PutSelectionOnShelf9()
    {
        PutSelectionOnShelf(8);
    }

    // ---------- Rack Settings Shortcuts ----------

    const string MenuOpenRackSettings = TheShelfMenuPath + "Rack Settings" + TheShelfOpenShortcut + "0";

    [MenuItem(MenuOpenRackSettings, true)]
    static bool ValidateOpenSettings()
    {
        return GetTargetRack() != null;
    }

    [MenuItem(MenuOpenRackSettings, false, 101)]
    static void OpenSettings()
    {
        OpenRackSettings();
    }
}

}

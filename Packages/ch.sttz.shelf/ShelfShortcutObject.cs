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
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;

using UObject = UnityEngine.Object;

namespace sttz.TheShelf {

/// <summary>
/// Asset representing a shortcut inside Unity.
/// </summary>
/// <remarks>
/// The asset can represent three types of shortcuts:
/// - MenuItem: Execute any of Unity's menu items
/// - Preferences: Open Unity preferences on a specific section
/// - Project Settings: Open Unity project settings on a specific section
/// Since menu items can differ between platforms, the shortcut
/// allows to provide different values for macOS or Linux.
/// </remarks>
[CreateAssetMenu(menuName = "The Shelf/Shortcut", order = 100), InitializeOnLoad]
public class ShelfShortcutObject : ScriptableObject, IShelfItem
{
    // ---------- Properties ----------

    /// <summary>
    /// The type of the shortcut.
    /// </summary>
    [SerializeField] ShortcutType type;
    /// <summary>
    /// The shortcut string value, depending on <see cref="type"/>.
    /// </summary>
    [SerializeField] string shortcut;
    /// <summary>
    /// Override for shortcut on macOS, if it differs from <see cref="shortcut">.
    /// </summary>
    [SerializeField] string macShortcut;
    /// <summary>
    /// Override for shortcut on Linux, if it differs from <see cref="shortcut">.
    /// </summary>
    [SerializeField] string linuxShortcut;

    // ---------- API ----------

    /// <summary>
    /// The type of the shortcut.
    /// </summary>
    public enum ShortcutType
    {
        Undefined,

        /// <summary>
        /// Execute a menu item.
        /// </summary>
        MenuItem,
        /// <summary>
        /// Open a Preferences page.
        /// </summary>
        Preferences,
        /// <summary>
        /// Open a Project Settings page.
        /// </summary>
        ProjectSettings,
    }

    /// <summary>
    /// Open the shortcut.
    /// </summary>
    public void Open()
    {
        var sc = shortcut;
        if (Application.platform == RuntimePlatform.OSXEditor && !string.IsNullOrEmpty(macShortcut)) {
            sc = macShortcut;
        } else if (Application.platform == RuntimePlatform.LinuxEditor && !string.IsNullOrEmpty(linuxShortcut)) {
            sc = linuxShortcut;
        }

        if (string.IsNullOrEmpty(sc))
            return;

        if (type == ShortcutType.MenuItem) {
            EditorApplication.ExecuteMenuItem(sc);
        } else if (type == ShortcutType.Preferences) {
            SettingsService.OpenUserPreferences(sc);
        } else if (type == ShortcutType.ProjectSettings) {
            SettingsService.OpenProjectSettings(sc);
        }
    }

    /// <summary>
    /// Create a copy of the scene object reference.
    /// </summary>
    public UObject Clone()
    {
        var clone = CreateInstance<ShelfShortcutObject>();
        clone.name = name;
        clone.type = type;
        clone.shortcut = shortcut;
        clone.macShortcut = macShortcut;
        clone.linuxShortcut = linuxShortcut;
        return clone;
    }

    // ---------- Actions ----------

    [OnOpenAsset]
    static bool OnOpenAsset(int instanceID, int line)
    {
        var obj = EditorUtility.InstanceIDToObject(instanceID);
        if (obj is not ShelfShortcutObject shortcutObject)
            return false;

        shortcutObject.Open();
        return true;
    }

    // ---------- IShelfItem ----------

    string IShelfItem.Name => name;

    Texture IShelfItem.Icon => ShelfItemsView.GetIconTexture(this);

    UObject IShelfItem.Reference => this;

    VisualElement IShelfItem.Accessory => null;

    bool IShelfItem.SaveInRackAsset => false;
}

}

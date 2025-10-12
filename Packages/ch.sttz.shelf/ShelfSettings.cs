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
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UObject = UnityEngine.Object;

#if !UNITY_2023_2_OR_NEWER
using sttz.TheShelf.Backwards;
#endif

namespace sttz.TheShelf {

/// <summary>
/// Size for shelf items / asset previews.
/// </summary>
public enum ShelfItemSize
{
    Default,
    Tiny,
    Small,
    Medium,
    Large,
    Huge,
}

/// <summary>
/// Per-project shelf settings.
/// </summary>
public class ShelfSettings : ScriptableObject, INotifyBindablePropertyChanged
{
    // ---------- Settings ----------

    /// <summary>
    /// The path to the settings asset (outside of the Assets folder).
    /// </summary>
    const string SettingsPath = "UserSettings/TheShelf.asset";

    /// <summary>
    /// The project settings instance, which is stored in the `UserSettings` folder
    /// and should not be part of the versioned project assets.
    /// </summary>
    public static ShelfSettings Project
    {
        get {
            if (_settings == null) {
                LoadAssets();
            }
            return _settings;
        }
    }

    // ---------- User Rack ----------

    /// <summary>
    /// The user rack instance, which is stored in the `UserSettings` folder
    /// and should not be part of the versioned project assets.
    /// </summary>
    /// <remarks>
    /// The user rack is lazily loaded when this property is accessed,
    /// to check if a rack is the user rack without always loading the asset,
    /// use <see cref="IsUserRack"/> instead.
    /// </remarks>
    public static ShelfRack UserRack
    {
        get {
            if (_userRack == null) {
                LoadAssets();
            }
            return _userRack;
        }
    }

    /// <summary>
    /// Check if a given rack is the current user rack,
    /// without loading the user rack if it hasn't.
    /// </summary>
    public static bool IsUserRack(ShelfRack rack)
    {
        return _userRack != null && rack == _userRack;
    }

    // ---------- Settings ----------

    /// <summary>
    /// The active rack.
    /// </summary>
    public ShelfRack ActiveRack {
        get => _activeRack != null ? _activeRack : _userRack;
        set {
            var newValue = value;
            if (newValue == _userRack) {
                newValue = null;
            }

            if (_activeRack == newValue) return;

            _activeRack = newValue;
            NotifyHasChanged();
            RepaintProjectWindows();
        }
    }
    [SerializeField] ShelfRack _activeRack;

    /// <summary>
    /// Wether to open the active rack in a popup-window.
    /// </summary>
    public bool OpenActiveRackAsPopup {
        get => _openActiveRackAsPopup;
        set {
            if (_openActiveRackAsPopup == value) return;
            _openActiveRackAsPopup = value;
            NotifyHasChanged();
        }
    }
    [SerializeField] bool _openActiveRackAsPopup = true;

    /// <summary>
    /// Default size for shelf items.
    /// </summary>
    public ShelfItemSize ItemSize {
        get => _itemSize;
        set {
            if (_itemSize == value) return;
            _itemSize = value;
            NotifyHasChanged();
        }
    }
    [SerializeField] ShelfItemSize _itemSize;

    /// <summary>
    /// Resolve the `Default` item size.
    /// </summary>
    public ShelfItemSize ResolveSize(ShelfItemSize shelfSize)
    {
        if (shelfSize != ShelfItemSize.Default)
            return shelfSize;
        if (_itemSize != ShelfItemSize.Default)
            return _itemSize;
        return ShelfItemSize.Medium;
    }

    /// <summary>
    /// Custom style sheet to apply on top of default.
    /// </summary>
    public StyleSheet CustomStyleSheet {
        get => _customStyleSheet;
        set {
            if (_customStyleSheet == value) return;
            _customStyleSheet = value;
            NotifyHasChanged();
        }
    }
    [SerializeField] StyleSheet _customStyleSheet;

    /// <summary>
    /// Event triggered when the settings `OnDisable` is called.
    /// </summary>
    public event Action<ShelfSettings> OnSettingsUnloading;

    /// <summary>
    /// Check if a rack can toggle its active state.
    /// </summary>
    /// <remarks>
    /// This handles the case where the user rack cannot be
    /// toggled off, another rack needs to be toggled active
    /// instead.
    /// </remarks>
    public bool CanToggleRackActive(ShelfRack rack)
    {
        return !IsUserRack(rack) || ActiveRack != rack;
    }

    /// <summary>
    /// Toggle the active state of the given rack.
    /// </summary>
    /// <remarks>
    /// If <see cref="CanToggleRackActive"/> returns `false`,
    /// this method does nothing.
    /// </remarks>
    public void ToggleRackActive(ShelfRack rack)
    {
        if (rack == ActiveRack) {
            ActiveRack = null;
        } else {
            ActiveRack = rack;
        }
    }

    // ---------- Window Sizes ----------

    /// <summary>
    /// Remember a window position for the given rack.
    /// The rack needs to be an asset with a GUID.
    /// </summary>
    /// <remarks>
    /// Unity only remember window positions per window type,
    /// meaning multiple shelf windows always open on top of each other.
    /// Here we remember the window position per shelf asset.
    /// </remarks>
    /// <param name="rack">Rack to remember the position for</param>
    /// <param name="rect">The window position</param>
    public void RememberWindowRect(ShelfRack rack, Rect rect)
    {
        if (!GetRackGuid(rack, out var guid))
            return;

        windowRects ??= new();
        windowRects.RemoveAll(wr => wr.guid == guid);

        if (rect == Rect.zero)
            return;

        windowRects.Add(new WindowRect() {
            guid = guid,
            rect = rect,
        });

        NotifyHasChanged("windowRects");
    }

    /// <summary>
    /// Try to recall a window position for the given rack.
    /// </summary>
    /// <param name="rack">The rack to get the position for</param>
    /// <param name="rect">When successful, the remembered position</param>
    /// <returns>Wether a position was recalled</returns>
    public bool RecallWindowRect(ShelfRack rack, out Rect rect)
    {
        rect = default;

        if (windowRects == null)
            return false;

        if (!GetRackGuid(rack, out var guid))
            return false;

        var windowRect = windowRects.Find(wr => wr.guid == guid);
        if (windowRect.guid.Empty())
            return false;

        rect = windowRect.rect;
        return true;
    }

    /// <summary>
    /// Clear all previously remembered rack window positions.
    /// </summary>
    public void ClearWindowRects()
    {
        if (windowRects == null || windowRects.Count == 0)
            return;

        windowRects.Clear();
        NotifyHasChanged("windowRects");
    }

    /// <summary>
    /// Get the asset GUID for the given rack.
    /// </summary>
    bool GetRackGuid(ShelfRack rack, out GUID guid)
    {
        guid = default;

        if (rack == UserRack) {
            // The UserRack is stored in UserSettings and doesn't have a GUID,
            // use a dummy GUID here instead
            guid = UserRackGuid;
            return true;
        }

        var path = AssetDatabase.GetAssetPath(rack);
        if (string.IsNullOrEmpty(path))
            return false;

        guid = AssetDatabase.GUIDFromAssetPath(path);
        if (guid.Empty())
            return false;

        return true;
    }

    /// <summary>
    /// Struct to serialize remembered window positions.
    /// </summary>
    [Serializable]
    struct WindowRect : ISerializationCallbackReceiver
    {
        public GUID guid;
        public Rect rect;

        // GUID is not serializable, save it as string instead
        [SerializeField] string _guidString;

        public void OnAfterDeserialize()
        {
            if (!string.IsNullOrEmpty(_guidString)) {
                GUID.TryParse(_guidString, out guid);
            }
        }

        public void OnBeforeSerialize()
        {
            if (guid.Empty()) {
                _guidString = null;
            } else {
                _guidString = guid.ToString();
            }
        }
    }

    /// <summary>
    /// Remembered window positions.
    /// </summary>
    [SerializeField] List<WindowRect> windowRects;

    /// <summary>
    /// Dummy GUID used for <see cref="UserRack">.
    /// </summary>
    public static readonly GUID UserRackGuid = new GUID("557365725261636b557365725261636b");

    // ---------- Project Settings UI ----------

    /// <summary>
    /// The Shelf configuration UI in Unity's project settings window.
    /// </summary>
    [SettingsProvider]
    public static SettingsProvider CreateProjectSettingsUI()
    {
        return new("Project/The Shelf", SettingsScope.Project) {
            label = "The Shelf",
            keywords = new[] { "Shelf", "Rack", "Popup", "Size" },
            activateHandler = (searchCtx, rootElement) => {
                rootElement.style.paddingTop
                    = rootElement.style.paddingRight
                    = rootElement.style.paddingBottom
                    = rootElement.style.paddingLeft
                    = 5;

                var title = new Label();
                title.text = "The Shelf";
                title.style.fontSize = 19;
                title.style.unityFontStyleAndWeight = FontStyle.Bold;
                title.style.marginLeft = title.style.marginRight = 4;
                title.style.marginTop = 2;
                title.style.marginBottom = 10;
                rootElement.Add(title);

                var activeField = new ObjectField();
                activeField.label = "Active Rack";
                activeField.objectType = typeof(ShelfRack);
                activeField.allowSceneObjects = false;
                activeField.value = Project.ActiveRack;
                activeField.RegisterValueChangedCallback(e => {
                    Project.ActiveRack = (ShelfRack)e.newValue;
                });
                rootElement.Add(activeField);

                var popupField = new Toggle();
                popupField.label = "Open as Popup";
                popupField.value = Project.OpenActiveRackAsPopup;
                popupField.RegisterValueChangedCallback(e => {
                    Project.OpenActiveRackAsPopup = e.newValue;
                });
                rootElement.Add(popupField);

                var sizeField = new EnumField(ShelfItemSize.Default);
                sizeField.label = "Default Item Size";
                sizeField.value = Project.ItemSize;
                sizeField.RegisterValueChangedCallback(e => {
                    Project.ItemSize = (ShelfItemSize)e.newValue;
                });
                rootElement.Add(sizeField);

                var stylesField = new ObjectField();
                stylesField.label = "Custom Style Sheet";
                stylesField.objectType = typeof(StyleSheet);
                stylesField.value = Project.CustomStyleSheet;
                stylesField.RegisterValueChangedCallback(e => {
                    Project.CustomStyleSheet = (StyleSheet)e.newValue;
                });
                rootElement.Add(stylesField);
            }
        };
    }

    // ---------- Active Rack Badge ----------

    /// <summary>
    /// After changing the active rack, the project window icon badge is not 
    /// immediately updated, trigger a repaint to refresh it.
    /// </summary>
    static void RepaintProjectWindows()
    {
        var projectBrowserType = typeof(Editor).Assembly.GetType("UnityEditor.ProjectBrowser");
        if (projectBrowserType == null)
            return;

        var windows = Resources.FindObjectsOfTypeAll(projectBrowserType)
            .OfType<EditorWindow>();
        foreach (var window in windows) {
            window.Repaint();
        }
    }

    /// <summary>
    /// Hook OnGUI events to draw the active rack badge.
    /// </summary>
    [InitializeOnLoadMethod]
    static void RegisterOnPostIconGUI()
    {
        // In the Project Window he have a public callback
    #if UNITY_2022_1_OR_NEWER
        EditorApplication.projectWindowItemInstanceOnGUI += OnProjectWindowItemOnGUI;
    #endif

        // For the editor header, we hook into an internal event
        var editorType = typeof(Editor);
        var OnEditorGUIDelegate = editorType.GetNestedType("OnEditorGUIDelegate", BindingFlags.NonPublic);
        if (OnEditorGUIDelegate == null) {
            // Not available in Unity < 2017.1
            return;
        }

        var del = Delegate.CreateDelegate(OnEditorGUIDelegate, typeof(ShelfSettings), "OnPostIconGUI", false, false);
        if (del == null) {
            Debug.LogWarning("Could not bind OnPostIconGUI as OnEditorGUIDelegate.");
            return;
        }

        var OnPostIconGUI = editorType.GetField("OnPostIconGUI", BindingFlags.Static | BindingFlags.NonPublic);
        if (OnPostIconGUI == null) {
            Debug.LogWarning("Could not find OnPostIconGUI field on Editor class.");
            return;
        }

        var value = (Delegate)OnPostIconGUI.GetValue(null);
        OnPostIconGUI.SetValue(null, Delegate.Combine(value, del));
    }

#if UNITY_2022_1_OR_NEWER
    static void OnProjectWindowItemOnGUI(int instanceID, Rect selectionRect)
    {
        if (instanceID != Project.ActiveRack.GetInstanceID())
            return;

        var icon = ShelfAssets.Shared.activeIcon;
        if (icon == null)
            return;

        var rect = selectionRect;
        rect.width = rect.height = selectionRect.height * 0.3f;
        if (selectionRect.width / selectionRect.height > 5) {
            // List mode (slider to the far left)
            rect.x += rect.width * 0.5f;
            rect.y += selectionRect.height - rect.height * 1.1f;
        } else {
            // Grid mode
            rect.x -= rect.width * 0.1f;
            rect.y = selectionRect.height - rect.height * 1.1f;
        }

        GUI.DrawTexture(rect, icon);
    }
#endif

    static void OnPostIconGUI(Editor editor, Rect drawRect)
    {
        if (editor.target != Project.ActiveRack)
            return;

        var icon = ShelfAssets.Shared.activeIcon;
        if (icon == null)
            return;

        drawRect.x -= 24;
        drawRect.y += 2;
        drawRect.width = drawRect.height = 14;
        GUI.DrawTexture(drawRect, icon);
    }

    // ---------- INotifyBindablePropertyChanged ----------

    public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;

    void NotifyHasChanged([CallerMemberName] string property = "")
    {
        propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(property));
    }

    // ---------- Static Internals ----------

    static ShelfRack _userRack;
    static ShelfSettings _settings;

    static void LoadAssets()
    {
        // Load settings and user rack from user settings
        var objects = UnityEditorInternal.InternalEditorUtility.LoadSerializedFileAndForget(SettingsPath);
        if (objects?.Length > 0) {
            _settings = objects.OfType<ShelfSettings>().FirstOrDefault();
            _userRack = objects.OfType<ShelfRack>().FirstOrDefault();
        }

        // Create new instances if one couldn't be loaded
        if (_settings == null) {
            _settings = CreateInstance<ShelfSettings>();
            _settings.hideFlags = HideFlags.DontSave;
        }
        if (_userRack == null) {
            _userRack = CreateInstance<ShelfRack>();
            _userRack.name = "User Rack";
            _userRack.hideFlags = HideFlags.DontSave;

            // Put start items on new user racks
            if (_userRack.Shelves?.Count > 0 && ShelfAssets.Shared.startShelf?.Length > 0) {
                foreach (var item in ShelfAssets.Shared.startShelf) {
                    _userRack.Shelves[0].Items.Add(item);
                }
            }
        }

        _settings.propertyChanged += OnProjectSettingsChanged;
        _settings.OnSettingsUnloading += OnProjectSettingsUnloading;

        _userRack.OnRackChanged += OnUserRackChanged;
        _userRack.OnRackUnloading += OnUserRackUnloading;
    }

    static HashSet<UObject> saveSet;

    static void SaveAssets()
    {
        if (_settings == null || _userRack == null)
            return;

        saveSet ??= new();
        saveSet.Clear();

        saveSet.Add(_settings);
        saveSet.Add(_userRack);
        _userRack.GetObjectsToPersist(saveSet);

        foreach (var obj in saveSet) {
            obj.hideFlags = HideFlags.DontSave;
        }

        UnityEditorInternal.InternalEditorUtility.SaveToSerializedFileAndForget(
            saveSet.ToArray(), 
            SettingsPath, 
            allowTextSerialization: true
        );

        saveSet.Clear();
    }

    static void OnProjectSettingsChanged(object sender, BindablePropertyChangedEventArgs args)
    {
        SaveAssets();
    }

    static void OnProjectSettingsUnloading(ShelfSettings settings)
    {
        settings.propertyChanged -= OnProjectSettingsChanged;
        settings.OnSettingsUnloading -= OnProjectSettingsUnloading;

        if (settings == _settings) {
            _settings = null;
            DestroyImmediate(settings);
        }
    }

    static void OnUserRackChanged(ShelfRack rack)
    {
        SaveAssets();
    }

    static void OnUserRackUnloading(ShelfRack rack)
    {
        rack.OnRackChanged -= OnUserRackChanged;
        rack.OnRackChanged -= OnUserRackUnloading;

        if (rack == _userRack) {
            _userRack = null;
            DestroyImmediate(rack);
        }
    }

    // ---------- Internals ----------

    void OnDisable()
    {
        OnSettingsUnloading?.Invoke(this);
    }
}

}

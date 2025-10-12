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
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

#if !UNITY_2023_2_OR_NEWER
using sttz.TheShelf.Backwards;
#endif

namespace sttz.TheShelf {

/// <summary>
/// Window presenting a single <see cref="ShelfRack"/>.
/// </summary>
public class ShelfWindow : EditorWindow, IHasCustomMenu
{
    // ---------- Static API ----------

    /// <summary>
    /// Default window size used for popup racks when they
    /// haven't been opened as a tab before (and their size was remembered).
    /// </summary>
    /// <remarks>
    /// When opening a rack as a tab, the initial positioning and sizing
    /// is left to Unity.
    /// </remarks>
    public static Vector2 DefaultPopupWindowSize = new Vector2(500, 500);

    /// <summary>
    /// Open a shelf window for the given rack,
    /// focuses the window if one is already open for the rack.
    /// </summary>
    public static ShelfWindow OpenRack(ShelfRack rack)
    {
        if (rack == null)
            return null;

        var window = GetOpenWindowForRack(rack);
        if (window != null) {
            window.Focus();
        } else {
            window = CreateWindow<ShelfWindow>();
            window.titleContent = new(rack.name);
            window.ViewRack(rack);

            if (ShelfSettings.Project.RecallWindowRect(rack, out var position)) {
                window.position = position;
            }
        }

        return window;
    }

    /// <summary>
    /// Open a rack as a popup window, positioned next to the window
    /// the mouse is over.
    /// </summary>
    /// <remarks>
    /// If the rack is already opened in a regular window,
    /// that window will be focused instead.
    /// </remarks>
    public static ShelfWindow PopupRack(ShelfRack rack)
    {
        if (rack == null)
            return null;

        var window = GetOpenWindowForRack(rack, includePopup: true);
        if (window != null) {
            window.Focus();
            return window;
        }

        window = CreateInstance<ShelfWindow>();
        window.titleContent = new(rack.name);
        window.IsPopup = true;

        window.ShowUtility();
        window.ViewRack(rack);

        var windowSize = DefaultPopupWindowSize;
        if (ShelfSettings.Project.RecallWindowRect(rack, out var windowRect)) {
            windowSize = windowRect.size;
        }

        var popupNextTo = EditorWindow.mouseOverWindow;
        if (popupNextTo != null) {
            var error = PopupWindowUtility.GetPopupPosition(popupNextTo, windowSize, out var popupPosition);
            if (error != null) error.LogOnce();
            window.position = popupPosition;
        } else {
            // Center on main window as fallback
            var mainWindowRect = EditorGUIUtility.GetMainWindowPosition();
            window.position = new Rect(
                mainWindowRect.x + (mainWindowRect.width - windowSize.x) / 2,
                mainWindowRect.y + (mainWindowRect.height - windowSize.y) / 2,
                windowSize.x,
                windowSize.y
            );
        }

        return window;
    }

    /// <summary>
    /// Find and already open window that displays the given rack.
    /// </summary>
    public static ShelfWindow GetOpenWindowForRack(ShelfRack rack, bool includePopup = false)
    {
        if (openWindows == null)
            return null;

        foreach (var window in openWindows) {
            if (window == null) continue;
            if (!includePopup && window.IsPopup) continue;

            if (window.rack == rack)
                return window;
        }

        return null;
    }

    // ---------- API ----------

    /// <summary>
    /// The main view for presenting the rack.
    /// </summary>
    public ShelfRackView RackView => rackView;

    /// <summary>
    /// The rack viewed by this window.
    /// </summary>
    public ShelfRack Rack => rackView.Rack;

    /// <summary>
    /// Wether this window is a popup window.
    /// </summary>
    public bool IsPopup {
        get => _isPopup;
        set {
            if (_isPopup == value) return;
            _isPopup = value;
            if (rackView != null) {
                rackView.IsPopup = value;
            }
        }
    }
    bool _isPopup;

    /// <summary>
    /// Change the window to view the given rack.
    /// </summary>
    public void ViewRack(ShelfRack viewRack)
    {
        rack = rackView.Rack = viewRack;
        isUserRack = ShelfSettings.IsUserRack(viewRack);
        UpdateWindowIcon();
        rackView.FocusActiveShelfItemsList();
    }

    // ---------- Internals ----------

    static List<ShelfWindow> openWindows;

    // State that needs to persist through domain reload
    [SerializeField] ShelfRack rack;
    [SerializeField] int activeTabIndex = -1;
    [SerializeField] bool isUserRack;

    ShelfRackView rackView;
    Rect popupMovePos;

    void IHasCustomMenu.AddItemsToMenu(GenericMenu menu)
    {
        var project = ShelfSettings.Project;

        // The user rack is the fallback active rack and cannot be de-activated
        // Passing a null MenuFunction makes the menu item disabled
        GenericMenu.MenuFunction toggleActive = null;
        if (project.CanToggleRackActive(Rack)) {
            toggleActive = () => project.ToggleRackActive(Rack);
        }

        // The user rack isn't an asset and cannot be pinged
        GenericMenu.MenuFunction pingRack = null;
        if (!ShelfSettings.IsUserRack(Rack)) {
            pingRack = () => EditorGUIUtility.PingObject(Rack);
        }

        menu.AddItem(new("Active Rack"), Rack == project.ActiveRack, toggleActive);
        menu.AddItem(new("Ping Rack"), false, pingRack);
    }

    void OnEnable()
    {
        // After a domain reload, the window will edit the old user rack object
        // we need to detect this and point it to the new user rack instead
        if (isUserRack && rack != ShelfSettings.UserRack) {
            rack = ShelfSettings.UserRack;
        }

        openWindows ??= new();
        openWindows.Add(this);

        ShelfSettings.Project.propertyChanged += OnSettingsPropertyChanged;
    }

    void CreateGUI()
    {
        rackView = new ShelfRackView();
        rackView.IsPopup = IsPopup;
        rootVisualElement.Add(rackView);

        if (rack != null) {
            rackView.Rack = rack;
        }
        if (activeTabIndex >= 0) {
            rackView.ActiveTabIndex = activeTabIndex;
            rackView.FocusActiveShelfItemsList();
        }
        if (IsPopup) {
        #if UNITY_2023_2_OR_NEWER && UNITY_EDITOR_OSX
            windowFocusChanged += OnWindowFocusChanged;
        #endif
            rackView.OnShelfItemAction += OnShelfItemAction;
            rackView.OnPopupDock += OnDockPopup;
            rackView.OnTabAdded += OnPoupTabAdded;
            rackView.RegisterCallback<KeyDownEvent>(OnPopupKeyDown, TrickleDown.TrickleDown);
        }
    }

    void OnSettingsPropertyChanged(object sender, BindablePropertyChangedEventArgs args)
    {
        if (args.propertyName == nameof(ShelfSettings.ActiveRack)) {
            UpdateWindowIcon();
        }
    }

    void UpdateWindowIcon()
    {
        if (rack == null)
            return;

        var content = titleContent;
        content.text = rack.name;
        content.image = rack == ShelfSettings.Project.ActiveRack
            ? ShelfAssets.Shared.rackWindowActiveIcon
            : ShelfAssets.Shared.rackWindowIcon;
        titleContent = content;
    }

    void OnPoupTabAdded(Tab tab)
    {
        var emptySpace = tab.Q(className: "shelf-tab-empty-space");
        if (emptySpace != null) {
            emptySpace.AddManipulator(new DragWindowManipulator(OnPopupMoveWindowStart, OnPopupMoveWinodw));
        }
    }

    void OnPopupMoveWindowStart()
    {
        // For some reason, when updating the window's rect during a drag,
        // the window position is reported as (0,0) (seen in unity 2023.2.15).
        // We therefore remember the current position at the start and
        // then use it instead of what Unity reports.
        popupMovePos = position;
    }

    void OnPopupMoveWinodw(Vector2 delta)
    {
        popupMovePos.position += delta;
        position = popupMovePos;
    }

    void OnDockPopup()
    {
        // Open window at same position as popup
        ShelfSettings.Project.RememberWindowRect(rack, position);

        // Close and re-open as regular window
        Close();
        OpenRack(rack);
    }

    void OnPopupKeyDown(KeyDownEvent evt)
    {
        if (evt.keyCode != KeyCode.Escape)
            return;

        Close();
        evt.StopPropagation();
    }

// Auto-closing only works on macOS, on Windows focus changes during
// a drag, which makes closing on lost focus impractical.
#if UNITY_EDITOR_OSX
#if UNITY_2023_2_OR_NEWER
    void OnWindowFocusChanged()
    {
        // focusedWindow is not always up to date in OnWindowFocusChanged
        // delay one frame to get actual focused window
        EditorApplication.delayCall += CheckPopupFocus;
    }

    void CheckPopupFocus()
    {
        if (focusedWindow == null)
            return;

        // Newer Unity version show custom context menu as its own popup
        // The shelf settings can also open the color picker as a utility window
        // Don't close the popup shelf if another popup has focus
        var error = PopupWindowUtility.GetShowMode(focusedWindow, out var showMode);
        if (error != null) error.LogOnce();

        if (showMode == PopupWindowUtility.ShowMode.PopupMenu 
                || showMode == PopupWindowUtility.ShowMode.Utility)
            return;

        if (focusedWindow != this)
            Close();
    }
#else
    void OnLostFocus()
    {
        if (IsPopup) {
            Close();
        }
    }
#endif
#endif

    void OnShelfItemAction(ShelfItemEvent evt)
    {
        Close();
    }

    void OnBecameInvisible()
    {
        if (IsPopup) UnregisterPopupListeners();
    }

    void OnDisable()
    {
        if (!IsPopup && rack != null) {
            ShelfSettings.Project.RememberWindowRect(rack, position);
        }

        ShelfSettings.Project.propertyChanged += OnSettingsPropertyChanged;

        if (openWindows != null) {
            openWindows.Remove(this);
        }

        if (rackView != null) {
            rack = rackView.Rack;
            activeTabIndex = rackView.ActiveTabIndex;
        }

        // OnBecameInvisible is not called when popup is closed,
        // remove listener here as well
        if (IsPopup) UnregisterPopupListeners();
    }

    void UnregisterPopupListeners()
    {
    #if UNITY_2023_2_OR_NEWER && UNITY_EDITOR_OSX
        windowFocusChanged -= OnWindowFocusChanged;
    #endif
        if (rackView != null) {
            rackView.OnShelfItemAction -= OnShelfItemAction;
            rackView.OnPopupDock -= OnDockPopup;
            rackView.OnTabAdded -= OnPoupTabAdded;
            rackView.UnregisterCallback<KeyDownEvent>(OnPopupKeyDown);
        }
    }

    /// <summary>
    /// Manipulator to support dragging the popup window by its background.
    /// </summary>
    class DragWindowManipulator : PointerManipulator
    {
        /// <summary>
        /// Distance to start a drag when dragging from the tab header.
        /// </summary>
        public static float startDragDistance = 5f;

        /// <summary>
        /// Event called when the window moving starts.
        /// </summary>
        public Action moveStart;
        /// <summary>
        /// Event called when the window should be moved.
        /// </summary>
        public Action<Vector2> moveWindow;

        bool isMoving;
        bool isDragging;
        Vector3 startPos;

        public DragWindowManipulator(Action onMoveStart = null, Action<Vector2> onMoveWindow = null)
        {
            if (onMoveStart != null) {
                moveStart += onMoveStart;
            }
            if (onMoveWindow != null) {
                moveWindow += onMoveWindow;
            }
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<PointerDownEvent>(OnPointerDown);
            target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            target.RegisterCallback<PointerUpEvent>(OnPointerUp);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
            target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
            target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
        }

        void OnPointerDown(PointerDownEvent evt)
        {
            isMoving = true;
            startPos = evt.localPosition;
        }

        void OnPointerMove(PointerMoveEvent evt)
        {
            if (!isMoving) return;

            var currentPos = evt.localPosition;

            if (!isDragging) {
                if (Vector2.Distance(startPos, currentPos) < startDragDistance)
                    return;

                // start drag!
                isDragging = true;
                target.CapturePointer(evt.pointerId);
                moveStart?.Invoke();
            }

            // update drag!
            moveWindow?.Invoke(currentPos - startPos);
        }

        void OnPointerUp(PointerUpEvent evt)
        {
            isMoving = false;
            isDragging = false;
            target.ReleasePointer(evt.pointerId);
        }
    }
}

/// <summary>
/// Helper class accessing editor internals to work with popup windows.
/// </summary>
public static class PopupWindowUtility
{
    /// <summary>
    /// Workaround to the size of the screen the given window is on.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <param name="window">Window to get the screen size from</param>
    /// <param name="screenRect">The screen size will be set to this rect</param>
    /// <returns>`true` when successful, `false` on error. Consult <see cref="Error"/> for the error description</returns>
    public static ReflectionError GetWindowScreenRect(EditorWindow window, out Rect screenRect)
    {
        screenRect = default;

        var error = ReflectScreenRect();
        if (error != null) return error;

        error = GetContainerWindow(window, out var container);
        if (error != null) return error;

        var inputRect = new Rect(float.MinValue, float.MinValue, float.MaxValue, float.MaxValue);

        if (FitRectToScreenMethod != null) {
            screenRect = (Rect)FitRectToScreenMethod.Invoke(null, new object[] { inputRect, screenRect.center, true, container });
        } else {
            screenRect = (Rect)FitWindowRectToScreenMethod.Invoke(container, new object[] { inputRect, true, false });
        }

        return null;
    }

    /// <summary>
    /// Find a position next to the given window to open a popup window at.
    /// </summary>
    /// <param name="popupNextTo">The window to open next to</param>
    /// <param name="popupSize">The size of the popup window</param>
    /// <returns>The rect to position the popup window with</returns>
    public static ReflectionError GetPopupPosition(EditorWindow popupNextTo, Vector2 popupSize, out Rect popupPosition)
    {
        // Try to get size of screen window is on using private APIs
        var error = GetWindowScreenRect(popupNextTo, out var screenRect);
        if (error != null) {
            // Failed to get proper screen rect, fallback to public APIs
            // This will not work properly on multi-screen setups
            screenRect = new Rect(
                0, 0,
                Screen.currentResolution.width,
                Screen.currentResolution.height
            );
        }

        var targetWindowRect = popupNextTo.position;

        popupPosition = new Rect {
            width = popupSize.x,
            height = popupSize.y
        };

        var aspect = targetWindowRect.width / targetWindowRect.height;
        var padding = 10f;

        // Taller than wide: left or right
        if (aspect < 1f) {
            var side = WindowSide(aspect, targetWindowRect, screenRect.size);
            if (side == 1f) {
                popupPosition.x = targetWindowRect.x + targetWindowRect.width + padding;
            } else {
                popupPosition.x = targetWindowRect.x - padding - popupSize.x;
            }
            popupPosition.y = targetWindowRect.y + targetWindowRect.height / 2 - popupSize.y / 2;

        // Wider than tall: above or below
        } else {
            var side = WindowSide(aspect, targetWindowRect, screenRect.size);
            if (side == 1f) {
                popupPosition.y = targetWindowRect.y + targetWindowRect.height + padding;
            } else {
                popupPosition.y = targetWindowRect.y - padding - popupSize.y;
            }
            popupPosition.x = targetWindowRect.x + targetWindowRect.width / 2 - popupSize.x / 2;
        }

        return error;
    }

    /// <summary>
    /// Window show mode, copied from ContainerWindow.bindings.cs.
    /// </summary>
    public enum ShowMode
    {
        // Unknown mode, set on error
        Unknown = -1,
        // Show as a normal window with max, min & close buttons.
        NormalWindow = 0,
        // Used for a popup menu. On mac this means light shadow and no titlebar.
        PopupMenu = 1,
        // Utility window - floats above the app. Disappears when app loses focus.
        Utility = 2,
        // Window has no shadow or decorations. Used internally for dragging stuff around.
        NoShadow = 3,
        // The Unity main window. On mac, this is the same as NormalWindow, except window doesn't have a close button.
        MainWindow = 4,
        // Aux windows. The ones that close the moment you move the mouse out of them.
        AuxWindow = 5,
        // Like PopupMenu, but without keyboard focus
        Tooltip = 6,
        // Modal Utility window
        ModalUtility = 7,
    }

    /// <summary>
    /// Return a window's show mode, making it possible to distinguish
    /// popup and utility windows from regular ones.
    /// </summary>
    public static ReflectionError GetShowMode(EditorWindow window, out ShowMode isPopup)
    {
        isPopup = ShowMode.Unknown;

        var error = ReflectIsPopup();
        if (error != null) return error;

        error = GetContainerWindow(window, out var container);
        if (error != null) return error;

        var mode = (int)m_ShowModeField.GetValue(container);
        isPopup = (ShowMode)mode;

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
                $"PopupWindowUtility: "
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

    static BindingFlags bindingFlags = 
          BindingFlags.NonPublic
        | BindingFlags.Public
        | BindingFlags.Static
        | BindingFlags.Instance;

    static Type EditorWindowType;
    // internal partial class View : ScriptableObject
    static Type ViewType;
    // internal partial class ContainerWindow : ScriptableObject
    static Type ContainerWindowType;
    // internal HostView m_Parent; (on EditorWindow)
    static FieldInfo m_ParentField;
    // ContainerWindow m_Window; (on View)
    static FieldInfo m_WindowField;
    // int m_ShowMode; (on ContainerWindow)
    static FieldInfo m_ShowModeField;
    // internal static extern Rect FitRectToScreen(Rect rect, Vector2 uiPositionToFindScreen, bool forceCompletelyVisible, ContainerWindow windowForBorderCalculation)
    // Replaced FitWindowRectToScreen in 6000.1 and was backpkorted to 6000.0.49 and others
    static MethodInfo FitRectToScreenMethod;
    // public extern Rect FitWindowRectToScreen(Rect r, bool forceCompletelyVisible, bool useMouseScreen) (on ContainerWindow)
    // Old method before being replaced with FitRectToScreen
    static MethodInfo FitWindowRectToScreenMethod;

    static ReflectionError errorBasic;
    static ReflectionError errorScreenRect;
    static ReflectionError errorIsPopup;

    static ReflectionError ReflectBasic()
    {
        if (errorBasic != null)
            return errorBasic;

        EditorWindowType = typeof(EditorWindow);

        m_ParentField = EditorWindowType.GetField("m_Parent", bindingFlags);
        if (m_ParentField == null) {
            return ReflectionError.Create(out errorBasic, "Could not get 'm_Parent' field of 'UnityEditor.EditorWindow' type.");
        }

        ViewType = EditorWindowType.Assembly.GetType("UnityEditor.View");
        if (ViewType == null) {
            return ReflectionError.Create(out errorBasic, "Could not get 'UnityEditor.View' type.");
        }

        m_WindowField = ViewType.GetField("m_Window", bindingFlags);
        if (m_WindowField == null) {
            return ReflectionError.Create(out errorBasic, "Could not get 'm_Window' field of 'UnityEditor.View' type.");
        }

        ContainerWindowType = EditorWindowType.Assembly.GetType("UnityEditor.ContainerWindow");
        if (ContainerWindowType == null) {
            return ReflectionError.Create(out errorBasic, "Could not get 'UnityEditor.ContainerWindowType' type.");
        }

        return null;
    }

    static ReflectionError ReflectScreenRect()
    {
        if (errorScreenRect != null)
            return errorScreenRect;

        var error = ReflectBasic();
        if (error != null) return error;

        // First try to find the new method added in 6000.2 and backported
        FitRectToScreenMethod = ContainerWindowType.GetMethod(
            "FitRectToScreen", bindingFlags, null,
            new[] { typeof(Rect), typeof(Vector2), typeof(bool), ContainerWindowType },
            null
        );

        if (FitRectToScreenMethod == null) {
            // Fall back to finding the old method
            FitWindowRectToScreenMethod = ContainerWindowType.GetMethod(
                "FitWindowRectToScreen", bindingFlags, null, 
                new[] { typeof(Rect), typeof(bool), typeof(bool) }, 
                null
            );

            if (FitWindowRectToScreenMethod == null) {
                return ReflectionError.Create(out errorScreenRect, "Could not get 'FitRectToScreen(Rect,Vector2,bool,ContainerWindow)' or 'FitWindowRectToScreen(Rect,bool,bool)' method of 'UnityEditor.ContainerWindow' type.");
            }
        }

        return null;
    }

    static ReflectionError ReflectIsPopup()
    {
        if (errorIsPopup != null)
            return errorIsPopup;

        var error = ReflectBasic();
        if (error != null) return error;

        m_ShowModeField = ContainerWindowType.GetField("m_ShowMode", bindingFlags);
        if (m_ShowModeField == null) {
            ReflectionError.Create(out errorIsPopup, "Could not get 'm_ShowMode' method of 'UnityEditor.ContainerWindow' type.");
        }

        return null;
    }

    static ReflectionError GetContainerWindow(EditorWindow window, out object containerWindow)
    {
        containerWindow = null;

        var error = ReflectBasic();
        if (error != null) return error;

        var view = m_ParentField.GetValue(window);
        if (view == null) {
            return ReflectionError.Create(out errorBasic, $"'m_Parent' field of 'UnityEditor.EditorWindow' ('{window.name}') is null");
        }

        containerWindow = m_WindowField.GetValue(view);
        if (containerWindow == null) {
            return ReflectionError.Create(out errorBasic, $"'m_Window' field of 'UnityEditor.View' ('{(view as ScriptableObject)?.name}') is null");
        }

        return null;
    }

    static float WindowSide(float aspect, Rect windowRect, Vector2 screenSize)
    {
        if (aspect < 1f) {
            return Mathf.Sign((screenSize.x - windowRect.x - windowRect.width) - windowRect.x);
        } else {
            return Mathf.Sign((screenSize.y - windowRect.y - windowRect.height) - windowRect.y);
        }
    }
}

}

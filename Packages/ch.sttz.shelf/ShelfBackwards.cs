using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

#if !UNITY_2023_2_OR_NEWER

/// <summary>
/// Namespace containing reimplementations for features that
/// are missing prior to Unity 2023.2.
/// </summary>
namespace sttz.TheShelf.Backwards {

#if !HAS_UNITY_PROPERTIES && !UNITY_2022_2_OR_NEWER
public class CreatePropertyAttribute : Attribute {}
public class DontCreatePropertyAttribute : Attribute {}
#endif

public readonly struct BindablePropertyChangedEventArgs
{
    readonly string m_PropertyName;

    public BindablePropertyChangedEventArgs(in string propertyName) => m_PropertyName = propertyName;

    public string propertyName => m_PropertyName;
}

public interface INotifyBindablePropertyChanged
{
    event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;
}

public static class ListViewHelpers
{
    /// <summary>
    /// Get the current insert position when dragging into the list view.
    /// </summary>
    /// <remarks>
    /// This API was only made public in Unity 2023.2, it's internal in 
    /// earlier version, so we need to use reflection to access it.
    /// </remarks>
    public static int GetInsertPosition(this ListView listView, Vector2 pointerPosition)
    {
        if (!FindMethods()) return -1;

        var dragger = draggerField.GetValue(listView);
        if (dragger == null) return -1;

        TryGetDragPositionMethodArgs[0] = pointerPosition;
        TryGetDragPositionMethodArgs[1] = null;
        var foundPosition = TryGetDragPositionMethod.Invoke(dragger, TryGetDragPositionMethodArgs);
        if (foundPosition is not bool foundPositionBool || !foundPositionBool) return -1;

        var dragPosition = TryGetDragPositionMethodArgs[1];
        if (dragPosition == null) return -1;

        var index = insertAtIndexField.GetValue(dragPosition);
        if (index is not int indexInt) return -1;

        return indexInt;
    }

    static bool FindMethods()
    {
        if (findResult != null)
            return findResult.Value;

        draggerField = typeof(BaseVerticalCollectionView).GetField("m_Dragger", bindingFlags);
        if (draggerField == null)
            return (findResult = false).Value;

        ListViewDraggerType = typeof(ListView).Assembly.GetType("UnityEngine.UIElements.ListViewDragger");
        if (ListViewDraggerType == null)
            return (findResult = false).Value;

        TryGetDragPositionMethod = ListViewDraggerType.GetMethod("TryGetDragPosition", bindingFlags);
        if (TryGetDragPositionMethod == null)
            return (findResult = false).Value;

        DragPositionType = ListViewDraggerType.GetNestedType("DragPosition", bindingFlags);
        if (DragPositionType == null)
            return (findResult = false).Value;

        insertAtIndexField = DragPositionType.GetField("insertAtIndex", bindingFlags);
        if (insertAtIndexField == null)
            return (findResult = false).Value;

        TryGetDragPositionMethodArgs = new object[2];

        return (findResult = true).Value;
    }

    static BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;
    static bool? findResult;
    static object[] TryGetDragPositionMethodArgs;
    static FieldInfo draggerField;
    static Type ListViewDraggerType;
    static MethodInfo TryGetDragPositionMethod;
    static Type DragPositionType;
    static FieldInfo insertAtIndexField;
}

public class TabView : VisualElement
{
    public static readonly string ussClassName = "unity-tab-view";
    public static readonly string headerContainerClassName = ussClassName + "__header-container";
    public static readonly string contentContainerUssClassName = ussClassName + "__content-container";
    public static readonly string reorderableUssClassName = ussClassName + "__reorderable";
    public static readonly string verticalUssClassName = ussClassName + "__vertical";

    public override VisualElement contentContainer => m_ContentContainer;

    public event Action<Tab, Tab> activeTabChanged;

    public Tab activeTab
    {
        get => m_ActiveTab;
        set
        {
            if (value == null && m_Tabs.Count > 0)
                throw new NullReferenceException("Active tab cannot be null when there are available tabs.");

            if (m_Tabs.IndexOf(value) == -1)
                throw new Exception("The tab to be set as active does not exist in this TabView.");

            if (value == m_ActiveTab)
                return;

            var previous = m_ActiveTab;
            m_ActiveTab?.SetInactive();
            m_ActiveTab = value;
            m_ActiveTab?.SetActive();

            activeTabChanged?.Invoke(previous, value);
        }
    }

    public int selectedTabIndex
    {
        get
        {
            if (activeTab == null || m_Tabs.Count == 0)
                return -1;

            return m_Tabs.IndexOf(activeTab);
        }
        set
        {
            if (value >= 0 && m_Tabs.Count > value)
                activeTab = m_Tabs[value];
        }
    }

    public TabView()
    {
        AddToClassList(ussClassName);

        m_HeaderContainer = new VisualElement() { name = headerContainerClassName };
        m_HeaderContainer.AddToClassList(headerContainerClassName);
        hierarchy.Add(m_HeaderContainer);

        m_ContentContainer = new VisualElement() { name = contentContainerUssClassName };
        m_ContentContainer.AddToClassList(contentContainerUssClassName);
        hierarchy.Add(m_ContentContainer);
    }

    public void AddTab(VisualElement ve)
    {
        m_ContentContainer.Add(ve);

        if (ve is not Tab tab)
            return;

        var tabHeader = tab.tabHeader;
        if (tabHeader != null)
        {
            m_HeaderContainer.Add(tabHeader);
            m_TabHeaders.Add(tabHeader);
            m_Tabs.Add(tab);
        }

        tab.selected += OnTabSelected;

        // Set the first tab to be active
        if (activeTab == null)
            activeTab = tab;
    }

    public void RemoveTab(VisualElement ve)
    {
        m_ContentContainer.Remove(ve);

        if (ve is not Tab tab)
            return;

        var tabHeaderVisualElement = tab.tabHeader;
        m_HeaderContainer.Remove(tabHeaderVisualElement);
        m_TabHeaders.Remove(tabHeaderVisualElement);
        m_Tabs.Remove(tab);

        // in case of tab being removed from TabView
        tab.hierarchy.Insert(0, tabHeaderVisualElement);
        tab.SetInactive();

        tab.selected -= OnTabSelected;

        // If we delete an active tab and there are more available, default back to the first one.
        if (activeTab == tab && m_Tabs.Count > 0)
            activeTab = m_Tabs[0];
        else if (m_Tabs.Count == 0)
            m_ActiveTab = null;
    }

    VisualElement m_HeaderContainer;
    VisualElement m_ContentContainer;
    List<Tab> m_Tabs = new();
    List<VisualElement> m_TabHeaders = new();
    Tab m_ActiveTab;

    void OnTabSelected(Tab tab)
    {
        activeTab = tab;
    }
}

public class Tab : VisualElement
{
    public static readonly string ussClassName = "unity-tab";
    public static readonly string selectedUssClassName = ussClassName + "--selected";
    public static readonly string tabHeaderUssClassName = ussClassName + "__header";
    public static readonly string tabHeaderSelectedUssClassName = tabHeaderUssClassName + "--selected";
    public static readonly string tabHeaderImageUssClassName = tabHeaderUssClassName + "-image";
    public static readonly string tabHeaderEmptyImageUssClassName = tabHeaderImageUssClassName + "--empty";
    public static readonly string tabHeaderStandaloneImageUssClassName = tabHeaderImageUssClassName + "--standalone";
    public static readonly string tabHeaderLabelUssClassName = tabHeaderUssClassName + "-label";
    public static readonly string tabHeaderEmptyLabeUssClassName = tabHeaderLabelUssClassName + "--empty";
    public static readonly string tabHeaderUnderlineUssClassName = tabHeaderUssClassName + "-underline";
    public static readonly string contentUssClassName = ussClassName + "__content-container";
    public static readonly string draggingUssClassName = ussClassName + "--dragging";
    public static readonly string reorderableUssClassName = ussClassName + "__reorderable";
    public static readonly string reorderableItemHandleUssClassName = reorderableUssClassName + "-handle";
    public static readonly string reorderableItemHandleBarUssClassName = reorderableItemHandleUssClassName + "-bar";
    public static readonly string closeableUssClassName = tabHeaderUssClassName + "__closeable";
    public static readonly string closeButtonUssClassName = ussClassName + "__close-button";

    public override VisualElement contentContainer => m_ContentContainer;

    public VisualElement tabHeader => m_TabHeader;

    public string label
    {
        get => m_Label;
        set
        {
            if (string.CompareOrdinal(value, m_Label) == 0)
                return;

            m_TabHeaderLabel.text = value;
            m_TabHeaderLabel.EnableInClassList(tabHeaderEmptyLabeUssClassName, string.IsNullOrEmpty(value));

            // Removes the margin using this class. This is until we have better support for targeting siblings in
            // USS or additional pseudo support like :not().
            m_TabHeaderImage.EnableInClassList(tabHeaderStandaloneImageUssClassName, string.IsNullOrEmpty(value));

            m_Label = value;
        }
    }

    public Background iconImage
    {
        get => m_IconImage;
        set
        {
            if (value == m_IconImage)
                return;

            if (value == default)
            {
                m_TabHeaderImage.image = null;
                m_TabHeaderImage.sprite = null;
                m_TabHeaderImage.vectorImage = null;

                m_TabHeaderImage.AddToClassList(tabHeaderEmptyImageUssClassName);
                m_TabHeaderImage.RemoveFromClassList(tabHeaderStandaloneImageUssClassName);

                m_IconImage = value;
                return;
            }

            // The image control will reset the other values to null
            if (value.texture)
                m_TabHeaderImage.image = value.texture;
            else if (value.sprite)
                m_TabHeaderImage.sprite = value.sprite;
            else if (value.renderTexture)
                m_TabHeaderImage.image = value.renderTexture;
            else
                m_TabHeaderImage.vectorImage = value.vectorImage;

            m_TabHeaderImage.RemoveFromClassList(tabHeaderEmptyImageUssClassName);
            m_TabHeaderImage.EnableInClassList(tabHeaderStandaloneImageUssClassName, string.IsNullOrEmpty(m_Label));
            m_IconImage = value;
        }
    }

    public event Action<Tab> selected;

    public Tab()
    {
        AddToClassList(ussClassName);

        m_TabHeader = new VisualElement()
        {
            name = tabHeaderUssClassName
        };
        m_TabHeader.AddToClassList(tabHeaderUssClassName);

        m_TabHeaderImage = new Image()
        {
            name = tabHeaderImageUssClassName,
        };
        m_TabHeaderImage.AddToClassList(tabHeaderImageUssClassName);
        m_TabHeaderImage.AddToClassList(tabHeaderEmptyImageUssClassName);
        m_TabHeader.Add(m_TabHeaderImage);

        m_TabHeaderLabel = new Label()
        {
            name = tabHeaderLabelUssClassName,
        };
        m_TabHeaderLabel.AddToClassList(tabHeaderLabelUssClassName);
        m_TabHeader.Add(m_TabHeaderLabel);

        m_TabHeader.RegisterCallback<PointerDownEvent>(OnTabClicked);

        var tabHeaderUnderline = new VisualElement()
        {
            name = tabHeaderUnderlineUssClassName,
        };
        tabHeaderUnderline.AddToClassList(tabHeaderUnderlineUssClassName);
        m_TabHeader.Add(tabHeaderUnderline);

        hierarchy.Add(m_TabHeader);

        m_ContentContainer = new VisualElement()
        {
            name = contentUssClassName,
            userData = m_TabHeader
        };
        m_ContentContainer.AddToClassList(contentUssClassName);
        hierarchy.Add(m_ContentContainer);

        this.label = label;
        this.iconImage = iconImage;
    }

    public void SetActive()
    {
        // TODO
        //m_TabHeader.pseudoStates |= PseudoStates.Checked;
        //pseudoStates |= PseudoStates.Checked;
        m_TabHeader.AddToClassList(tabHeaderSelectedUssClassName);
        AddToClassList(selectedUssClassName);
    }

    public void SetInactive()
    {
        // TODO
        //m_TabHeader.pseudoStates &= ~PseudoStates.Checked;
        //pseudoStates &= ~PseudoStates.Checked;
        m_TabHeader.RemoveFromClassList(tabHeaderSelectedUssClassName);
        RemoveFromClassList(selectedUssClassName);
    }

    string m_Label;
    Background m_IconImage;
    VisualElement m_ContentContainer;
    VisualElement m_TabHeader;
    Image m_TabHeaderImage;
    Label m_TabHeaderLabel;

    void OnTabClicked(PointerDownEvent _)
    {
        selected?.Invoke(this);
    }
}

public class IconButton : Button
{
    public new static readonly string ussClassName = "unity-button";
    public static readonly string iconUssClassName = ussClassName + "--with-icon";
    public static readonly string imageUSSClassName = ussClassName + "__image";

    public Background iconImage
    {
        get => _iconImage;
        set {
            if (_iconImage == value)
                return;

            _iconImage = value;

            if (_iconImage.texture != null) {
                _image.image = _iconImage.texture;
            } else if (_iconImage.sprite != null) {
                _image.sprite = _iconImage.sprite;
            } else if (_iconImage.renderTexture != null) {
                _image.image = _iconImage.renderTexture;
            } else if (_iconImage.vectorImage != null) {
                _image.vectorImage = _iconImage.vectorImage;
            }
        }
    }

    public override string text
    {
        // This is required for the base class not to render the text
        get => string.Empty; 
        set {
            if (_label.text == value)
                return;

            _label.text = value;
        }
    }

    Background _iconImage;
    Image _image;
    TextElement _label;

    public IconButton()
    {
        _image = new Image();
        _image.AddToClassList(imageUSSClassName);
        Add(_image);
        AddToClassList(iconUssClassName);

        _label = new TextElement();
        Add(_label);
    }
}

}

#endif

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
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

#if !UNITY_2023_2_OR_NEWER
using sttz.TheShelf.Backwards;
using Button = sttz.TheShelf.Backwards.IconButton;
#endif

using UObject = UnityEngine.Object;

namespace sttz.TheShelf {

public class ShelfSettingsTab : Tab
{
    public ShelfSettingsTab(ShelfRack rack)
    {
        this.rack = rack;

        // -- Configure tab header

        if (ShelfAssets.Shared.settingsIcon != null) {
            iconImage = ShelfAssets.BackgroundFromObject(ShelfAssets.Shared.settingsIcon);
        } else {
            label = "…";
        }
        tabHeader.AddToClassList("settings");
        tabHeader.EnableInClassList("shelf-tab-header-has-icon", iconImage != default);
        tabHeader.AddManipulator(new ContextualMenuManipulator(evt => {
            var project = ShelfSettings.Project;

            var status = project.CanToggleRackActive(rack)
                ? DropdownMenuAction.Status.Normal
                : DropdownMenuAction.Status.Disabled;
            if (project.ActiveRack == rack)
                status |= DropdownMenuAction.Status.Checked;

            evt.menu.AppendAction("Active Rack", action => {
                project.ToggleRackActive(rack);
            }, status);

            status = !ShelfSettings.IsUserRack(rack)
                ? DropdownMenuAction.Status.Normal
                : DropdownMenuAction.Status.Disabled;
            evt.menu.AppendAction("Ping Rack", action => {
                EditorGUIUtility.PingObject(rack);
            }, status);
        }));

    #if UNITY_2023_2_OR_NEWER
        delegatesFocus = true;
        contentContainer.delegatesFocus = true;
    #endif

        // -- Create contents

        var scrollView = new ScrollView(ScrollViewMode.Vertical);
        Add(scrollView);

        CreateSettingsGUI(rack, scrollView);

        RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
        RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
    }

    ShelfRack rack;
    ObjectField activeRackField;
    Button activateRackButton;

    void CreateSettingsGUI(ShelfRack rack, VisualElement container)
    {
        // -- Active Rack

        var section = new VisualElement();
        section.AddToClassList("config-section");
        container.Add(section);

        var rackHeader = new Label();
        rackHeader.AddToClassList("config-header");
        rackHeader.text = "Rack";
        section.Add(rackHeader);

        var activeRackContainer = new VisualElement();
        activeRackContainer.AddToClassList("shelf-config-active-rack");
        section.Add(activeRackContainer);

        activeRackField = new ObjectField();
        activeRackField.label = "Active Rack";
        activeRackField.objectType = typeof(ShelfRack);
        activeRackField.allowSceneObjects = false;
        activeRackField.value = ShelfSettings.Project.ActiveRack;
        activeRackField.RegisterValueChangedCallback(e => {
            ShelfSettings.Project.ActiveRack = (ShelfRack)e.newValue;
        });
        activeRackContainer.Add(activeRackField);

        activateRackButton = new Button();
        activateRackButton.text = $"Select \"{rack.name}\"";
        activateRackButton.clicked += () => {
            ShelfSettings.Project.ActiveRack = rack;
        };
        activeRackContainer.Add(activateRackButton);

        // -- Shelves

        section = new VisualElement();
        section.AddToClassList("config-section");
        container.Add(section);

        var headerContainer = new VisualElement();
        headerContainer.AddToClassList("shelves-header");
        section.Add(headerContainer);

        var shelves = new ShelvesView(rack);
        section.Add(shelves);

        var shelvesHeader = new Label();
        shelvesHeader.AddToClassList("config-header");
        shelvesHeader.text = "Shelves";
        headerContainer.Add(shelvesHeader);

        var addButton = new Button();
        addButton.AddToClassList("add-button");
        addButton.text = "Add Shelf";
        addButton.iconImage = ShelfAssets.BackgroundFromObject(ShelfAssets.Shared.addIcon);
        addButton.tooltip = "Add shelf to rack";
        addButton.clicked += () => {
            rack.Shelves.Add(new ShelfRack.Shelf() {
                Name = "New Shelf",
                Items = new List<UObject>()
            });
            shelves.selectedIndex = rack.Shelves.Count - 1;
        };
        headerContainer.Add(addButton);

        // -- Shelf

        section = new VisualElement();
        section.AddToClassList("config-section");
        container.Add(section);

        var configHeader = new Label();
        configHeader.AddToClassList("config-header");
        configHeader.text = "Shelf";
        section.Add(configHeader);

        var shelfConfig = new ShelfConfigView();
        section.Add(shelfConfig);

        Action<IEnumerable<object>> selectionChanged = (selected) => {
            shelfConfig.Shelf = selected.FirstOrDefault() as ShelfRack.Shelf;
        };

    #if UNITY_2022_2_OR_NEWER
        shelves.selectionChanged += selectionChanged;
    #else
        shelves.onSelectionChange += selectionChanged;
    #endif
    }

    void OnAttachToPanel(AttachToPanelEvent evt)
    {
        activeRackField.BindTwoWayWithUserData(ShelfSettings.Project, nameof(ShelfSettings.ActiveRack),
            settings => {
                activateRackButton.SetEnabled(settings.CanToggleRackActive(rack) && settings.ActiveRack != rack);
                return settings.ActiveRack;
            },
            (settings, rack) => settings.ActiveRack = (ShelfRack)rack
        );
    }

    void OnDetachFromPanel(DetachFromPanelEvent evt)
    {
        activeRackField.UnbindWithUserData();
    }

    /// <summary>
    /// List view of shelves on a rack.
    /// </summary>
    public class ShelvesView : ListView
    {
        /// <summary>
        /// Rack this view is presenting.
        /// </summary>
        public ShelfRack Rack => rack;

        ShelfRack rack;
        Dictionary<ShelfRack.Shelf, VisualElement> activeItems = new();

        public ShelvesView(ShelfRack rack) : base()
        {
            this.rack = rack;
            itemsSource = (IList)rack.Shelves;

            virtualizationMethod = CollectionVirtualizationMethod.FixedHeight;
            selectionType = SelectionType.Single;
            showAlternatingRowBackgrounds = AlternatingRowBackground.None;
            reorderable = true;
            reorderMode = ListViewReorderMode.Simple;

            makeItem = MakeItem;
            bindItem = BindItem;
            unbindItem = UnbindItem;

            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
        }

        void OnAttachToPanel(AttachToPanelEvent args)
        {
            if (rack != null) {
                rack.propertyChanged += OnRackPropertyChanged;
            }
        }

        void OnDetachFromPanel(DetachFromPanelEvent args)
        {
            if (rack != null) {
                rack.propertyChanged -= OnRackPropertyChanged;
            }
        }

        void OnRackPropertyChanged(object sender, BindablePropertyChangedEventArgs args)
        {
            if (args.propertyName == nameof(ShelfRack.Shelves)) {
                RefreshItems();
            }
        }

        VisualElement MakeItem()
        {
            var container = new VisualElement();
            container.AddToClassList("shelf-item");

            var box = new Box();

            var icon = new Image();
            icon.AddToClassList("shelf-icon");
            box.Add(icon);

            var label = new Label();
            box.Add(label);

            var accessoryContainer = new VisualElement();
            accessoryContainer.AddToClassList("shelf-item-accessory");
            box.Add(accessoryContainer);

            var button = new Button();
            button.AddToClassList("remove-button");
            button.tooltip = "Remove from rack";
            button.iconImage = ShelfAssets.BackgroundFromObject(ShelfAssets.Shared.removeIcon);
            button.text = "x";
            button.clicked += () => ItemRemove(container);
            box.Add(button);

            container.Add(box);
            return container;
        }

        void BindItem(VisualElement element, int index)
        {
            var shelf = rack.Shelves[index];
            shelf.propertyChanged += OnShelfPropertyChanged;

            element.userData = shelf;
            activeItems[shelf] = element;

            var icon = element.Q<Image>();
            icon.BindWithUserData(
                shelf,
                nameof(ShelfRack.Shelf.Icon),
                shelf => {
                    ShelfAssets.ApplyObjectToImage(icon, shelf.Icon);
                    icon.EnableInClassList("shelf-icon-empty", shelf.Icon == null);
                }
            );

            var label = element.Q<Label>();
            label.BindWithUserData(
                shelf,
                nameof(ShelfRack.Shelf.Name),
                shelf => label.text = shelf.Name
            );

            UpdateColors(shelf, element);
        }

        void UnbindItem(VisualElement element, int index)
        {
            if (element.userData is not ShelfRack.Shelf shelf)
                return;

            shelf.propertyChanged -= OnShelfPropertyChanged;
            activeItems.Remove(shelf);

            var icon = element.Q<Image>();
            icon.UnbindWithUserData();

            var label = element.Q<Label>();
            label.UnbindWithUserData();
        }

        void ItemRemove(VisualElement itemRoot)
        {
            if (itemRoot.userData is not ShelfRack.Shelf shelf)
                return;

            rack.Shelves.Remove(shelf);
        }

        void OnShelfPropertyChanged(object sender, BindablePropertyChangedEventArgs args)
        {
            var shelf = (ShelfRack.Shelf)sender;

            if (!activeItems.TryGetValue(shelf, out var element))
                return;

            UpdateColors(shelf, element);
        }

        void UpdateColors(ShelfRack.Shelf shelf, VisualElement element)
        {
            var box = element.Q<Box>();

            if (shelf.HeaderTextColor.r < 0) {
                box.style.color = new StyleColor(StyleKeyword.Null);
            } else {
                box.style.color = new StyleColor(shelf.HeaderTextColor);
            }

            if (shelf.HeaderBackgroundColor.r < 0) {
                box.style.backgroundColor = new StyleColor(StyleKeyword.Null);
            } else {
                box.style.backgroundColor = new StyleColor(shelf.HeaderBackgroundColor);
            }
        }
    }

    /// <summary>
    /// View to configure a single shelf.
    /// </summary>
    public class ShelfConfigView : VisualElement
    {
        /// <summary>
        /// The inspected shelf.
        /// </summary>
        public ShelfRack.Shelf Shelf {
            get => _shelf;
            set {
                if (_shelf == value) return;
                var oldValue = _shelf;
                _shelf = value;
                OnShelfChanged(oldValue, _shelf);
            }
        }
        ShelfRack.Shelf _shelf;

        public enum IconType
        {
            Texture,
            Sprite,
            VectorImage
        }

        TextField nameField;
        EnumField sizeField;
        ObjectField iconField;
        EnumField iconTypeField;
        ColorField textColorField;
        Button textColorResetButton;
        ColorField backgroundColorField;
        Button backgroundColorResetButton;

        public ShelfConfigView() : base()
        {
            nameField = new TextField();
            nameField.label = "Name";
            Add(nameField);

            var iconContainer = new VisualElement();
            iconContainer.AddToClassList("shelf-config-icon");
            Add(iconContainer);

            iconField = new ObjectField();
            iconField.objectType = typeof(Sprite);
            iconField.label = "Icon";
            iconContainer.Add(iconField);

            iconTypeField = new EnumField(IconType.Sprite);
            iconTypeField.RegisterValueChangedCallback(evt => {
                UpdateIconField();
            });
            iconContainer.Add(iconTypeField);

            var textColorContainer = new VisualElement();
            textColorContainer.AddToClassList("shelf-config-header-color");
            Add(textColorContainer);

            textColorField = new ColorField();
            textColorField.label = "Text Color";
            textColorContainer.Add(textColorField);

            textColorResetButton = new Button();
            textColorResetButton.text = "Reset";
            textColorResetButton.clicked += () => {
                if (Shelf == null) return;
                Shelf.HeaderTextColor = new Color(-1, -1, -1, 1);
            };
            textColorContainer.Add(textColorResetButton);

             var backgroundColorContainer = new VisualElement();
            backgroundColorContainer.AddToClassList("shelf-config-header-color");
            Add(backgroundColorContainer);

            backgroundColorField = new ColorField();
            backgroundColorField.label = "Background Color";
            backgroundColorContainer.Add(backgroundColorField);

            backgroundColorResetButton = new Button();
            backgroundColorResetButton.text = "Reset";
            backgroundColorResetButton.clicked += () => {
                if (Shelf == null) return;
                Shelf.HeaderBackgroundColor = new Color(-1, -1, -1, 1);
            };
            backgroundColorContainer.Add(backgroundColorResetButton);

            sizeField = new EnumField(ShelfItemSize.Default);
            sizeField.label = "Item Size";
            Add(sizeField);

            RegisterCallback<AttachToPanelEvent>(OnAttachToPanelEvent);
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);

            OnShelfChanged(null, null);
        }

        void UpdateIconField()
        {
            iconField.objectType = iconTypeField.value switch {
                IconType.Texture      => typeof(Texture),
                IconType.Sprite       => typeof(Sprite),
                IconType.VectorImage  => typeof(VectorImage),
                _                     => typeof(Sprite),
            };
        }

        void OnShelfChanged(ShelfRack.Shelf oldVlaue, ShelfRack.Shelf newValue)
        {
            if (oldVlaue != null) {
                nameField.UnbindWithUserData();
                sizeField.UnbindWithUserData();
                iconField.UnbindWithUserData();
                textColorField.UnbindWithUserData();
                backgroundColorField.UnbindWithUserData();
            }

            if (newValue != null) {
                nameField.BindTwoWayWithUserData(
                    newValue, nameof(ShelfRack.Shelf.Name), 
                    (shelf) => shelf.Name,
                    (shelf, name) => shelf.Name = name
                );

                iconField.BindTwoWayWithUserData(
                    newValue, nameof(ShelfRack.Shelf.Icon),
                    (shelf) => shelf.Icon,
                    (shelf, icon) => shelf.Icon = icon
                );
                if (iconField.value is Texture) {
                    iconTypeField.value = IconType.Texture;
                } else if (iconField.value is Sprite) {
                    iconTypeField.value = IconType.Sprite;
                } else if (iconField.value is VectorImage) {
                    iconTypeField.value = IconType.VectorImage;
                } else {
                    iconTypeField.value = IconType.Sprite;
                }
                UpdateIconField();

                textColorField.BindTwoWayWithUserData(
                    newValue, nameof(ShelfRack.Shelf.HeaderTextColor),
                    (shelf) => shelf.HeaderTextColor,
                    (shelf, color) => shelf.HeaderTextColor = color
                );
                backgroundColorField.BindTwoWayWithUserData(
                    newValue, nameof(ShelfRack.Shelf.HeaderBackgroundColor),
                    (shelf) => shelf.HeaderBackgroundColor,
                    (shelf, color) => shelf.HeaderBackgroundColor = color
                );

                sizeField.BindTwoWayWithUserData(
                    newValue, nameof(ShelfRack.Shelf.ItemSize), 
                    (shelf) => shelf.ItemSize,
                    (shelf, itemSize) => shelf.ItemSize = (ShelfItemSize)itemSize
                );
            }

            nameField.SetEnabled(newValue != null);
            sizeField.SetEnabled(newValue != null);
            textColorField.SetEnabled(newValue != null);
            textColorResetButton.SetEnabled(newValue != null);
            backgroundColorField.SetEnabled(newValue != null);
            backgroundColorResetButton.SetEnabled(newValue != null);
            iconField.SetEnabled(newValue != null);
            iconTypeField.SetEnabled(newValue != null);
        }

        void OnAttachToPanelEvent(AttachToPanelEvent args)
        {
            // Rebind on attach
            if (Shelf != null) {
                OnShelfChanged(null, Shelf);
            }
        }

        void OnDetachFromPanel(DetachFromPanelEvent args)
        {
            // Trigger unbinding
            OnShelfChanged(Shelf, null);
        }
    }
}

}

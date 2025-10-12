-------------------------------------------------------
-- A Shelf for the Unity Editor - v3.0.3
-------------------------------------------------------
-- © 2024 Adrian Stutz (adrian@sttz.ch)
-------------------------------------------------------

-- Table of Contents

1. Included Parts
2. Assembly Instructions
3. Making the Most Out of Your New Shelf
3.1 Shelf Racks
3.2 Shortcuts
3.3 Popup Rack
3.4 Item Action & Context Menu
3.5 Dragging items
4. Version History
5. Support & Contact



-- 1. Included Parts

Thanks for buying The Shelf! Here's a quick rundown
of what's included in your purchase:

* Create racks (assets) and add shelves (tabs) to
  organize references to assets and scene objects.
* Special "User Rack" that lives in in UserSettings,
  to stay out of the way from version control.
* Simple drag & drop interface: Drag assets to and
  from shelves, multi-select to drag many items at
  once, and drag to and from shelf tab headers.
* A set of shortcuts to quickly open and put items
  on the active rack.
* Make any rack asset active to switch between tasks
  or contexts.
* Different click and context menu actions depending 
  on the asset type.
* Customize The Shelf using APIs to change click
  actions and context menus or add your own UI Toolkit
  style sheet.



-- 2. Assembly Instructions

As soon as you've added The Shelf package to your
Unity project, you're good to go.

Press Shift-1 or select "Window/The Shelf/Open User Rack"
from the menu to show the User Rack. Drag and drop
any project asset, scene object or prefab object onto 
a shelf to add a reference to it.

Click the gear icon to the far right of the tab bar 
to open the rack settings. Here you can add, remove
and reorder shelves, as well as configuring shelf
names and item size.

-- TIP: Starting with Unity 2023.2, you can use emoji
in your shelf names. --

In Project Settings, there's a section containing
configuration options for The Shelf.



-- 3. Making the most out of your new Shelf

- 3.1 Shelf Racks

Racks are collections of shelves. Racks can be viewed 
in the inspector or opened in a window.

There's also a special "User Rack" that is stored
in the UserSettings folder. This folder should
not be checked into version control and therefore
the User Rack can be changed without affecting
the versioned project.

Every project comes with a User Rack, additional
asset racks can be created using "Create > Shelf Rack".

-- NOTE: Rack assets are editor-only, you should
avoid referencing them from non-editor code, which
can lead to build compile errors or missing script
errors when running the build. --

Items can be added to shelves by dragging them 
onto one or by using the Alt+Shift+Number shortcuts.
Items on shelves can be dragged to reorder them.
Hold alt/shift (macOS) or ctrl/shift (Windows) to
select multiple items. Press command/ctrl-A to 
select all items on a shelf.

Shelves can accept references to any project asset 
in Unity (i.e. what you can see in the Project view).

There are also two special types of references:

Scene objects:

Game Objects, Components, and other Unity objects
contained in scenes can also be put on shelves.
They show a scene icon and the scene name next
to their own name.

If the scene is loaded, a single click will select
the object. If the scene isn't loaded, the click
will only select the containing scene. Double-click
the item to open the scene and select the object.

Note that when the scene isn't loaded, the 
reference cannot be used. The item can still be
dragged between shelves but cannot be dragged
to other parts in Unity. First load the scene
and then drag the item to drag its reference.

Prefab objects:

Prefabs can be added as regular assets to shelves.
In addition, game objects and components inside
prefabs can also be added. When the prefab is
open, clicking those items will select the object
inside the prefab. If the prefab is not open,
clicking will select the prefab asset. Double-click
the item to open the prefab and select the object.

Note that references to child objects inside 
prefabs are not supported in Unity. So instead,
dragging a prefab object drags the root prefab
asset instead.

- 3.2 Shortcuts

The Shelf defines a set of shortcuts to quickly
open shelves and put the current selection on a shelf.

These shortcuts can be found in the "Window > The Shelf"
menu and, by default, are mapped to Shift+Number to
open a shelf and Alt+Shift+Number to put the 
current selection on a shelf. You can change these
shortcuts in Unity's Shortcut Manager.

If a rack is open in a window and that window has focus,
that window will receive the shortcuts. However,
if no rack window has focus, the active rack will
receive the shortcuts instead.

By default, the "User Rack" is the active rack. In
any rack window, you can click the three-dots menu
in the top-right and activate that rack. If the 
active rack is deactivated, the User Rack will become
the active rack again.

-- TIP: There are more commands in the "Window > The Shelf"
menu that are not mapped to a shortcut by default. If
you use them often, you can use Unity's Shortcut Manager
to set up keyboard shortcuts for them. --

- 3.3 Popup Rack

The active rack opens as a special popup window. 
The popup window will be opened next to the tab 
the mouse is over, keeping it close within reach 
but not covering what you're interacting with.
The popup will also close automatically after 
it loses focus.

If the active rack is already open as a regular window,
that window will be focused instead of opening it as
a popup. Close the window first to then open it
as a popup using the shortcuts.

The popup mode can be deactivated in The Shelf's Project
Settings or in the "Window > The Shelf" menu.

-- TIP: In the popup, there's an additional button
next to the settings tab, to turn the popup into
a regular window. --

- 3.4 Item Action & Context Menu

Clicking an item on a shelf will select that item,
which will also show it in the Inspector for editing.

Double-clicking an item will open it. For most items,
this is the same as double-clicking it in the 
project window, opening native assets inside Unity
or opening them in the default external editor.

Single-clicking folders will focus them in the 
project view. Double-clicking does nothing.

For scene and prefab objects, a single click will
select the containing asset if it isn't open or
select the object if it is. A double-click will
open the containing asset and select the object.

When opening scenes, either double-clicking a 
scene asset or double-clicking a scene object,
the default open action will be executed, usually
replacing all open scenes. Holding the alt key 
will instead open the scene additively.

-- TIP: You can customize the opening behavior
using Unity's OnOpenAsset attribute. This also works
for scenes and other native Unity assets. This way,
opening them on The Shelf will do the same as when
you open them in the Project view. --

Right-click an item to open a context menu. The 
context menu allows you to manage the item on the 
shelf (removing or moving it to another shelf),
ping it to reveal it without changing the current
selection, or open the Properties window for that
item.

Specific item types will show additional actions,
like adding a component, instantiating a prefab,
creating an asset from a scriptable object,
or opening a scene in single or additive mode.

The context menu actions also work with multiple
selected items, executing them on all applicable
items.

- 3.5 Dragging items

Items can be dragged onto tab headers to add them
to that shelf. Holding the drag over a tab header
for a short while will open the tab.

Dragging from a tab header will drag all items
from the shelf. Dragging a tab into another rack
will move the whole shelf. Holding alt will copy
the shelf instead.

Dragging items between shelves will move the items.
Holding alt will copy the items instead.

-- Note: Unity does not update while holding the
mouse still during a drag. When trying to open
a tab during a drag, make sure you keep moving the
mouse to trigger the hover timeout. --



-- 4. Version History

- v3.0.3
  * Fix reflection error on newer Unity versions that limited
    popup rack placement functionality
    (Appeared in 6000.1.0, 6000.0.49 and 2022.3.62)

- v3.0.2
  * Fix a compile error with Unity 2023.2+ on Windows

- v3.0.1
  * Clip text instead of icon when tab header space is limited
  * Fix rack assets changes not being saved

- v3.0
  * Now requires Unity 2021.3+
  * Completely rewritten using UI Toolkit
  * Support for multiple racks, each containing its set of shelves
  * Racks asset contents are also shown in the Inspector
  * The default "User Rack" lives in UserSettings, to not clog up the project assets
  * Any number of additional racks can be created as project assets
  * Scene objects are now referenced uniquely in a specific scene
  * Added support to add objects inside prefabs to shelves
  * Added shortcut object for quick access to Preferences, Project Settings, or to execute menu items
  * Extensible API for click and context menu actions
  * Customizable size of items (per shelf or per project)
  * Customizable shelf icons and tab colors
  * Add override stylesheet for even more visual customization

- v2.5
  * Now requires Unity 2017.4 and supports up to 2019.3
  * Add option to show asset previews
  * Replaced "i" (instantiate) button with "↓" and "a" (add component) with "＋"
  * Fixed layout issues on 2019.3
  * Fixed PreferenceItem deprecation warning on 2019.1+
  * Fixed add component button shown for non-MonoBehaviour scripts

- v2.1
  * Add support for Unity 5.5 and 5.6 beta
  * Fix dragging item over its own list in newer Unity versions
  * Fix items sometimes being inserted one row above
  * Create directories if necessary when creating default asset
  * Requires Unity 4.7

- v2.0.1
  * Ask to save the current scene before opening one from the shelf

- v2.0
  * Pop-up shelf to save screen real estate
  * Multi-selection in shelf
  * Shortcuts to put current Unity selection on shelves
  * Quick access to Shelf preferences through window menu
  * Select folder only in left column in two-column project view
  * Configurable number of shelf keyboard shortcuts
  * Scripts are now in their own namespace to avoid conflicts
  * Requires Unity 4.3 due to new Undo system

- v1.2
  * Fix icon size in Unity 4
  * Use default color for text buttons, improves visibility using Dark skin
  * Fix an index out of range exception

- v1.1
  * Add scroll bars when items don't fit into the shelf
  * Clicking on scenes will open instead of selecting them
  * Scripts have an additional "a" button to add them to the selected game object
  * Prefabs have an additional "i" button to instantiate them
  * Remember the currently selected shelf
  * Prevent shelf items from jumping slightly when rearranging them

- v1.0
  * Initial release



-- 5. Support & Contact

The Shelf is being developed by Adrian Stutz.
For support and other inquiries, contact
Adrian at <adrian@sttz.ch>.

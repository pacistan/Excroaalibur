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
using UnityEngine;
using UnityEngine.UIElements;

namespace sttz.TheShelf {

/// <summary>
/// Window presenting a <see cref="ShelfRack"/> in the inspector.
/// </summary>
[CustomEditor(typeof(ShelfRack), true)]
public class ShelfEditor : Editor
{
    public override VisualElement CreateInspectorGUI()
    {
        var rackView = new ShelfRackView();
        rackView.AddToClassList("rack-view-editor");

        rackView.Rack = target as ShelfRack;

        return rackView;
    }
}

}

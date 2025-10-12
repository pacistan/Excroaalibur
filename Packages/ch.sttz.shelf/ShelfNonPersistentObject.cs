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
using UnityEngine;
using UnityEngine.UIElements;

using UObject = UnityEngine.Object;

namespace sttz.TheShelf {

/// <summary>
/// Wrapper to show a warning for non-persistent references.
/// </summary>
/// <remarks>
/// Non-persistent references (non-assets) cannot be stored permanently
/// in Unity. After a domain reload or editor restart, the reference will
/// become `null`. Specific workarounds need to implemented to map
/// non-persitent objects to a persitent ID and back (i.e.
/// <see cref="ShelfSceneObject"/> and <see cref="ShelfPrefabObject"/>).
/// This wrapper only serves a visual purpose, to mark such
/// volatile reference in the UI and provide a bit more context
/// when they are lost.
/// </remarks>
public class ShelfNonPersistentObject : ShelfAbstractObject<ShelfNonPersistentObject.Status>, IShelfItem
{
    // ---------- Properties ----------

    /// <summary>
    /// Reference to non-persitent object.
    /// </summary>
    [SerializeField] UObject reference;

    /// <summary>
    /// The type name of the reference, 
    /// used when the reference cannot be resolved.
    /// </summary>
    [SerializeField] string referenceTypeName;

    // ---------- API ----------

    /// <summary>
    /// The status of the reference.
    /// </summary>
    public enum Status
    {
        /// <summary>
        /// The status hasn't been determined.
        /// </summary>
        Unkown,
        /// <summary>
        /// The reference is null (object has been deleted).
        /// </summary>
        Lost,
        /// <summary>
        /// The object has been resolved.
        /// </summary>
        Resolved,
    }

    /// <summary>
    /// The cached resolved reference of the object Id.
    /// </summary>
    public UObject Reference
    {
        get => reference;
        set {
            if (reference == value) return;
            reference = value;
            RefreshReference();
        }
    }

    /// <summary>
    /// The type of the object the reference points to.
    /// </summary>
    public Type ReferenceType
    {
        get {
            if (_referenceType == null && !string.IsNullOrEmpty(referenceTypeName)) {
                _referenceType = Type.GetType(referenceTypeName);
            }
            return _referenceType;
        }
        set {
            if (_referenceType == value) return;
            _referenceType = value;

            if (_referenceType == null) {
                referenceTypeName = null;
            } else {
                if (_referenceType.Assembly == null) {
                    referenceTypeName = _referenceType.FullName;
                } else {
                    referenceTypeName = _referenceType.FullName + ", " + _referenceType.Assembly.GetName().Name;
                }
            }
        }
    }
    Type _referenceType;

    /// <summary>
    /// Create a copy of the scene object reference.
    /// </summary>
    public UObject Clone()
    {
        var clone = CreateInstance<ShelfNonPersistentObject>();
        clone.name = name;
        clone.reference = reference;
        return clone;
    }

    /// <summary>
    /// Get a short description for the given <see cref="Status"/>.
    /// </summary>
    public static string DescriptionForStatus(Status status) => status switch {
        Status.Unkown => "Undetermined",
        Status.Lost => "Non-persistent reference has been lost",
        Status.Resolved => "Reference that will likely be lost on domain reload or restart",
        _ => "Invalid status",
    };

    // ---------- IShelfItem ----------

    string IShelfItem.Name => name;

    Texture IShelfItem.Icon => ShelfItemsView.GetIconTexture(reference, ReferenceType);

    UObject IShelfItem.Reference => reference;

    VisualElement IShelfItem.Accessory { get {
        var accessory = new VisualElement();
        accessory.AddToClassList("shelf-wrapped-object");
        accessory.AddToClassList("shelf-non-persistent-object");

        var icon = new Image();
        accessory.Add(icon);

        var label = new Label();
        accessory.Add(label);

        RefreshReference();

        BindToReferenceStatus(accessory, status => {
            accessory.tooltip = DescriptionForStatus(status);

            if (status == Status.Resolved) {
                icon.image = null;
                label.text = "Non-persistent";
            } else {
                ShelfAssets.ApplyObjectToImage(icon, ShelfAssets.Shared.warningIcon);
                label.text = "Not Found";
            }
        });

        return accessory;
    } }

    // ---------- Internals ----------

    void RefreshReference()
    {
        if (this == null) return;

        if (reference == null) {
            ReferenceStatus = Status.Lost;
            return;
        }

        name = reference.name;
        ReferenceType = reference.GetType();
        ReferenceStatus = Status.Resolved;
    }
}

}

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
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

using UObject = UnityEngine.Object;

namespace sttz.TheShelf {

/// <summary>
/// Wrapper to put references to scene objects on shelves.
/// </summary>
/// <remarks>
/// Unity can only serialize references to assets. You can
/// technically put a scene object on a shelf, but as soon
/// as the scene is reloaded, that reference becomes invalid.
/// This wrapper uses <see cref="GlobalObjectId"/> to create
/// a permanent reference to scene objects.
/// </remarks>
public class ShelfSceneObject : ShelfAbstractObject<ShelfSceneObject.Status>, IShelfItem
{
    // ---------- Properties ----------

    /// <summary>
    /// The string representation of the global object Id of the target object.
    /// </summary>
    [SerializeField] string globalIdString;

    /// <summary>
    /// The type name of the reference, 
    /// used when the reference cannot be resolved,
    /// e.g. when the containing scene is not loaded.
    /// </summary>
    [SerializeField] string referenceTypeName;

    // ---------- API ----------

    /// <summary>
    /// The status of the scene reference.
    /// </summary>
    public enum Status
    {
        /// <summary>
        /// The status hasn't been determined.
        /// </summary>
        Unkown,
        /// <summary>
        /// The reference is invalid (not set or malformed).
        /// </summary>
        Invalid,
        /// <summary>
        /// The containing scene asset could not be found.
        /// </summary>
        SceneNotFound,
        /// <summary>
        /// The scene has been found but is not loaded to look for the object.
        /// </summary>
        SceneNotLoaded,
        /// <summary>
        /// The object in the scene could not be found.
        /// </summary>
        ObjectNotFound,
        /// <summary>
        /// The object has been resolved.
        /// </summary>
        Resolved,
    }

    /// <summary>
    /// Returns wether <see cref="globalIdString"/> is not set to the `null` id.
    /// </summary>
    public bool IsSet => !string.IsNullOrEmpty(globalIdString) 
        && GlobalObjectId.TryParse(globalIdString, out var globalId) 
        && globalId.identifierType != 0;

    /// <summary>
    /// The asset containing the reference.
    /// </summary>
    public UObject ContainerAsset
    {
        get {
            if (_containerAsset == null) {
                Resolve();
            }
            return _containerAsset;
        }
    }
    UObject _containerAsset;

    /// <summary>
    /// The cached resolved reference of the object Id.
    /// </summary>
    public UObject Reference
    {
        get {
            if (_reference == null) {
                Resolve();
            }
            return _reference;
        }
        set {
            if (_reference == value) return;
            _reference = value;

            var globalId = GlobalObjectId.GetGlobalObjectIdSlow(value);
            globalIdString = globalId.ToString();
            _containerAsset = GetContainerAsset(globalId);

            RefreshFromReference(_reference);
        }
    }
    UObject _reference;

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
        var clone = CreateInstance<ShelfSceneObject>();
        clone.name = name;
        clone.globalIdString = globalIdString;
        clone.referenceTypeName = referenceTypeName;
        return clone;
    }

    /// <summary>
    /// Get a short description for the given <see cref="Status"/>.
    /// </summary>
    public static string DescriptionForStatus(Status status) => status switch {
        Status.Unkown => "Undetermined",
        Status.Invalid => "Reference not set or malformed",
        Status.SceneNotFound => "Containing scene not found",
        Status.SceneNotLoaded => "Containing scene found but not loaded",
        Status.ObjectNotFound => "Referenced object not found in scene",
        Status.Resolved => "Reference found",
        _ => "Invalid status",
    };

    // ---------- IShelfItem ----------

    string IShelfItem.Name => name;

    Texture IShelfItem.Icon => ShelfItemsView.GetIconTexture(Reference, ReferenceType);

    UObject IShelfItem.Reference => Reference;

    VisualElement IShelfItem.Accessory { get {
        var accessory = new VisualElement();
        accessory.AddToClassList("shelf-wrapped-object");
        accessory.AddToClassList("shelf-scene-object");

        var icon = new Image();
        accessory.Add(icon);

        var label = new Label();
        accessory.Add(label);

        if (ReferenceStatus == Status.Unkown)
            Resolve();

        BindToReferenceStatus(accessory, status => {
            accessory.tooltip = DescriptionForStatus(status);
            label.text = _containerAsset != null ? _containerAsset.name : "<Not Found>";

            if (status == Status.SceneNotLoaded || status == Status.Resolved) {
                icon.image = AssetPreview.GetMiniTypeThumbnail(typeof(SceneAsset));
            } else {
                ShelfAssets.ApplyObjectToImage(icon, ShelfAssets.Shared.warningIcon);
            }
        });

        return accessory;
    } }

    // ---------- Internals ----------

    void Resolve()
    {
        if (!GlobalObjectId.TryParse(globalIdString, out var globalId)) {
            ReferenceStatus = Status.Invalid;
            return;
        }
        if (globalId.identifierType == 0) {
            ReferenceStatus = Status.Invalid;
            return;
        }

        _reference = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(globalId);
        _containerAsset = GetContainerAsset(globalId);

        if (_reference != null) {
            ReferenceStatus = Status.Resolved;
            RefreshFromReference(_reference);
        } else if (_containerAsset == null) {
            ReferenceStatus = Status.SceneNotFound;
        } else {
            var scenePath = AssetDatabase.GetAssetPath(_containerAsset);
            var scene = SceneManager.GetSceneByPath(scenePath);
            if (!scene.IsValid() || !scene.isLoaded) {
                ReferenceStatus = Status.SceneNotLoaded;
            } else {
                ReferenceStatus = Status.ObjectNotFound;
            }
        }
    }

    UObject GetContainerAsset(GlobalObjectId globalId)
    {
        if (globalId.assetGUID.Empty())
            return null;

        var path = AssetDatabase.GUIDToAssetPath(globalId.assetGUID);
        if (string.IsNullOrEmpty(path))
            return null;

        return AssetDatabase.LoadMainAssetAtPath(path);
    }

    void RefreshFromReference(UObject reference)
    {
        if (reference == null || this == null) return;

        name = reference.name;
        ReferenceType = reference.GetType();
        ReferenceStatus = Status.Resolved;
    }
}

}

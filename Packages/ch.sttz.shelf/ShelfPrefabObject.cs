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
using UnityEngine;
using UnityEngine.UIElements;

using UObject = UnityEngine.Object;

namespace sttz.TheShelf {

/// <summary>
/// Wrapper to put references to prefab objects on shelves.
/// </summary>
/// <remarks>
/// Objects inside prefabs are a bit tricky, since Unity instantiates them
/// while editing and overwrites the whole prefab on save.
/// There's no public mapping of prefab sub-assets to the corresponding
/// instantiated object. We need to use internal APIs to get the 
/// local file identifier and recursively search the prefab contents
/// and sub-assets for a match (internally the Unity editor does something
/// similar).
/// 
/// Another issue is that references to children inside prefabs
/// aren't useful. There's no place in the editor where such a 
/// reference can be dragged to. So, dragging a prefab reference
/// drags a reference to the root prefab asset, only when clicking
/// on a prefab reference does it matter what child it points to.
/// </remarks>
public class ShelfPrefabObject : ShelfAbstractObject<ShelfPrefabObject.Status>, IShelfItem
{
    // ---------- Properties ----------

    /// <summary>
    /// The GUID of the prefab (variant) asset.
    /// </summary>
    [SerializeField] string prefabGuid;
    /// <summary>
    /// The local file identifier of the prefab sub-object.
    /// </summary>
    [SerializeField] ulong localFileId;
    /// <summary>
    /// The type name of the reference, 
    /// used when the reference cannot be resolved.
    /// </summary>
    [SerializeField] string referenceTypeName;

    // ---------- API ----------

    /// <summary>
    /// The status of the prefab reference.
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
        /// The containing prefab asset could not be found.
        /// </summary>
        PrefabNotFound,
        /// <summary>
        /// The object in the prefab could not be found.
        /// </summary>
        ObjectNotFound,
        /// <summary>
        /// The object has been resolved.
        /// </summary>
        Resolved,
    }

    /// <summary>
    /// Returns wether <see cref="prefabGuid"> and <see cref="localFileId"/> are set.
    /// </summary>
    public bool IsSet => !string.IsNullOrEmpty(prefabGuid) && localFileId != 0;

    /// <summary>
    /// The local file identifier of this reference inside the prefab asset.
    /// </summary>
    public ulong LocalFileId => localFileId;

    /// <summary>
    /// The prefab asset containing the reference.
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
    UObject _reference;

    /// <summary>
    /// For prefab objects, this reference is equivalent to
    /// <see cref="ContainerAsset"/>, since child references
    /// cannot be used in the editor.
    /// </summary>
    public UObject Reference => ContainerAsset;

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
    /// Set the reference this instance points to using a GUID and local file identifier.
    /// </summary>
    public void Assign(string prefabGuid, ulong localFileId)
    {
        this.prefabGuid = prefabGuid;
        this.localFileId = localFileId;
        Resolve();
    }

    /// <summary>
    /// Create a copy of the scene object reference.
    /// </summary>
    public UObject Clone()
    {
        var clone = CreateInstance<ShelfPrefabObject>();
        clone.name = name;
        clone.prefabGuid = prefabGuid;
        clone.localFileId = localFileId;
        clone.referenceTypeName = referenceTypeName;
        return clone;
    }

    /// <summary>
    /// Get a short description for the given <see cref="Status"/>.
    /// </summary>
    public static string DescriptionForStatus(Status status) => status switch {
        Status.Unkown => "Undetermined",
        Status.Invalid => "Reference not set or malformed",
        Status.PrefabNotFound => "Containing prefab not found",
        Status.ObjectNotFound => "Referenced object not found in prefab",
        Status.Resolved => "Reference found",
        _ => "Invalid status",
    };

    // ---------- IShelfItem ----------

    string IShelfItem.Name => name;

    Texture IShelfItem.Icon => ShelfItemsView.GetIconTexture(_reference, ReferenceType);

    UObject IShelfItem.Reference => Reference;

    VisualElement IShelfItem.Accessory { get {
        var accessory = new VisualElement();
        accessory.AddToClassList("shelf-wrapped-object");
        accessory.AddToClassList("shelf-prefab-object");

        var icon = new Image();
        accessory.Add(icon);

        var label = new Label();
        accessory.Add(label);

        if (ReferenceStatus == Status.Unkown)
            Resolve();

        BindToReferenceStatus(accessory, status => {
            accessory.tooltip = DescriptionForStatus(status);
            label.text = _containerAsset != null ? _containerAsset.name : "<Not Found>";

            if (status == Status.Resolved) {
                icon.image = AssetPreview.GetMiniThumbnail(ContainerAsset);
            } else {
                ShelfAssets.ApplyObjectToImage(icon, ShelfAssets.Shared.warningIcon);
            }
        });

        return accessory;
    } }

    // ---------- Static API ----------

    /// <summary>
    /// Get the actual local file identifier or the file identifier that will
    /// be used when the object is saved.
    /// Calls the internal method `Unsupported.GetOrGenerateFileIDHint`.
    /// </summary>
    public static ulong GetOrGenerateFileIDHint(UObject target)
    {
        if (!FindMethod()) return 0;

        GetOrGenerateFileIDHintArgs[0] = target;
        if (GetOrGenerateFileIDHintMethod.Invoke(null, GetOrGenerateFileIDHintArgs) is ulong fileId) {
            GetOrGenerateFileIDHintArgs[0] = null;
            return fileId;
        }

        GetOrGenerateFileIDHintArgs[0] = null;
        return 0;
    }

    /// <summary>
    /// Find the game object or component with the given local file identifier
    /// inside the given prefab contents.
    /// </summary>
    /// <param name="gameObject">The `PrefabStage.prefabContentsRoot` or result of `PrefabUtility.LoadPrefabContents`</param>
    /// <param name="fileId">The local file identifier to look for</param>
    /// <returns>The corresponding game object or component or `null` if nothing was found</returns>
    public static UObject FindFileIdentifierInPrefabContents(GameObject gameObject, ulong fileId)
    {
        var goId = GetOrGenerateFileIDHint(gameObject);
        if (goId == fileId) return gameObject;

        components ??= new();
        gameObject.GetComponents(typeof(Component), components);
        foreach (var comp in components) {
            var compId = GetOrGenerateFileIDHint(comp);
            if (compId == fileId) return comp;
        }

        foreach (Transform child in gameObject.transform) {
            var childResult = FindFileIdentifierInPrefabContents(child.gameObject, fileId);
            if (childResult != null) return childResult;
        }

        return null;
    }

    // ---------- Static Internals ----------

    static MethodInfo GetOrGenerateFileIDHintMethod;
    static object[] GetOrGenerateFileIDHintArgs;
    static List<Component> components;

    static bool FindMethod()
    {
        if (GetOrGenerateFileIDHintMethod != null)
            return true;

        GetOrGenerateFileIDHintMethod = typeof(Unsupported).GetMethod(
            "GetOrGenerateFileIDHint", 
            BindingFlags.Static | BindingFlags.NonPublic
        );

        if (GetOrGenerateFileIDHintMethod != null && GetOrGenerateFileIDHintArgs == null) {
            GetOrGenerateFileIDHintArgs = new object[1];
        }

        return GetOrGenerateFileIDHintMethod != null;
    }

    // ---------- Internals ----------

    void Resolve()
    {
        if (string.IsNullOrEmpty(prefabGuid) || localFileId == 0) {
            ReferenceStatus = Status.Invalid;
            return;
        }

        var path = AssetDatabase.GUIDToAssetPath(prefabGuid);
        if (string.IsNullOrEmpty(path)) {
            ReferenceStatus = Status.PrefabNotFound;
            return;
        }

        if (!FindMethod()) {
            ReferenceStatus = Status.ObjectNotFound;
            return;
        }

        UObject asset = null;
        var assets = AssetDatabase.LoadAllAssetsAtPath(path);
        foreach (var candidate in assets) {
            var candidateFileId = GetOrGenerateFileIDHint(candidate);

            if (candidateFileId == localFileId) {
                asset = candidate;
                break;
            }
        }

        _reference = asset;
        _containerAsset = GetContainerAsset(prefabGuid);

        if (_containerAsset == null) {
            ReferenceStatus = Status.PrefabNotFound;
        } else if (_reference == null) {
            ReferenceStatus = Status.ObjectNotFound;
        } else {
            ReferenceStatus = Status.Resolved;
            RefreshFromReference(_reference);
        }
    }

    UObject GetContainerAsset(string guid)
    {
        if (string.IsNullOrEmpty(guid))
            return null;

        var path = AssetDatabase.GUIDToAssetPath(guid);
        if (string.IsNullOrEmpty(path))
            return null;

        return AssetDatabase.LoadMainAssetAtPath(path);
    }

    void RefreshFromReference(UObject reference)
    {
        if (reference == null || this == null) return;

        name = reference.name;
        ReferenceType = reference.GetType();
    }
}

}

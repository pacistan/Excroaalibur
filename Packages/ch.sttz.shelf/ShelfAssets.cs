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

using UnityEngine;
using UnityEngine.UIElements;

using UObject = UnityEngine.Object;

namespace sttz.TheShelf {

/// <summary>
/// Scriptable object used to set asset references for the shelf
/// through the script's default references.
/// </summary>
public class ShelfAssets : ScriptableObject
{
    // ---------- Shared Instance ----------

    /// <summary>
    /// The shared assets instance.
    /// </summary>
    public static ShelfAssets Shared
    {
        get {
            if (_shared == null) {
                // Try to get default reference from instance,
                // fall back to use empty instance to not repeatedly create one
                var temp = CreateInstance<ShelfAssets>();
                _shared = temp.sharedAssets ?? temp;
            }
            return _shared;
        }
    }
    static ShelfAssets _shared;

    /// <summary>
    /// The main `ShelfAssets` instance that is available through <see cref="Shared"/>.
    /// </summary>
    /// <remarks>
    /// To be able to link the `ShelfAssets` asset in the project to the
    /// static <see cref="Shared"/> instance, we use Unity's default
    /// script references. The instance is set on the `ShelfAsset` script
    /// itself when it is selected in the editor and then a new instance created
    /// once to get this default reference.
    /// It's marked `HideInInspector` so that it only appears in the
    /// default references but not when editing the asset itself.
    /// </remarks>
    [HideInInspector] public ShelfAssets sharedAssets;

    // ---------- Assets ----------

    /// <summary>
    /// The contents of the first user rack shelf after the package is installed.
    /// </summary>
    public UObject[] startShelf;

    [Header("Style Sheets")]

    /// <summary>
    /// The style sheet to use in shelf windows and inspectors.
    /// </summary>
    public StyleSheet styleSheet;

    /// <summary>
    /// Styles for backwards-compatibility for Unity version prior to 2023.2.
    /// </summary>
    public StyleSheet styleSheetBackwards;

    [Header("Icons")]

    /// <summary>
    /// Icon used for rack windows.
    /// </summary>
    public Texture2D rackWindowIcon;
    
    /// <summary>
    /// Icon used for the active rack window.
    /// </summary>
    public Texture2D rackWindowActiveIcon;

    /// <summary>
    /// Icon used for remove buttons.
    /// </summary>
    public UObject removeIcon;

    /// <summary>
    /// Icon used for add buttons.
    /// </summary>
    public UObject addIcon;

    /// <summary>
    /// Icon used for rack settings tab.
    /// </summary>
    public UObject settingsIcon;

    /// <summary>
    /// Icon used for warnings.
    /// </summary>
    public UObject warningIcon;

    /// <summary>
    /// Icon used for dock button.
    /// </summary>
    public UObject dockIcon;

    /// <summary>
    /// Icon used for the active rack.
    /// </summary>
    public Texture2D activeIcon;

    /// <summary>
    /// Icon used for the url object.
    /// </summary>
    public Texture2D urlIcon;

    // ---------- Utilities ----------

    /// <summary>
    /// Convert a supported Unity Object to a <see cref="Background"/> instance.
    /// Returns an empty `Background` if the object is not supported.
    /// </summary>
    /// <remarks>
    /// Supported types are `Texture2D`, `RenderTexture`, `Sprite` and `VectorImage`.
    /// There's `Background.FromObject` but that's unfortunately `internal`.
    /// </remarks>
    public static Background BackgroundFromObject(UObject asset)
    {
        if (asset is Texture2D texture2D)
            return Background.FromTexture2D(texture2D);
        if (asset is RenderTexture renderTexture)
            return Background.FromRenderTexture(renderTexture);
        if (asset is Sprite sprite)
            return Background.FromSprite(sprite);
        if (asset is VectorImage vectorImage)
            return Background.FromVectorImage(vectorImage);

        return default;
    }

    /// <summary>
    /// Apply a supported Unity Object to the given <see cref="Image" instance.
    /// </summary>
    /// <remarks>
    /// Supported types are `Texture2D`, `RenderTexture`, `Sprite` and `VectorImage`.
    /// </remarks>
    /// <returns>If the object was successfully applied, `false` if the type is not supported or is `null`</returns>
    public static bool ApplyObjectToImage(Image image, UObject asset)
    {
        if (asset is Texture2D texture2D) {
            image.image = texture2D;
            return true;
        }
        if (asset is RenderTexture renderTexture) {
            image.image = renderTexture;
            return true;
        }
        if (asset is Sprite sprite) {
            image.sprite = sprite;
            return true;
        }
        if (asset is VectorImage vectorImage) {
            image.vectorImage = vectorImage;
            return true;
        }

        return false;
    }
}

}

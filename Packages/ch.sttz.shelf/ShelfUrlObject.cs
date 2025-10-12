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
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

using UObject = UnityEngine.Object;

namespace sttz.TheShelf {

/// <summary>
/// Asset representing an URL that can be opened in the Unity editor.
/// </summary>
/// <remarks>
/// The URL can be anything that <see cref="Application.OpenURL"/> can open,
/// including web links. The asset can be double-clicked to open the URL.
/// 
/// For web links, the asset can optionally fetch the favicon of the website
/// and apply it as its icon (asset preview). Note that Unity doesn't show the
/// asset preview when the icon is displayed too small.
/// </remarks>
[CreateAssetMenu(menuName = "The Shelf/Url", order = 100), InitializeOnLoad]
public class ShelfUrlObject : ScriptableObject, IShelfItem
{
    // ---------- Properties ----------

    /// <summary>
    /// The URL to open.
    /// </summary>
    [SerializeField] string url;

    // ---------- API ----------

    /// <summary>
    /// Open this object's URL.
    /// </summary>
    public void Open()
    {
        var urlToOpen = url;

        // Allow entering URLs without scheme
        if (!urlToOpen.ToLower().StartsWith("http")) {
            urlToOpen = "https://" + urlToOpen;
        }

        Application.OpenURL(urlToOpen);
    }

    /// <summary>
    /// Create a copy of the scene object reference.
    /// </summary>
    public UObject Clone()
    {
        var clone = CreateInstance<ShelfUrlObject>();
        clone.name = name;
        clone.url = url;
        return clone;
    }

    // ---------- Actions ----------

    static ShelfUrlObject()
    {
        ShelfApi.RegisterContextMenuAction<ShelfUrlObject>("Copy URL", ActionCopyURL);
        ShelfApi.RegisterContextMenuAction<ShelfUrlObject>("Load Favicon", ActionLoadFavicon);
    }

    public static void ActionCopyURL(ShelfUrlObject urlObject)
    {
        EditorGUIUtility.systemCopyBuffer = urlObject.url;
    }

    public static void ActionLoadFavicon(ShelfUrlObject urlObject)
    {
        LoadFaviconUI(urlObject);
    }

    [OnOpenAsset]
    static bool OnOpenAsset(int instanceID, int line)
    {
        var obj = EditorUtility.InstanceIDToObject(instanceID);
        if (obj is not ShelfUrlObject urlObject)
            return false;

        urlObject.Open();
        return true;
    }

    [MenuItem("CONTEXT/ShelfUrlObject/Load Favicon")]
    static void LoadFavicon(MenuCommand cmd)
    {
        LoadFaviconUI((ShelfUrlObject)cmd.context);
    }

    // ---------- Favicon ----------

    /// <summary>
    /// Name of the favicon texture added as a sub-asset.
    /// </summary>
    public const string FaviconName = "Favicon";

    /// <summary>
    /// URL of the favicon fetching service.
    /// "{0}" will be replaced with the domain name.
    /// </summary>
    public static string FaviconServiceUrl = "https://www.google.com/s2/favicons?domain={0}&sz=256";

    /// <summary>
    /// Try to load the favicon and show a dialog on error.
    /// </summary>
    public static async void LoadFaviconUI(ShelfUrlObject urlObject)
    {
        try {
            await LoadAndApplyFavicon(urlObject);
        } catch (Exception e) {
            EditorUtility.DisplayDialog(
                "Load Favicon",
                e.Message,
                "OK"
            );
        }
    }

    /// <summary>
    /// Try to load the favicon for the given http(s) URL.
    /// </summary>
    /// <returns>The favicon texture, throws on error</returns>
    public static async Task<Texture2D> LoadFavicon(string url)
    {
        if (string.IsNullOrEmpty(url)) {
            throw new Exception("URL is empty.");
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) {
            throw new Exception("Failed to parse URL.");
        }

        if (string.IsNullOrEmpty(uri.Host) || !uri.Scheme.ToLower().StartsWith("http")) {
            throw new Exception($"URL must be a http(s) URL.\n(Got {uri.Scheme}://{uri.Host})");
        }

        var escapedDomain = UnityWebRequest.EscapeURL(uri.Host);
        var requestUrl = string.Format(FaviconServiceUrl, escapedDomain);
        var req = UnityWebRequestTexture.GetTexture(requestUrl);

    #if UNITY_2023_2_OR_NEWER
        // Unity 2023.2+ with Awaitable has native support to await AsyncOperation
        await req.SendWebRequest();
    #else
        // On older versions, work around no await support
        req.SendWebRequest();
        while (!req.isDone) await Task.Yield();
    #endif

        if (req.responseCode == 404) {
            throw new Exception($"Could not find a favicon for '{uri.Host}'.");
        }
        if (req.result != UnityWebRequest.Result.Success) {
            throw new Exception($"Failed loading favicon\n{req.error}");
        }

        return DownloadHandlerTexture.GetContent(req);
    }

    /// <summary>
    /// Try to load the favicon for the given url and apply it to the url object.
    /// Throws if an error occurs.
    /// </summary>
    public async static Task LoadAndApplyFavicon(ShelfUrlObject urlObject)
    {
        var path = AssetDatabase.GetAssetPath(urlObject);
        if (string.IsNullOrEmpty(path)) {
            throw new Exception("ShelfUrlObject must be a saved asset");
        }

        var url = urlObject.url;
        if (string.IsNullOrEmpty(url)) {
            throw new Exception("ShelfUrlObject.url is empty.");
        }

        // Allow entering URLs without scheme
        if (!url.ToLower().StartsWith("http")) {
            url = "https://" + url;
        }

        var tex = await LoadFavicon(url);
        tex.name = FaviconName;

        // Clean up previous favicons
        var allAssets = AssetDatabase.LoadAllAssetsAtPath(path);
        foreach (var asset in allAssets) {
            if (asset.name != FaviconName) continue;
            if (asset is not Texture2D) continue;
            AssetDatabase.RemoveObjectFromAsset(asset);
        }

        // Save favicon texture together with url asset
        AssetDatabase.AddObjectToAsset(tex, urlObject);

        // Reimport asset to update asset icon
        AssetDatabase.ImportAsset(path);
    }

    // ---------- IShelfItem ----------

    string IShelfItem.Name => name;

    Texture IShelfItem.Icon => ShelfItemsView.GetIconTexture(this);

    UObject IShelfItem.Reference => this;

    VisualElement IShelfItem.Accessory { get {
        var accessory = new VisualElement();
        accessory.AddToClassList("shelf-wrapped-object");

        var icon = new Image();
        icon.image = ShelfAssets.Shared.urlIcon;
        accessory.Add(icon);

        var label = new Label();
        label.text = url;
        accessory.Add(label);

        return accessory;
    } }

    bool IShelfItem.SaveInRackAsset => false;
}

/// <summary>
/// Custom editor for <see cref="ShelfUrlObject"/>,
/// rendering the favicon as static preview and
/// adding the "Load Favicon" button in the inspector.
/// </summary>
[CustomEditor(typeof(ShelfUrlObject))]
public class ShelfUrlObjectEditor : Editor
{
    public override VisualElement CreateInspectorGUI()
    {
        var container = new VisualElement();

        InspectorElement.FillDefaultInspector(container, serializedObject, this);

        var loadFavicon = new Button() {
            text = "Load Favicon",
        };
        loadFavicon.style.marginTop = 20;
        loadFavicon.clicked += () => {
            if (targets.Length == 1) {
                // Single selection: Show errors as dialog
                ShelfUrlObject.LoadFaviconUI((ShelfUrlObject)target);
            } else {
                // Multi selection: Log errors in console
                foreach (var urlObject in targets.OfType<ShelfUrlObject>()) {
                    _ = ShelfUrlObject.LoadAndApplyFavicon(urlObject);
                }
            }
        };
        container.Add(loadFavicon);

        return container;
    }

    public override Texture2D RenderStaticPreview(string assetPath, UObject[] subAssets, int width, int height)
    {
        foreach (var subAsset in subAssets) {
            if (subAsset is Texture2D favicon && favicon.name == ShelfUrlObject.FaviconName) {
                var tex = new Texture2D(width, height);
                EditorUtility.CopySerialized(favicon, tex);
                return tex;
            }
        }

        return null;
    }
}

}

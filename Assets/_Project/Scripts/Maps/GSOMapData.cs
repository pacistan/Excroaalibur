using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "MapData", menuName = "MapData")]
public class GSOMapData : ScriptableObject
{
#if UNITY_EDITOR
    [SerializeField]
    private UnityEditor.SceneAsset _scene;
#endif
    
    [field: SerializeField, ReadOnly] public string SceneName {get; private set;}
    [field: SerializeField, BoxGroup("Map Data")] public Sprite MapCardSprite {get; private set;}
    [field: SerializeField, BoxGroup("Map Data")] public LocalizedString MapName {get; private set;}
    [field: SerializeField, BoxGroup("Unlock Data")] public int NumberOfWavesToUnlock { get; private set; } = 5;
    [field: SerializeField, BoxGroup("Unlock Data")] public GSOMapData ProgressMapToUnlock { get; private set; }
    [field: SerializeField, BoxGroup("Unlock Data")] public int NumberOfWavesOnThisMap { get; private set; } = 0;
    
    #if UNITY_EDITOR
    private void OnValidate()
    {
        SceneName = _scene ? _scene.name : null;
    }
    #endif
}

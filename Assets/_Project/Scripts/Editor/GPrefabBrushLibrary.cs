using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "PrefabBrushLibrary", menuName = "Painter Tool/Prefab Brush Library")]
public class GPrefabBrushLibrary : SerializedScriptableObject
{
    [System.Serializable]
    public class BrushData
    {
        [FoldoutGroup("$brushName")]
        public string brushName = "New Brush";
        [FoldoutGroup("$brushName")]
        public GameObject prefab;
        [FoldoutGroup("$brushName")]
        public Texture2D icon;
        
        [Header("Placement Settings")]
        [FoldoutGroup("$brushName")]
        public bool randomRotation = false;
        [FormerlySerializedAs("incrementalRotation")]
        [FoldoutGroup("$brushName"), ShowIf("randomRotation")]
        public bool isIncrementalRotation = false;
        [FoldoutGroup("$brushName"),HideIf("randomRotation")]
        public float rotationOffset = 0;
    }
    
    [Header("Brush Collection")]
    [SerializeField]
    public List<BrushData> brushes = new List<BrushData>();
    
    [Header("Default Settings")]
    public float defaultBrushSize = 1f;
}

using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class GBrushData
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

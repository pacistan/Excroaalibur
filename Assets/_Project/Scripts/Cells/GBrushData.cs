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
        
    [FoldoutGroup("$brushName")]
    [BoxGroup("$brushName/Rotation")]
    public bool keepRotation = false;
    
    [FoldoutGroup("$brushName")]
    [BoxGroup("$brushName/Rotation")]
    public bool randomRotation = false;
    
    [FormerlySerializedAs("incrementalRotation")]
    [BoxGroup("$brushName/Rotation"), ShowIf("randomRotation")]
    public bool isIncrementalRotation = false;
    
    [FoldoutGroup("$brushName/Rotation"),HideIf("randomRotation")]
    public float rotationOffset = 0;
}

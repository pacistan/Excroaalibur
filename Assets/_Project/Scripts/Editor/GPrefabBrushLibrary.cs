using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PrefabBrushLibrary", menuName = "Painter Tool/Prefab Brush Library")]
public class GPrefabBrushLibrary : SerializedScriptableObject
{

    
    [Header("Brush Collection")]
    [SerializeField]
    public List<GBrushData> brushes = new List<GBrushData>();
    
    [Header("Default Settings")]
    public float defaultBrushSize = 1f;
}
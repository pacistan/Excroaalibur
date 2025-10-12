using UnityEngine;
using UnityEditor;

/// <summary>
/// Disable Transform Gizmos when Objects Selected has Script <see cref="GNoTransformEdit"/>
/// </summary>
#if UNITY_EDITOR
[InitializeOnLoad]
public static class TransformLockUtility
{
    static TransformLockUtility()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    static void OnSceneGUI(SceneView sceneView)
    {
        foreach (var transform in Selection.transforms)
        {
            if (transform.GetComponent<GNoTransformEdit>() != null)
            {
                Tools.current = Tool.None; // hides move/rotate/scale handles
            }
        }
    }
}
#endif

public class GNoTransformEdit : MonoBehaviour { }
using System;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Makes Readable Only the Transform Component of GameObjects that have the Component <see cref="GNoTransformEdit"/>
/// </summary>
#if UNITY_EDITOR
[CanEditMultipleObjects]
[CustomEditor(typeof(Transform))]
public class GLockableTransformEditor : Editor
{
    private Editor _defaultEditor; // Unity's internal TransformInspector
    private Type _internalInspectorType;

    private void OnEnable()
    {
        _internalInspectorType = Type.GetType("UnityEditor.TransformInspector, UnityEditor");
        if (_internalInspectorType != null)
            _defaultEditor = CreateEditor(targets, _internalInspectorType);
    }

    private void OnDisable()
    {
        if (_defaultEditor != null)
            DestroyImmediate(_defaultEditor);
    }

    public override void OnInspectorGUI()
    {
        Transform t = (Transform)target;
        bool isLocked = t.GetComponent<GNoTransformEdit>() != null;

        if (_defaultEditor == null)
        {
            EditorGUILayout.HelpBox("Internal TransformInspector not found.", MessageType.Warning);
            base.OnInspectorGUI();
            return;
        }

        if (isLocked)
        {
            using (new EditorGUI.DisabledGroupScope(true))
            {
                _defaultEditor.OnInspectorGUI();
            }
            EditorGUILayout.HelpBox("Transform locked by LockTransform component.", MessageType.Info);
        }
        else
        {
            _defaultEditor.OnInspectorGUI();
        }
    }
}
#endif
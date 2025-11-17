using UnityEngine;

[CreateAssetMenu(fileName = "DebugManager", menuName = "DebugManager")]
public class GDebugManager : ScriptableObject
{
    [SerializeField]
    ELogType _currentLogType;

    void OnValidate()
    {
        GDebug.currentLogType = _currentLogType;
    }
}

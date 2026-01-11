using Sirenix.OdinInspector;
using UnityEngine;

public class GSerializedSingleton<T> : SerializedMonoBehaviour where T : SerializedMonoBehaviour
{
    public static T Instance { get; private set; }

    protected virtual void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Multiple instances of " + GetType() + " found!");
            Destroy(this);
        }
        else
        {
            Instance = this as T;
        }
    }
}

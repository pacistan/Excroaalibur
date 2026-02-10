using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;


[Flags]
public enum ELogType
{
    None = 0, 
    Andre_Channel_1 = 2, 
    Andre_Channel_2 = 4, 
    Stan = 8, 
    Other = 16,
    All = 30
}

public static class GDebug 
{
    public static ELogType currentLogType = ELogType.None;
    public static void Log(ELogType logType, string msg, Object context = null)
    {
        if (currentLogType.HasFlag(logType)) return;
        Debug.Log(msg, context);
    }
}


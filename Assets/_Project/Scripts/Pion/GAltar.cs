using System;
using UnityEngine;
using UnityEngine.Serialization;

public class GAltar : GPawn
{
    GAltar()
    {
        hp = -1;
        isPlayer = false;
    }
    
    protected override void Awake()
    {
        base.Awake();
        isPlayer = false;
        hp = -1;
    }
}

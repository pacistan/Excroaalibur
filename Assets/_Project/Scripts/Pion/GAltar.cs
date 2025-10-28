using System;
using UnityEngine;
using UnityEngine.Serialization;

public class GAltar : GPawn
{
    protected override void Awake()
    {
        base.Awake();
        hp = -1;
    }
}

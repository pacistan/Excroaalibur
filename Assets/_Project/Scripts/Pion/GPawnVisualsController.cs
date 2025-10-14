using Sirenix.OdinInspector;
using System;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(GPawn))]
public class GPawnVisualsController : SerializedMonoBehaviour
{
    [SerializeField, FoldoutGroup("Components")]
    GPawn _pawn;
    [SerializeField, FoldoutGroup("Components")]
    TextMeshProUGUI _debugTxt;

    void Start()
    {
        _pawn.OnStunned += OnStunned;
        _pawn.OnUnstunned += OnUnstunned;
    }

    public void OnStunned()
    {
        _debugTxt.text = "STUNNED";
    }

    public void OnUnstunned()
    {
        _debugTxt.text = "";
    }
}

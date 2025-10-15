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
    int _health = 0;
    bool _isStunned = false;
    
    void Start()
    {
        _pawn.OnStunned += OnStunned;
        _pawn.OnUnstunned += OnUnstunned;
        _pawn.OnHealthChanged += HealthChange;
        HealthChange(_pawn.hp);
    }

    public void HealthChange(int health)
    {
        _health = health;
        UpdateText();
    }
    
    public void OnStunned()
    {
        _isStunned = true;
        UpdateText();
    }

    public void OnUnstunned()
    {
        _isStunned = false;
        UpdateText();
    }

    private void UpdateText()
    {
        String text = "";

        if (_isStunned) text += "STUNNED\n";
        text += $"HP: {_health}";
        _debugTxt.text = text;
    }
}

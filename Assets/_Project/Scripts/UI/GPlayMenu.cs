using Sirenix.OdinInspector;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GPlayMenu : MonoBehaviour
{
    public event Action<int> OnUpdateCrownUI;

    [SerializeField]
    TextMeshProUGUI _crownDamageTxt;
    
    [field : SerializeField]
    public Button endTurnButton { get; private set; }

    [SerializeField]
    Button _pauseButton;

    [SerializeField]
    TextMeshProUGUI _waveNumberTxt;

    [SerializeField]
    Image _flameImage;
    
    Material _flameMaterial;
    
    [SerializeField]
    int _maxFlameDamage;
    
    [SerializeField, HideInEditorMode, ReadOnly]
    Cursor _cursor;

    public void SetCrownDamageText(int damage) { 
        _crownDamageTxt.text = $"{damage.ToString()}";
        OnUpdateCrownUI?.Invoke(damage);
        
        if (!_flameMaterial) return;
        _flameMaterial.SetFloat("_Circle_Size", Mathf.Clamp01((float)damage/_maxFlameDamage));
    }

    public void SetWaveNumberText(int waveNumber)
    {
        _waveNumberTxt.text = $"Wave {waveNumber}";
    } 
    
    void Start()
    {
        _pauseButton.onClick.AddListener(()=> GGameManager.Instance.ChangeState(EMacroStates.Pause));

        if (_flameImage)
        {
            _flameMaterial = new(_flameImage.material);
            _flameImage.material = _flameMaterial;
        }
    }
}

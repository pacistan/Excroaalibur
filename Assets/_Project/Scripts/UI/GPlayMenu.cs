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

    [SerializeField, HideInEditorMode, ReadOnly]
    Cursor _cursor;


    public void SetCrownDamageText(int damage) { 
        _crownDamageTxt.text = $"{damage.ToString()}";
        OnUpdateCrownUI?.Invoke(damage);
    }

    public void SetWaveNumberText(int waveNumber) => 
        _waveNumberTxt.text = $"Wave {waveNumber}";
    
    void Start()
    {
        _pauseButton.onClick.AddListener(()=> GGameManager.Instance.ChangeState(EMacroStates.Pause));
    }
}

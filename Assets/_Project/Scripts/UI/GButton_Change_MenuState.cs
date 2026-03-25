using System;
using UnityEngine;
using UnityEngine.UI;

public class GButton_Change_MenuState : MonoBehaviour
{
    [SerializeField]
    bool returnToPrevious;
    
    [SerializeField]
    EMacroStates _targetMenu;
    
    Button _button;

    void Start()
    {
        _button = GetComponent<Button>();
        if (!_button)
        {
            enabled = false;
            return;
        }
        
        _button.onClick.AddListener(onButtonClick);
    }

    private void onButtonClick()
    {
        EMacroStates newState = _targetMenu;
        if (returnToPrevious) newState = GGameManager.Instance.previousState;
        
        GGameManager.Instance.ChangeState(newState);
    }
}

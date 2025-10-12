using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GPlayerController : GController
{
    [ReadOnly]
    public IGplayer SelectedPlayer {get; private set;}

    public event Action<IGplayer> OnSelectedPlayerChanged;
    
    private InputAction _selectInput;

    public void SetSelectedPlayer(IGplayer newSelected)
    {
        SelectedPlayer = newSelected;
    }

    private void Start()
    {
        _selectInput = InputSystem.actions.FindAction("Select");
    }

    private void Update()
    {
        if (_selectInput.IsPressed())
        {
            if (SelectedPlayer == null)
            {
                
            }
            else
            {
                
            }
        }
    }
}
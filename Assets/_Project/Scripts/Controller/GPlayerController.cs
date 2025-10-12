using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GPlayerController : GController
{
    public event Action<IGplayer> SelectedPlayerChanged;
    
    [ReadOnly] IGplayer _selectedPlayer;
    InputAction _selectInput;

    public void SetSelectedPlayer(IGplayer newSelected)
    {
        if (_selectedPlayer == newSelected) return;
        
        _selectedPlayer = newSelected;
        SelectedPlayerChanged?.Invoke(newSelected);
    }

    private void Start()
    {
        _selectInput = InputSystem.actions.FindAction("Select");
    }

    private void Update()
    {
        if (_selectInput.IsPressed())
        {
            if (_selectedPlayer == null)
            {
                //TODO Select player on cell
                GetCellUnderMouse();
            }
            else
            {
                //If clicked on the same entity unselect
                //If action selected, send request
            }
        }
    }

    private GCell GetCellUnderMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            GCell cell = hit.transform.gameObject.GetComponent<GCell>();
            return cell;
        }
        return null;
    }
}
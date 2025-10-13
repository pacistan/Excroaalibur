using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class GIPlayerController : MonoBehaviour/*, GIController*/
{
    public event Action<GPawn> SelectedPlayerChanged;
    public int actionToken = 3;
    
    [ReadOnly] GPawn _selectedPlayer;
    InputAction _selectInput;
    int _remainingActionToken = 0;

    public void SetSelectedPlayer(GPawn newSelected)
    {
        if (_selectedPlayer == newSelected) return;
        
        _selectedPlayer = newSelected;
        SelectedPlayerChanged?.Invoke(newSelected);
    }

    public void SetPlayerTurn(bool isPlayerTurn)
    {
        if (isPlayerTurn)
        {
            _remainingActionToken = actionToken;
        }
        else
        {
            _remainingActionToken = 0;
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
}
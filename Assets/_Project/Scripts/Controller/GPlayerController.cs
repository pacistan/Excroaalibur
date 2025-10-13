using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class GIPlayerController : MonoBehaviour/*, GIController*/
{
    public event Action<GPawn> SelectedPlayerChanged;
    public int actionToken = 3;
    public List<GAction> availableActions = new List<GAction>();
    
    [ReadOnly] GPawn _selectedPlayer;
    InputAction _selectInput;
    int _remainingActionToken = 0;

    public void SetSelectedPlayer(GPawn newSelected)
    {
        if (_selectedPlayer == newSelected) return;
        
        _selectedPlayer = newSelected;
        selectedPlayerChanged?.Invoke(newSelected);
    }

    public void SetPlayerTurn()
    {
        _remainingActionToken = actionToken;
    }

    public void ForceEndTurn()
    {
        _remainingActionToken = 0;
    }
    
    private GCell GetCellUnderMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            GCell cell = hit.transform.gameObject.GetComponentInParent<GCell>();
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
            GCell cell = GetCellUnderMouse();
            if (!cell) return;
            GPion player = cell.GetPion() && cell.GetPion().isPlayer ? cell.GetPion() : null;

            if (player)
            {
                if (!_selectedPlayer || _selectedPlayer != player && player.IsStunned)
                    _selectedPlayer = cell.GetPion();
                else
                    _selectedPlayer = null;
            }
            else
            {
                
            }
        }
    }
}
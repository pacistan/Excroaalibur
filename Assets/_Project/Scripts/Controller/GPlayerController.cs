using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class GPlayerController : GController
{
    public event Action<GPawn> SelectedPlayerChanged;
    public GAction[] availableActions = new GAction[] { };
    public ActionList actionList;

    [ReadOnly] GPawn _selectedPlayer;
    [ReadOnly] GAction _selectedAction;
    InputAction _selectInput;
    
    [SerializeField, ReadOnly]
    int _remainingActionToken = 0;
    
    private GCell _targetCell;
    
    public void SetSelectedPlayer(GPawn newSelected)
    {
        if (_selectedPlayer == newSelected) return;
        
        _selectedPlayer = newSelected;
        SelectedPlayerChanged?.Invoke(newSelected);
    }

    public void SetPlayerTurn()
    {
        _remainingActionToken = actionTokens;
    }

    public void ForceEndTurn()
    {
        _remainingActionToken = 0;
    }

    public void SelectAction(GAction action)
    {
        _selectedAction = action;
    }
    
    public void SelectAction(int id)
    {
        if (availableActions.Length <= id) return;
        SelectAction(availableActions[id]);
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

    private GAction[] GetAvailableActions()
    {
        if (!_selectedPlayer) return new GAction[]{};

        List<GAction> newAvailableActions = new List<GAction>();
        
        foreach (GAction newAction in _selectedPlayer.actions)
        {
            newAvailableActions.Add(newAction);
        }
        return newAvailableActions.ToArray();
    }

    private void Start()
    {
        _selectInput = InputSystem.actions.FindAction("Select");
        if (actionList) actionList.OnActionSelected += SelectAction;
    }

    private void Update()
    {
        if (_selectInput.WasPressedThisFrame())
        {
            _targetCell = GetCellUnderMouse();
            if (!_targetCell) return;
            GPawn player = _targetCell.GetPawn() && _targetCell.GetPawn().isPlayer ? _targetCell.GetPawn() : null;

            if (player)
            {
                if (_selectedPlayer != player && !player.IsStunned)
                {
                    _selectedPlayer = _targetCell.GetPawn();
                    SelectedPlayerChanged?.Invoke(_selectedPlayer);
                }
                else
                {
                    _selectedPlayer = null;
                    SelectedPlayerChanged?.Invoke(_selectedPlayer);
                }

                availableActions = GetAvailableActions();
                if (actionList) actionList.UpdateButtons(availableActions);
            }
            
            if (_selectedPlayer && _selectedAction != null)
            {
                StartAction();
            }
        }
    }
    public override void StartTurn()
    {
        SetPlayerTurn();
    }

    public override void StartAction()
    {
        _selectedAction.targetCell = _targetCell;
        if (_selectedPlayer.RequestAction(_selectedAction))
        {
            _remainingActionToken--;
            if (_remainingActionToken <= 0) 
            {
                GTurnBaseManager.Instance.RequestEndTurn(this);
            }
        }
    }

    public override void OnActionOver()
    {
        throw new NotImplementedException();
    }

    public override void EndTurn()
    {
        throw new NotImplementedException();
    }
}
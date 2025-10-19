using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class GPlayerController : GController
{
    public event Action<GPawn> SelectedPlayerChanged;
    public GAction[] availableActions = new GAction[] { };
    [FormerlySerializedAs("actionList")]
    [SerializeField]
    public GActionList actionList;

    [ReadOnly] GPawn _selectedPlayer;
    [ReadOnly] GAction _selectedAction;
    [ReadOnly] GHexCoordinate[] _validCells => _selectedAction != null ? _selectedAction.validCells : Array.Empty<GHexCoordinate>();
    InputAction _selectInput;
    
    [FormerlySerializedAs("_CellLayerMask")]
    [FormerlySerializedAs("_CelllayerMask")]
    [SerializeField, Tooltip("Layer Mask for the Cell Raycast")]
    private LayerMask _cellLayerMask;
    
    
    private GCell _targetCell;
    
    public void SetSelectedPlayer(GPawn newSelected)
    {
        if (_selectedPlayer == newSelected) return;
        if (newSelected == null) SelectAction(null);
        _selectedPlayer = newSelected;
        SelectedPlayerChanged?.Invoke(newSelected);

        availableActions = GetAvailableActions();

        foreach (var action in availableActions)
            action.GetValidCells();
        
        if (actionList) actionList.UpdateButtons(availableActions);
    }

    public void SelectAction(GAction action)
    {
        if (_selectedAction == action) return;
        ResetHighlight();
        _selectedAction = action;
        if (_selectedAction == null) return;
        _selectedAction.linkedPawn = _selectedPlayer;
        ShowHighlight();
    }
    
    public void SelectGAction(int id)
    {
        if (availableActions.Length <= id) return;
        SelectAction(availableActions[id]);
    }

    private void ShowHighlight()
    {
        foreach (var coordinate in _validCells)
        {
            GCell cell = GGridManager.Instance.GetCell(coordinate);
            cell._cellVisualsController.ChangeCellHighlightColor(Color.blue);
        }
    }

    private void ResetHighlight()
    {
        foreach (var coordinate in _validCells)
        {
            GCell cell = GGridManager.Instance.GetCell(coordinate);
            cell._cellVisualsController.ResetCellHighlightColor();
        }
    }
    
    private GCell GetCellUnderMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity ,_cellLayerMask))
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

    //TODO : Change to Button or other interface
    private void DebugEndTurn()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            StopTurn();
        }
    }
    
    private void Start()
    {
        _selectInput = InputSystem.actions.FindAction("Select");
        if (actionList) actionList.OnActionSelected += SelectGAction;
    }

    private void Update()
    {
        if (GTurnBaseManager.Instance.currentTurnController != this) return;
        DebugEndTurn();
        if (_selectInput.WasPressedThisFrame())
        {
            _targetCell = GetCellUnderMouse();
            if (!_targetCell) return;
            
            if (_selectedPlayer && _selectedAction != null && _selectedAction.IsValidCell(_targetCell._hexCoordinates))
            {
                StartAction();
                return;
            }
            
            GPawn player = _targetCell.ownedPawn && _targetCell.ownedPawn.isPlayer ? _targetCell.ownedPawn : null;
            if (player)
            {
                if (_selectedPlayer != player && !player.IsStunned && player.remainingActionToken > 0)
                {
                    SetSelectedPlayer(_targetCell.ownedPawn);
                }
                else
                {
                    SetSelectedPlayer(null);
                }
            }
        }
    }
    
    public override void StartTurn()
    {
        base.StartTurn();
    }

    public override void StartAction()
    {
        if (!_validCells.Contains(_targetCell._hexCoordinates)) return;
        _selectedAction.targetCell = _targetCell;
        if (_selectedPlayer.RequestAction(_selectedAction))
        {
            _selectedPlayer.remainingActionToken--;
            SetSelectedPlayer(null);
            // 
            /*if (remainingActionToken <= 0) 
            {
                GTurnBaseManager.Instance.RequestEndTurn(this);
            }*/
        }
    }

    public override void OnActionOver()
    {
        throw new NotImplementedException();
    }

    public override void EndTurn()
    {
        base.EndTurn();
    }
}
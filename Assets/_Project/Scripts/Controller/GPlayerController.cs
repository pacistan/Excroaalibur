using FMODUnity;
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
    
    [SerializeField]
    private GPlayerHudManager _playerHudManager;

    [SerializeField, ReadOnly, HideInEditorMode] 
    GPawn _selectedPlayer;
    
    [SerializeField, ReadOnly, HideInEditorMode]
    GAction _selectedAction;
    
    [SerializeField, ReadOnly, HideInEditorMode] 
    GHexCoordinate[] _validCells => _selectedAction != null ? _selectedAction.validCells : Array.Empty<GHexCoordinate>();
    
    [SerializeField, Tooltip("Layer Mask for the Cell Raycast")]
    private LayerMask _cellLayerMask;
    
    InputAction _leftClickInput;
    InputAction _rightClickInput;
    private GCell _targetCell;
    private GCell _hoverCell;
    private int _currentActionIndex;
    
    
    public void SetSelectedPlayer(GPawn newSelected)
    {
        if (_selectedPlayer == newSelected) return;
        if (newSelected == null || !newSelected.isPlayer)
        {
            _selectedPlayer = null;
            SelectAction(null);
            return;
        }
        _selectedPlayer = newSelected;
        
        availableActions = GetAvailableActions();
        foreach (var action in availableActions)
            action.GetValidCells();
        SelectAction(0);

        
        if (_playerHudManager) 
            _playerHudManager.actionList.UpdateButtons(_selectedPlayer);
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
    
    public void SelectAction(int id)
    {
        _currentActionIndex = id;
        if (availableActions.Length <= id) return;
        SelectAction(availableActions[id]);
    }

    private void ShowHighlight()
    {
        foreach (var coordinate in _validCells)
        {
            GCell cell = GGridManager.Instance.GetCell(coordinate);
            cell.cellVisualsController.ChangeCellHighlightColor(Color.blue);
        }
    }

    private void ResetHighlight()
    {
        foreach (var coordinate in _validCells)
        {
            GCell cell = GGridManager.Instance.GetCell(coordinate);
            cell.cellVisualsController.ResetCellHighlightColor();
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

        if (_selectedPlayer.GetCell().data.tileType == ETileType.Hole)
        {
            newAvailableActions.Add(_selectedPlayer.actions[3]);
        }
        else
        {
            newAvailableActions.Add(_selectedPlayer.actions[0]);
            if (_selectedPlayer.equipment && _selectedPlayer.equipment is GCrown)
            {
                newAvailableActions.Add(_selectedPlayer.actions[2]);
            }
            else
            {
                newAvailableActions.Add(_selectedPlayer.actions[1]);
            }
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
        _leftClickInput = InputSystem.actions.FindAction("Select");
        _rightClickInput = InputSystem.actions.FindAction("Switch");
    }

    private void Update()
    {
        if (GTurnBaseManager.Instance.currentTurnController != this) return;
        DebugEndTurn();

        HandlePlayerHover();
        HandlePlayerClick();
       
        
    }

    void HandlePlayerHover()
    {
        GCell newCell = GetCellUnderMouse();
        if (newCell && newCell != _hoverCell)
        {
            if (!_selectedPlayer && newCell.gridObject)
            {
                _playerHudManager.OnGridObjectHovered(newCell.gridObject);
            }
            GPawn cellPawn = newCell.GetGridObject<GPawn>();
            
            if (cellPawn && !cellPawn.hoverSound.IsNull)
            {
                RuntimeManager.PlayOneShotAttached(cellPawn.hoverSound, cellPawn.gameObject);
            }
            else
            {
                RuntimeManager.PlayOneShot("event:/Map/Hover_Empty");
            }
            
            _hoverCell = newCell;
        }
        else if (!newCell)
        {
            _hoverCell = null;
            if (!_selectedPlayer)
            {
                _playerHudManager.OnGridObjectHovered(null);
            }
        }
    }

    void HandlePlayerClick()
    {
        if (_leftClickInput.WasPressedThisFrame())
        {
            if (!_hoverCell) return;
            _targetCell = _hoverCell;
            GPawn cellPawn = _targetCell.GetGridObject<GPawn>();

            
            if (_selectedPlayer && _selectedAction.IsValidCell(_targetCell.hexCoordinates))
            {
                StartAction();
            }
            else
            {
                if (cellPawn)
                {
                    // No Selected player and Clicked on not Player Pawn
                    if (!_selectedPlayer && !cellPawn.isPlayer)
                    {
                        
                    }
                    // No Selected player and Clicked on Player Pawn
                    else if (!_selectedPlayer && cellPawn.isPlayer)
                    {
                        SetSelectedPlayer(cellPawn);
                    }
                    // Already Selected pawn
                    else if (_selectedPlayer == cellPawn)
                    {
                        SetSelectedPlayer(null);
                    }
                    // Other Player Selected
                    else if (_selectedPlayer != cellPawn && _selectedPlayer.isPlayer)
                    {
                        SetSelectedPlayer(cellPawn);
                        _playerHudManager.OnGridObjectHovered(cellPawn);
                    }
                    // Not Player Pawn
                    else if (_selectedPlayer != cellPawn && !_selectedPlayer.isPlayer)
                    {
                        SetSelectedPlayer(null);
                        _playerHudManager.OnGridObjectHovered(cellPawn);
                    }
                }
                // No Pawn on Cell
                else
                {
                    SetSelectedPlayer(null);
                    _playerHudManager.OnGridObjectHovered(null);
                }
            }
            
            if (cellPawn && !cellPawn.SelectSound.IsNull)
            {
                RuntimeManager.PlayOneShotAttached(cellPawn.SelectSound, cellPawn.gameObject);
            }
            else
            {
                RuntimeManager.PlayOneShot("event:/Map/Select_Empty");
            }
        }
        else if (_rightClickInput.WasPressedThisFrame())
        {
            SwitchAction();
            _playerHudManager.actionList.SwitchActionIndex();
        }
    }

    private void SwitchAction()
    {
        _currentActionIndex = (_currentActionIndex + 1) % 2;
        SelectAction(_currentActionIndex);
    }
    
    public override void StartTurn()
    {
        base.StartTurn();
    }

    public override void StartAction()
    {
        if (!_validCells.Contains(_targetCell.hexCoordinates)) return;
        _selectedAction.targetCell = _targetCell;
        if (_selectedPlayer.RequestAction(_selectedAction))
        {
            _selectedPlayer.remainingActionToken--;
            _selectedPlayer.visuals.OnUpdateActionsToken();
            availableActions = GetAvailableActions();
            foreach (var action in availableActions)
                action.GetValidCells();
            SelectAction(0);
            _playerHudManager.UpdateGridObjectHoveredInfo(_selectedPlayer);
            //SetSelectedPlayer(null);
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
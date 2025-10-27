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
    
    List<GCell> previsuCell = new List<GCell>();
    
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
        //_selectedAction.linkedPawn = _selectedPlayer;
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
            cell.visuals.SetHighlightActionType(_selectedAction.GetHighlightActionType());
        }
    }

    private void ResetHighlight()
    {
        foreach (var coordinate in _validCells)
        {
            GCell cell = GGridManager.Instance.GetCell(coordinate);
            cell.visuals.SetHighlightActionType(ETileHighlightActionType.Normal);
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

    private GAction[] GetAvailableActions(GPawn target = null)
    {
        target = target ? target : _selectedPlayer;
        if (!target) return new GAction[]{};

        List<GAction> newAvailableActions = new List<GAction>();

        if (target.isPlayer)
        {
            if (target.GetCell().data.tileType == ETileType.Hole)
            {
                newAvailableActions.Add(target.actions[3]);
            }
            else
            {
                newAvailableActions.Add(target.actions[0]);
                if (target.equipment && target.equipment is GCrown)
                {
                    newAvailableActions.Add(target.actions[2]);
                }
                else
                {
                    newAvailableActions.Add(target.actions[1]);
                }
            }
        }
        else if (target.TryGetComponent<GAIController>(out GAIController controller))
        {
            // TODO : Get Default Action from AI Controller
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
            foreach (GCell cell in previsuCell)
                cell.visuals.isPrevisualized = false;
            
            if (_selectedPlayer && _selectedAction != null && newCell != _hoverCell && _selectedAction.IsValidCell(newCell.hexCoordinates))
            {
                GActionContext context = new GActionContext();
                _selectedAction.targetCell = newCell;
                
                previsuCell = _selectedAction.Previsualisation(context);
                foreach (GCell cell in previsuCell)
                {
                    cell.visuals.isPrevisualized = true;
                }
                
                // TODO : Here recup Stun and Damage Value 
            }
            
            newCell.visuals.isHovered = true;
            if (_hoverCell != null)
            {
                _hoverCell.visuals.isHovered = false;
            }
            
            // Hover New Tile with no Selection
            if (!_selectedPlayer && newCell.gridObject)
            {
                _playerHudManager.OnGridObjectHovered(newCell.gridObject);
            }
            // Hover New Tile with no Selection and No Object
            else if (!_selectedPlayer && !newCell.gridObject)
            {
                _playerHudManager.OnGridObjectHovered(null);
            }
            
            GPawn cellPawn = newCell.GetGridObject<GPawn>();
            
            // Handle Hover Sounds
            if (cellPawn && !cellPawn.hoverSound.IsNull)
            {
                RuntimeManager.PlayOneShotAttached(cellPawn.hoverSound, cellPawn.gameObject);
            }
            else
            {
                RuntimeManager.PlayOneShot("event:/Map/Hover_Empty");
            }
            
            // Handle Action Highlight on Hover
            if (cellPawn && _hoverCell != newCell && !_selectedPlayer)
            {
                var tempAvailableActions = GetAvailableActions(cellPawn);
                if (tempAvailableActions.Length > 0)
                {
                    tempAvailableActions[0].GetValidCells();
                    SelectAction(tempAvailableActions[0]);
                }
                else
                {
                    SelectAction(null);
                }
            }
            else if (!_selectedPlayer && _hoverCell != newCell && _selectedAction != null)
            {
                SelectAction(null);
            }
            
            _hoverCell = newCell;
        }
        else if (!newCell)
        {
            if (_hoverCell != null)
            {
                _hoverCell.visuals.isHovered = false;
            }
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

            if (_targetCell != null)
            {
                _targetCell.visuals.isSelected = false;
                if (_targetCell == _hoverCell)
                {
                    _targetCell = null;
                    SetSelectedPlayer(null);
                    return;
                }
            }

            
            _targetCell = _hoverCell;
            GPawn cellPawn = _targetCell.GetGridObject<GPawn>();

            
            if (_selectedPlayer && _selectedAction.IsValidCell(_targetCell.hexCoordinates) && _selectedPlayer.remainingActionToken > 0)
            {
                ResetHighlight();
                StartAction();
                SetSelectedPlayer(null);
                SelectAction(null);
                _targetCell = null;
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
                        _targetCell.visuals.isSelected = true;
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
                        _targetCell.visuals.isSelected = true;
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
            if (_selectedPlayer == null) return;
            SwitchAction();
            _playerHudManager.actionList.SwitchActionIndex();
        }
    }

    private void SwitchAction()
    {
        _currentActionIndex = (_currentActionIndex + 1) % 2;
        SelectAction(_currentActionIndex);
    }
    
    public override void StartAction()
    {
        if (!_validCells.Contains(_targetCell.hexCoordinates) &&  _selectedPlayer.remainingActionToken <= 0) return;
        _selectedAction.targetCell = _targetCell;
        if (_selectedPlayer.RequestAction(_selectedAction))
        {
            _selectedPlayer.remainingActionToken--;
            _selectedPlayer.visuals.OnUpdateActionsToken();
            _playerHudManager.UpdateGridObjectHoveredInfo(_selectedPlayer);
            SetSelectedPlayer(null);
            SelectAction(null);
        }
    }

    public override void OnActionOver()
    {
        base.OnActionOver();
    }
}
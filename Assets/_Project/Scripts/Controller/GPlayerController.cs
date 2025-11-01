using FMODUnity;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GPlayerController : GController
{
    public event Action<GPawn> SelectedPlayerChanged;
    public GAction[] availableActions = new GAction[] { };

    [SerializeField]
    public bool _endTurnWhenNoActionsLeft;
    
    [SerializeField]
    private float _speedRotateTowardsActionValidsCell = 0.1f;
    public void SetEndTurnWhenNoActionsLeft(bool endTurnWhenNoActionsLeft) => _endTurnWhenNoActionsLeft = endTurnWhenNoActionsLeft;
    
    [SerializeField, ReadOnly, HideInEditorMode] 
    GPawn _selectedPlayer;

    [SerializeField, ReadOnly, HideInEditorMode]
    GAction _selectedAction;
    
    [SerializeField, ReadOnly, HideInEditorMode] 
    GHexCoordinate[] _validCells => _selectedAction != null ? _selectedAction.validCells : Array.Empty<GHexCoordinate>();
    
    [SerializeField, Tooltip("Layer Mask for the Cell Raycast")]
    private LayerMask _cellLayerMask;

    [FormerlySerializedAs("_normalActionCursor")]
    [SerializeField]
    Texture2D _normalCursor;

    [FormerlySerializedAs("_waitActionCursor")]
    [SerializeField]
    Texture2D _waitCursor;

    [SerializeField]
    Vector2 _normalCursorOffset, _waitCursorOffset;
    
    private GTargetHud _targetHud;
    InputAction _leftClickInput;
    InputAction _rightClickInput;
    private GCell _targetCell;
    private GCell _hoverCell;
    private int _currentActionIndex;
    
    List<GCell> previsuCell = new List<GCell>();
    bool _isFirstAction = true;
    
    public void SetSelectedPlayer(GPawn newSelected)
    {
        if (_selectedPlayer == newSelected) return;
        if (newSelected == null || !newSelected.data.isPlayer)
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

        
        if (_targetHud) 
            _targetHud.actionList.UpdateButtons(_selectedPlayer, _isFirstAction);
    }

    public void SelectAction(GAction action)
    {
        if (_selectedAction == action) return;
        ResetHighlight();
        
        if (_selectedAction != null) 
            _selectedAction.OnUnselectedAction();
        
        GPawn targetPawn = _hoverCell ? _hoverCell.GetGridObject<GPawn>() : null;

        _selectedAction = action;
        
        int index = action != null && action.GetType() != typeof(GThrowAction) ? 0 :
            action != null && targetPawn && !targetPawn.data.isPlayer ? 0 : 1;
        Texture2D cursor = action == null ? _normalCursor : action.GetCursorIcon(index);
        Vector2 cursorOffset = action != null && action.centerCursorOffset ? new Vector2(cursor.width / 2f, cursor.height / 2f) : Vector2.zero;

        Cursor.SetCursor(cursor, cursorOffset, CursorMode.Auto);
        
        if (_selectedAction == null) return;
        _selectedAction.OnSelectedAction();
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
            ETileHighlightActionType highlightActionType = _selectedAction.linkedPawn.data.isPlayer
                ? _selectedAction.GetHighlightActionType()
                : ETileHighlightActionType.EnnemyAction;
            cell.visuals.SetHighlightActionType(highlightActionType);
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

        if (target.data.isPlayer)
        {
            if (_isFirstAction)
            {
                newAvailableActions.Add(target.data.actionList[1]);
            }
            else if (target.GetCell().data.tileType == ETileType.Hole)
            {
                newAvailableActions.Add(target.data.actionList[3]);
            }
            else
            {
                newAvailableActions.Add(target.data.actionList[0]);
                if (target.equipment && target.equipment is GCrown)
                {
                    newAvailableActions.Add(target.data.actionList[2]);
                }
                else
                {
                    newAvailableActions.Add(target.data.actionList[1]);
                }
            }
        }
        else if (target.TryGetComponent<GAIController>(out GAIController controller))
        {
            newAvailableActions.Add(target.data.actionList[0]);
            // TODO : Get Default Action from AI Controller
        }
        
        return newAvailableActions.ToArray();
    }

    //TODO : Change to Button or other interface
    private void DebugTools()
    {
        return;
        if (Input.GetKeyDown(KeyCode.L) && !_isFirstAction)
        {
            StopTurn();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
           GGameManager.Instance.ChangeState(EMacroStates.LoadingScreen);
        }
    }
    
    private void Start()
    {
        _targetHud = GHudManager.Instance.TargetHud;
        _leftClickInput = InputSystem.actions.FindAction("Select");
        _rightClickInput = InputSystem.actions.FindAction("Switch");
        GHudManager.Instance.playMenu.endTurnButton.onClick.AddListener(StopTurn);
        GHudManager.Instance.playMenu.endTurnButton.interactable = false;
        GGameManager.Instance.OnChangeMacroStateEvent += OnChangeMacroStateCallback;
        Cursor.SetCursor(_normalCursor, Vector2.zero, CursorMode.Auto);
    }

    private void Update()
    {
        if (GTurnBaseManager.Instance == null || GGameManager.Instance.isGamePaused || !GTurnBaseManager.Instance.enabled) return;
        HandlePlayerHover();
        if (GTurnBaseManager.Instance.currentTurnController != this) return;
        DebugTools();

        HandlePlayerLeftClick();
    }

    void HandlePlayerHover()
    {
        GCell newCell = GetCellUnderMouse();
        if (newCell && newCell != _hoverCell)
        {
            GPawn cellPawn = newCell.GetGridObject<GPawn>();
            
            DisablePrevisualisation();
            
            if (_selectedPlayer && _selectedPlayer.remainingActionToken > 0 && !_selectedPlayer.IsStunned && _selectedAction != null && newCell != _hoverCell && _selectedAction.IsValidCell(newCell.hexCoordinates))
            {
                ActivatePrevisualisation(newCell, cellPawn);
            }
            
            newCell.visuals.isHovered = true;
            if (_hoverCell != null)
            {
                _hoverCell.visuals.isHovered = false;
            }
            
            // Hover New Tile with no Selection
            if (!_selectedPlayer && newCell.gridObject)
            {
                _targetHud.OnGridObjectHovered(newCell.gridObject, _isFirstAction);
            }
            // Hover New Tile with no Selection and No Object
            else if (!_selectedPlayer && !newCell.gridObject)
            {
                _targetHud.OnGridObjectHovered(null, _isFirstAction);
            }
            
            
            // Handle Hover Sounds
            if (cellPawn && !cellPawn.data.hoverSound.IsNull)
            {
                RuntimeManager.PlayOneShotAttached(cellPawn.data.hoverSound, cellPawn.gameObject);
            }
            else
            {
                RuntimeManager.PlayOneShot("event:/Map/Hover_Empty");
            }
            
            // Handle Action Highlight on Hover
            if (cellPawn && _hoverCell != newCell && !_selectedPlayer && !(cellPawn.data.isPlayer && (cellPawn.remainingActionToken == 0 || cellPawn.IsStunned) ))
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
            
            if (_selectedAction != null && _selectedAction.IsValidCell(newCell.hexCoordinates))
            {
                if (_selectedPlayer != null)
                    _selectedPlayer.RotateTowards(newCell.transform.position, _speedRotateTowardsActionValidsCell);
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
                _targetHud.OnGridObjectHovered(null, _isFirstAction);
            }
        }
    }

    void HandlePlayerLeftClick()
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
                DisablePrevisualisation();
                StartAction();
                SetSelectedPlayer(null);
                SelectAction(null);
                _targetCell = null;
            }
            else
            {
                if (cellPawn)
                {
                    bool isSelectable = cellPawn.data.isPlayer && cellPawn.remainingActionToken > 0 && !cellPawn.IsStunned;
                    // No Selected player and Clicked on not Player Pawn
                    if (!_selectedPlayer && !cellPawn.data.isPlayer)
                    {
                        
                    }
                    // No Selected player and Clicked on Player Pawn
                    else if (!_selectedPlayer && cellPawn.data.isPlayer && isSelectable)
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
                    else if (_selectedPlayer != cellPawn && isSelectable)
                    {
                        _targetCell.visuals.isSelected = true;
                        SetSelectedPlayer(cellPawn);
                        _targetHud.OnGridObjectHovered(cellPawn, _isFirstAction);
                    }
                    // Not Player Pawn
                    else if (_selectedPlayer != cellPawn && !isSelectable)
                    {
                        SetSelectedPlayer(null);
                        if (!cellPawn.data.isPlayer)
                        {
                            _targetHud.OnGridObjectHovered(cellPawn, _isFirstAction);
                        }
                    }
                }
                // No Pawn on Cell
                else
                {
                    SetSelectedPlayer(null);
                    SelectAction(null);
                    _targetHud.OnGridObjectHovered(null, _isFirstAction);
                }
            }
            
            if (cellPawn && !cellPawn.data.SelectSound.IsNull)
            {
                RuntimeManager.PlayOneShotAttached(cellPawn.data.SelectSound, cellPawn.gameObject);
            }
            else
            {
                RuntimeManager.PlayOneShot("event:/Map/Select_Empty");
            }
        }
        else if (_rightClickInput.WasPressedThisFrame())
        {
            if (_selectedPlayer == null || availableActions == null || availableActions.Length <= 1) return;
            SwitchAction();
            _targetHud.actionList.SwitchActionIndex();
        }
    }

    private void SwitchAction()
    {
        DisablePrevisualisation();
        _currentActionIndex = (_currentActionIndex + 1) % 2;
        SelectAction(_currentActionIndex);
    }
    
    public override void StartAction()
    {
        if (!_validCells.Contains(_targetCell.hexCoordinates) &&  _selectedPlayer.remainingActionToken <= 0) return;
        if(_selectedPlayer.IsStunned) return;
        _selectedAction.targetCell = _targetCell;
        if (_selectedPlayer.RequestAction(_selectedAction))
        {
            if (_isFirstAction)
            {
                _isFirstAction = false;
                GHudManager.Instance.playMenu.endTurnButton.interactable = true;
            }
            _selectedPlayer.remainingActionToken--;
            _selectedPlayer.visuals.OnUpdateActionsToken();
            _targetHud.UpdateGridObjectHoveredInfo(_selectedPlayer, _isFirstAction);
            SetSelectedPlayer(null);
            SelectAction(null);
            availableActions = null;
        }
    }

    public override void OnActionOver()
    {
        base.OnActionOver();
        if (_endTurnWhenNoActionsLeft)
        {
            bool isTurnOver = true;
            pawns.ForEach(p =>
            {
                if (p.remainingActionToken > 0 && !p.IsStunned) isTurnOver = false;
            });

            if (isTurnOver)
            {
                StopTurn();
            }
        }
    }

    private void ActivatePrevisualisation(GCell hoveredCell, GPawn hoveredPawn)
    {
        GActionContext context = new GActionContext();
        _selectedAction.targetCell = hoveredCell;
                
        int index = _selectedAction.GetType() != typeof(GThrowAction) ? 0 :
            hoveredPawn && !hoveredPawn.data.isPlayer ? 0 : 1;
        //Cursor.SetCursor(_selectedAction.GetCursorIcon(index), Vector2.zero, CursorMode.Auto);
        
        previsuCell = _selectedAction.Previsualisation(context);
                
        foreach (GCell cell in previsuCell)
            cell.visuals.isPrevisualized = true;
        
        context.TryGet(GActionContext.DAMAGE_STRING, out int damage);
        context.TryGet(GActionContext.STUN_STRING, out int stun);
        if (hoveredPawn)
            hoveredPawn.visuals.OnPrevisualisation(damage, stun);
    }

    private void DisablePrevisualisation()
    {
        if (_selectedAction == null || !_selectedAction.targetCell) return;
        GPawn targetPawn = _selectedAction.targetCell.GetGridObject<GPawn>();
        if (_selectedAction != null && targetPawn)
            targetPawn.visuals.OnDisablePrevisualisation();

        if (previsuCell.Count <= 0) return;
        foreach (GCell cell in previsuCell)
            cell.visuals.isPrevisualized = false;
    }

    protected override void StopTurn()
    {
        base.StopTurn();
        GHudManager.Instance.playMenu.endTurnButton.interactable = false;
        Cursor.SetCursor(_waitCursor, Vector2.zero, CursorMode.Auto);
    }

    public override void StartTurn()
    {
        base.StartTurn();
        Cursor.SetCursor(_normalCursor, Vector2.zero, CursorMode.Auto);
        
        if (!_isFirstAction)
        {
            GHudManager.Instance.playMenu.endTurnButton.interactable = true;
        }
    }

    public override void EndTurn()
    {
        base.EndTurn();
        /*foreach (var gHexCoordinate in _validCells)
        {
            GCell cell = GGridManager.Instance.GetCell(gHexCoordinate);
            cell.visuals.isPrevisualized = false;
            cell.visuals.isPrevisualized = false;
            cell.visuals.isSelected = false;
        }*/
    }

    private void OnChangeMacroStateCallback(EMacroStates newState, EMacroStates oldState)
    {
        if (newState == EMacroStates.Play && oldState == EMacroStates.LoadingScreen)
        {
            _isFirstAction = true;
        }
    }

    void Awake() 
    {
        GGameManager.Instance.playerController = this;
    }
}
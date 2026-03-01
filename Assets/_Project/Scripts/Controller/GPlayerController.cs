using FMODUnity;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GPlayerController : GController
{
    public static Action<GPawn> OnPlayerActionOverEvent;
    
    [Tooltip("Event Triggered When Player Try To Play An Action but Cannot du to No Action Token")]
    public UnityEvent<GPawn> onLaunchActionFailed;
    
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

    [SerializeField]
    LineRenderer _prefabPrevisuLineRenderer;

    [SerializeField]
    int _lineRendererNumber;
    
    [SerializeField]
    LayerMask _pawnLayerMask;
    
    private GTargetHud _targetHud;
    InputAction _leftClickInput;
    InputAction _rightClickInput;
    private GCell _targetCell;
    private GCell _hoverCell;
    private int _currentActionIndex;
    LineRenderer[] _lineRenderers;
    
    List<GCell> previsuCell = new List<GCell>();
    public bool isFirstAction = true;
    Camera _camera;

    public void SetSelectedPlayer(GPawn newSelected)
    {
        if (_selectedPlayer == newSelected) return;
        if (newSelected == null /*|| !newSelected.data.isPlayer*/)
        {
            _selectedPlayer?.OnDeactivateOutline?.Invoke();
            _selectedPlayer = null;
            SelectAction(null);
            return;
        }
        
        _selectedPlayer?.OnDeactivateOutline?.Invoke();
        _selectedPlayer = newSelected;
        _selectedPlayer.OnActivateOutline?.Invoke();
        
        availableActions = GetAvailableActions();
        foreach (var action in availableActions)
            action.GetValidCells();
        SelectAction(0);

        
        if (_targetHud) 
            _targetHud.actionList.UpdateButtons(_selectedPlayer, isFirstAction);
    }

    public void SelectAction(GAction action)
    {
        if (_selectedAction == action) return;
        ResetActionZone();
        
        if (_selectedAction != null) 
            _selectedAction.OnUnselectedAction();
        
        GPawn targetPawn = _hoverCell ? _hoverCell.GetGridObject<GPawn>() : null;

        _selectedAction = action;
        
        int index = action != null && action.GetType() != typeof(GThrowAction) ? 0 : action != null && targetPawn && !targetPawn.data.isPlayer ? 0 : 1;
        Texture2D cursor = action == null ? _normalCursor : action.GetCursorIcon(index);
        Vector2 cursorOffset = action != null && action.centerCursorOffset ? new Vector2(cursor.width / 2f, cursor.height / 2f) : Vector2.zero;

        Cursor.SetCursor(cursor, cursorOffset, CursorMode.Auto);
        
        if (_selectedAction == null) return;
        _selectedAction.OnSelectedAction();
        SetActionZone();
    }
    
    public void SelectAction(int id)
    {
        _currentActionIndex = id;
        if (availableActions.Length <= id) return;
        SelectAction(availableActions[id]);
    }

    private void SetActionZone()
    {
        foreach (var coordinate in _validCells)
        {
            GCell cell = GGridManager.Instance.GetCell(coordinate);
            
            EZoneActionType highlightActionType = _selectedAction.linkedPawn.data.isPlayer ? _selectedAction.GetHighlightActionType() : EZoneActionType.EnnemyAction;
            cell.visuals.SetActionZone(highlightActionType);
        }
    }

    private void ResetActionZone()
    {
        foreach (var coordinate in _validCells)
        {
            GCell cell = GGridManager.Instance.GetCell(coordinate);
            cell.visuals.SetActionZone(EZoneActionType.Default);
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
            if (isFirstAction)
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
        if (Application.isEditor)
        {
            if (Input.GetKeyDown(KeyCode.L) && !isFirstAction)
            {
                StopTurn();
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
               GGameManager.Instance.ChangeState(EMacroStates.LoadingScreen);
            }
        }
    }
    
    private void Start()
    {
        _camera = Camera.main;
        _targetHud = GHudManager.Instance.TargetHud;
        _leftClickInput = InputSystem.actions.FindAction("Select");
        _rightClickInput = InputSystem.actions.FindAction("Switch");
        _leftClickInput.Enable();
        _rightClickInput.Enable();
        GHudManager.Instance.playMenu.endTurnButton.onClick.AddListener(StopTurn);
        GHudManager.Instance.playMenu.endTurnButton.interactable = false;
        GGameManager.Instance.OnChangeMacroStateEvent += OnChangeMacroStateCallback;
        _lineRenderers = new  LineRenderer[_lineRendererNumber];
        for (int i = 0; i < _lineRendererNumber; i++)
        {
            _lineRenderers[i] = Instantiate(_prefabPrevisuLineRenderer, transform);
            _lineRenderers[i].enabled = false;
        }
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
        if(!(GGameManager.Instance.currentState == EMacroStates.Play || GGameManager.Instance.currentState == EMacroStates.Upgrade_Select_Character) ) return;
        RaycastHit hit;
        GPawn currentHoveredPawn = null;
        if (Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out hit, _pawnLayerMask))
        {
            currentHoveredPawn  = hit.transform.GetComponentInParent<GPawn>();
        }
        
        GCell newCell = currentHoveredPawn != null ? currentHoveredPawn.GetCell() : GetCellUnderMouse();
        if (newCell && newCell != _hoverCell)
        {
            OnHoverNewCell(newCell);
        }
        else if (!newCell)
        {
            if (_hoverCell != null)
            {
                DisablePrevisualisation();
                
                _hoverCell.visuals.isHovered = false;
                
                GPawn PreviousCellPawn = _hoverCell.GetGridObject<GPawn>();
                if (PreviousCellPawn && PreviousCellPawn != _selectedPlayer)
                    PreviousCellPawn.OnDeactivateOutline?.Invoke();
            }
            _hoverCell = null;
            if (!_selectedPlayer)
            {
                _targetHud.OnGridObjectHovered(null, isFirstAction);
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

            if (GGameManager.Instance.currentState == EMacroStates.Upgrade_Select_Character && cellPawn &&
                cellPawn.data.isPlayer)
            {
                GUpgradeManager.Instance.OnCharacterSelected(cellPawn);
            }
            
            if (_selectedPlayer && _selectedPlayer.data.isPlayer && _selectedAction.IsValidCell(_targetCell.hexCoordinates) && _selectedPlayer.remainingActionToken > 0)
            {
                ResetActionZone();
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
                    bool isSelectable = /*cellPawn.data.isPlayer && cellPawn.remainingActionToken > 0 && cellPawn.stunTurns == 0*/ true;
                    // No Selected player and Clicked on not Player Pawn
                    if (!_selectedPlayer && !cellPawn.data.isPlayer)
                    {
                        SetSelectedPlayer(cellPawn);
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
                        _targetHud.OnGridObjectHovered(cellPawn, isFirstAction);
                    }
                    // Not Player Pawn
                    else if (_selectedPlayer != cellPawn && !isSelectable)
                    {
                        SetSelectedPlayer(null);
                        if (!cellPawn.data.isPlayer)
                        {
                            _targetHud.OnGridObjectHovered(cellPawn, isFirstAction);
                        }
                    }
                }
                // No Pawn on Cell
                else
                {
                    SetSelectedPlayer(null);
                    SelectAction(null);
                    _targetHud.OnGridObjectHovered(null, isFirstAction);
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
        if (!_validCells.Contains(_targetCell.hexCoordinates)) return;

        // Merge with Stun Check in Condition if GD want to Trigger it ! 
        if (_selectedPlayer.remainingActionToken <= 0) 
        {
            onLaunchActionFailed?.Invoke(_selectedPlayer);
            return;
        }
        
        if(_selectedPlayer.stunTurns > 0) return;
        _selectedAction.targetCell = _targetCell;
        if (_selectedPlayer.RequestAction(_selectedAction))
        {
            base.StartAction();
            currentPawn = _selectedPlayer;
            
            if (isFirstAction)
            {
                isFirstAction = false;
                GHudManager.Instance.playMenu.endTurnButton.interactable = true;
            }
            
            PlayActionVFX(_selectedAction);
            _selectedPlayer.remainingActionToken--;
            _selectedPlayer.visuals.OnUpdateActionsToken();
            _targetHud.UpdateGridObjectHoveredInfo(_selectedPlayer, isFirstAction);
            SetSelectedPlayer(null);
            SelectAction(null);
            availableActions = null;
        }
    }

    // here Because Just Need to be play by the Player !
    // TODO : Add the VFx to the pool System ! 
    public void PlayActionVFX(GAction action)
    {
        if (action.ValidateTargetCellVFXPrefab != null  && action.targetCell != null)
        {
            GVfxPlayer _particleSystem = Instantiate(action.ValidateTargetCellVFXPrefab);
            _particleSystem.transform.position = action.targetCell.transform.position;
            _particleSystem.Play();
        }
    }

    void OnHoverNewCell(GCell newCell)
    {
        GPawn cellPawn = newCell.GetGridObject<GPawn>();
            
        DisablePrevisualisation();
        
        if (_selectedPlayer && _selectedPlayer.data.isPlayer && _selectedPlayer.remainingActionToken > 0 && _selectedPlayer.stunTurns == 0 && _selectedAction != null && newCell != _hoverCell && _selectedAction.IsValidCell(newCell.hexCoordinates))
        {
            ActivatePrevisualisation(newCell, cellPawn);
        }
        
        newCell.visuals.isHovered = true;
        if (_hoverCell != null)
        {
            _hoverCell.visuals.isHovered = false;
            
            GPawn previousCellPawn = _hoverCell.GetGridObject<GPawn>();
            if(previousCellPawn && previousCellPawn != _selectedPlayer)
                previousCellPawn.OnDeactivateOutline?.Invoke();
        }
        
        // Hover New Tile with no Selection
        if (!_selectedPlayer && newCell.gridObject)
        {
            _targetHud.OnGridObjectHovered(newCell.gridObject, isFirstAction);
        }
        
        // Hover New Tile with no Selection and No Object
        else if (!_selectedPlayer && !newCell.gridObject)
        {
            _targetHud.OnGridObjectHovered(null, isFirstAction);
        }

        // Handle Hover New Pawn
        if (cellPawn)
        {
            if (!cellPawn.data.hoverSound.IsNull)
            {
                RuntimeManager.PlayOneShotAttached(cellPawn.data.hoverSound, cellPawn.gameObject);
            }
            
            if(cellPawn != _selectedPlayer) // Trigger Only if hovered pawn is not the selected one (because selected pawn already trigger hover event on selection)
                cellPawn.OnActivateOutline?.Invoke();
        } 
        else 
        {
            RuntimeManager.PlayOneShot("event:/Map/Hover_Empty");
        }
        
        // Handle Action Highlight on Hover
        if (cellPawn && _hoverCell != newCell && !_selectedPlayer && !(cellPawn.data.isPlayer && (cellPawn.remainingActionToken == 0 || cellPawn.stunTurns > 0) ))
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
    
    public override void OnActionOver()
    {
        base.OnActionOver();
        if (_endTurnWhenNoActionsLeft)
        {
            OnPlayerActionOverEvent?.Invoke(currentPawn);
            bool isTurnOver = true;
            pawns.ForEach(p =>
            {
                if (p.remainingActionToken > 0 && p.stunTurns == 0) isTurnOver = false;
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
        
        previsuCell = _selectedAction.Previsualisation(context);

        if (context.TryGet(GActionContext.PREVISU_CURVE_POS_STRING, out List<Vector3[]> curves))
        {
            if(curves.Count > _lineRenderers.Length)
                Debug.LogWarning($"Not Enough line renderers to see all previsualisations {curves.Count} > {_lineRenderers.Length}");
            int maxCurves = Mathf.Min(curves.Count, _lineRenderers.Length);
            for (int i = 0; i < maxCurves; i++)
            {
                _lineRenderers[i].enabled = true;
                _lineRenderers[i].positionCount = curves[i].Length;
                _lineRenderers[i].SetPositions(curves[i]);
            }
        }

        if (previsuCell.Count > 0)
        {
            foreach (GCell cell in previsuCell)
                cell.visuals.StartVisualisation();
        }
        
        Material lineMaterial = context.Get<Material>(GActionContext.PREVISU_CURVE_MATERIAL_STRING);
        if (!context.TryGet(GActionContext.PREVISU_CURVE_WIDTH_STRING, out float lineWidth))
        {
            lineWidth = 1;
        }
        AnimationCurve curve = AnimationCurve.Constant(0, 1, lineWidth);
        _lineRenderers.ForEach(l =>
        {
            l.widthCurve = curve;
            l.material = lineMaterial;
        });
        
        context.TryGet(GActionContext.DAMAGE_STRING, out int damage);
        context.TryGet(GActionContext.STUN_STRING, out int stunTurns);
        context.TryGet(GActionContext.ACTION_GAIN_STRING, out int actionGain);
        if (hoveredPawn)
        {
            // TODO : Not intuitive logic that action gain is defaulted to 1 when there is no action point number change
            hoveredPawn.visuals.OnPrevisualisation(damage, stunTurns, 1);
        }
        _selectedPlayer.visuals.OnPrevisualisation(0, 0, actionGain);
    }

    private void DisablePrevisualisation()
    {
        if (_selectedPlayer == null) return;
        if (!(_selectedAction == null || !_selectedAction.targetCell))
        {
            GPawn targetPawn = _selectedAction.targetCell.GetGridObject<GPawn>();
            if (targetPawn)
            {
                targetPawn.visuals.OnDisablePrevisualisation();
            }
        }
        _selectedPlayer.visuals.OnDisablePrevisualisation();

        if (previsuCell == null || previsuCell.Count <= 0) return;
        foreach (GCell cell in previsuCell)
            cell.visuals.StopVisualisation();
        _lineRenderers.ForEach(l => l.enabled = false);
           
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
        
        if (!isFirstAction)
        {
            GHudManager.Instance.playMenu.endTurnButton.interactable = true;
        }
    }

    public override void EndTurn()
    {
        base.EndTurn();
    }

    private void OnChangeMacroStateCallback(EMacroStates newState, EMacroStates oldState)
    {
        if (newState == EMacroStates.Play && oldState == EMacroStates.LoadingScreen && GGameManager.Instance.isLoadingNewSave)
        {
            isFirstAction = true;
        }

        if(newState == EMacroStates.Play)
        {
            // TODO : Replace condition
            bool isPlayerTurn = GHudManager.Instance.playMenu.endTurnButton.interactable;
            Cursor.SetCursor(isPlayerTurn ? _normalCursor : _waitCursor, Vector2.zero, CursorMode.Auto);
        }
    }

    void Awake() 
    {
        GGameManager.Instance.playerController = this;
    }
}
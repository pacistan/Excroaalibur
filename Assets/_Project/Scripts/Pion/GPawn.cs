using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;


public class GPawn : GGridObject
{
    public event Action<GEquipment> OnEquip;
    public event Action<GEquipment> OnUnequip;

    public event Action OnKill;

    public event Action OnStunned;
    public event Action OnUnstunned;
    public event Action<int> OnHealthChanged;
    
    
    [SerializeField]
    public EEquipmentType _equipmentType = EEquipmentType.None;
    
    [SerializeField]
    public bool isPlayer;
    [SerializeReference, ShowIf("isPlayer")]
    public List<GAction> actions = new List<GAction>();
    [SerializeField, ReadOnly, FoldoutGroup("Components")]
    public GEquipment equipment;
    
    [FormerlySerializedAs("_stunTurn")]
    [SerializeField, ReadOnly, HideInEditorMode] 
    int stunTurn = 0;
    public bool IsStunned => stunTurn > 0;

    [field: SerializeField]
    public int hp { get; protected set; } = 3;
    
    [FormerlySerializedAs("_commonData")]
    [SerializeField, FoldoutGroup("Components") ]
    private GCommonInstantiationData _instantiationData;

    [SerializeField, FoldoutGroup("Components")]
    Transform _equipmentParentTr;

    [SerializeField, HideInInspector]
    private EEquipmentType _previousEquipmentType;

    public void Stun(int stunTurnNumber)
    {
        if (stunTurn == 0)
        {
            OnStunned?.Invoke();
        }
        stunTurn += stunTurnNumber;
    }
    
    void Start()
    {
        if (!isPlayer) return;
        GPlayerController controller = FindFirstObjectByType<GPlayerController>();
        if (controller)
            controller.RegisterPawn(this);
        foreach (var action in actions)
            action.linkedPawn = this;
    }
    
    
    public void Posess(GEquipment equipment)
    {
        this.equipment = equipment;
        OnEquip?.Invoke(equipment);
        equipment.SetOwner(this);
        this.equipment.transform.parent = _equipmentParentTr;
        this.equipment.transform.localPosition = Vector3.zero; 
    }

    public void Release()
    {
        if (equipment == null) return;
        equipment.OnReleased();
        OnUnequip?.Invoke(equipment);
        equipment = null;
    }
    
    public override void SetCell(GHexCoordinate newCoordinate)
    {
        base.SetCell(GGridManager.Instance.GetCell(newCoordinate));
    }
    
    public override void SetCell(GCell newCell)
    {
        base.SetCell(newCell);
        currentCell.SetPawn(this);
    }
    
    public bool RequestAction(GAction action)
    {
        if (action == null) return false;
        action.linkedPawn = this;
        print("Request " + action.ToString());
        if (!GTurnBaseManager.Instance.TryPlayAction(action, false))
        {
            print("Action Failed");
            return false;
        }
        
        return true;
    }

    public void TakeDamage(int damage = 1)
    {
        if (hp <= 0) return;
        hp--;
        OnHealthChanged?.Invoke(hp);
        if (hp < 0)
        {
            Kill();
        }
    }

    public void OnStartAction()
    {
        
    }
    
    public void OnStartTurn()
    {
        if (stunTurn > 0)
            stunTurn--;
        if (stunTurn == 0)
        {
            OnUnstunned?.Invoke();
        }
    }

    public void OnEndTurn()
    {
        
    }

    public void Kill()
    {
        gameObject.SetActive(false);
        OnKill?.Invoke();
        Destroy(gameObject);
    }
    
    
#if UNITY_EDITOR
    void OnValidate()
    {
        if (!Application.isPlaying)
        {
            // Equipment Type
            if(_previousEquipmentType != _equipmentType)
            {
                if (equipment)
                {
                    DestroyImmediate(equipment.gameObject);
                }
                GEquipment equipmentPrefab = _instantiationData.equipmentTypeData[_equipmentType];
                if (equipmentPrefab)
                {
                    equipment = PrefabUtility.InstantiatePrefab(equipmentPrefab) as GEquipment;
                    equipment.transform.parent = _equipmentParentTr;
                    equipment.transform.localPosition = Vector3.zero;
                    equipment.SetOwner(this);
                    EditorUtility.SetDirty(equipment);
                }
                _previousEquipmentType = _equipmentType;
            }
            EditorUtility.SetDirty(this);
        }
    }
#endif
    
}
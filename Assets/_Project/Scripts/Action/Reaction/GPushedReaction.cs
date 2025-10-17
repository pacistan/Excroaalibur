using DG.Tweening;
using System.Runtime.CompilerServices;
using UnityEngine;

public class GPushedReaction : GReaction
{
    [SerializeField]
    int _distance = 2;
    
    [SerializeField]
    int _damage = 2;
    [SerializeField]
    int _stun = 2;
    
    EHexDirection _direction;

    bool _inflictDamage;
    
    public override void PreProcess(GActionContext context = null)
    {
        base.PreProcess();
        
        if (context != null && context.Has("direction"))
            _direction = context.Get<EHexDirection>("direction");
        else if (instigatorCell && instigatorCell._hexCoordinates.InStraightLine(instigatorCell._hexCoordinates))
            _direction = instigatorCell._hexCoordinates.GetLineDirection(linkedPawn.coordinate);
        else
            return;
        
        if (context != null && context.Has("distance"))
            _distance = context.Get<int>("distance");
        if (context != null && context.Has("damage"))
            _damage = context.Get<int>("damage");
        if (context != null && context.Has("stun"))
            _stun = context.Get<int>("stun");
        
        
        
        if (instigatorCell != linkedPawn.currentCell && linkedPawn && linkedPawn.equipment && !(linkedPawn is GAltar))
        {
            GEquipment equipment = linkedPawn.equipment;
            linkedPawn.Release();
            instigatorPawn.Possess(equipment);
        }
        
        if (linkedPawn is GAltar && linkedPawn.equipment && linkedPawn.equipment is GCrown)
        {
            GEquipment equipment = linkedPawn.equipment;
            linkedPawn.Release(); 
            instigatorPawn.Possess(equipment);
        }
        
        GCell cell = linkedPawn.currentCell;
        for (int i = 0; i < _distance; i++)
        {
            GCell neighbor = cell.GetNeighbor(_direction);
            
            if (!neighbor || neighbor.GetTileType == ETileType.Wall || neighbor.GetPawn())
            {
                _inflictDamage = true;
                break;
            }
            
            cell = neighbor;
            linkedPawn.SetCell(cell);
            if (!linkedPawn.equipment && cell._equipment && cell._equipment is GCrown)
            {
                linkedPawn.Possess(cell._equipment);
            }
            if (neighbor.GetTileType == ETileType.Hole)
                break;
        }
        
        targetCell = cell;
        linkedPawn.SetCell(targetCell);
        
        if (_inflictDamage)
        {
            linkedPawn.TakeDamage(_damage);
            linkedPawn.Stun(_stun);
        }
    }

    public override void Start_Action()
    {
        base.Start_Action();
        linkedPawn.transform.DOMove(targetCell.transform.position, 0.5f).SetEase(Ease.OutCirc).onComplete = End_Action;
    }

    public override void Update_Action(float delta)
    {
        base.Update_Action(delta);
    }

    public override void End_Action()
    {
        base.End_Action();
        linkedPawn.transform.position = targetCell.transform.position;
    }
}
public class GPlaceOnAltarAction : GAction
{
    GCrown _crown;
    GAltar _altar;
    
    public override void PreProcess()
    {
        _crown = null;
        _altar = null;
        if (linkedPawn.GetEquipment() && linkedPawn.GetEquipment() is GCrown)
        {
            _crown = (GCrown)linkedPawn.GetEquipment();
        }

        if (targetCell._ownedPawn && targetCell._ownedPawn is GAltar)
        {
            _altar = (GAltar)targetCell._ownedPawn;
        }
        
    }

    public override void Start_Action()
    {
        base.Start_Action();
        if (_crown && _altar)
        {
            _crown.ForceRelease();
            _altar.Posess(_crown);
        }
        End_Action();
    }

    public override void End_Action()
    {
        base.End_Action();
    }

}

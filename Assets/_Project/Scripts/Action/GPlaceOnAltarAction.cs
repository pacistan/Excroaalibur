public class GPlaceOnAltarAction : GAction
{
    GCrown _crown;
    GAltar _altar;
    
    public override void PreProcess(GActionContext context = null)
    {
        _crown = null;
        _altar = null;
        if (linkedPawn.equipment && linkedPawn.equipment is GCrown)
        {
            _crown = (GCrown)linkedPawn.equipment;
        }
        
        if (targetCell.GetGridObject<GPawn>() && targetCell.GetGridObject<GPawn>() is GAltar)
        {
            _altar = (GAltar)targetCell.GetGridObject<GPawn>();
        }
        
    }

    public override void Start_Action()
    {
        base.Start_Action();
        if (_crown && _altar)
        {
            _crown.owner.ReleaseEquipement(false, true);
            _altar.GiveEquipement(_crown, true, true);
        }
        End_Action();
    }

    public override void End_Action()
    {
        base.End_Action();
    }

}

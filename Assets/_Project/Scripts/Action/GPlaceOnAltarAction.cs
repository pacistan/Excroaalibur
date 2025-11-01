using FMODUnity;
using System.Collections.Generic;

public class GPlaceOnAltarAction : GAction
{
    GCrown _crown;
    GAltar _altar;

    public override ETileHighlightActionType GetHighlightActionType() => ETileHighlightActionType.Move;
    
    public override List<GCell> Previsualisation(in GActionContext previsuContext)
    {
        return new List<GCell>() { targetCell }; 
    }
    
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
            RuntimeManager.PlayOneShot("event:/Map/Restore_Sword", _altar.transform.position);
        }
        End_Action();
    }

    public override void End_Action()
    {
        base.End_Action();
        if (GGameManager.Instance.isLoadingTutorial)
        {
            GGameManager.Instance.ChangeState(EMacroStates.LoadingScreen);
            GGameManager.Instance.LoadScene(true);            
        }
        else
        {
            GGameManager.Instance.ChangeState(EMacroStates.End);
        }
    }

}

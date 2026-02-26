using UnityEngine;

[CreateAssetMenu(menuName = "AI Behavior/Brawler", fileName = "Brawler Behaviour")]
public class GBrawlerBehavior : GSentryBehavior
{
    public override GAction GetAction()
    {
        if (_isTurnOver) return null;
        
        GPawn linkedPawn = _controller.currentPawn;
        GCell pawnCell = linkedPawn.GetCell();
        bool hasCrown = linkedPawn.equipment && linkedPawn.equipment is GCrown;
        GGridManager.Instance.GenerateStepMap(pawnCell);
        
        if (hasCrown)
        {
            GAltar altar = GGridObjectRegistry.GetClosestObjectOfType<GAltar>(pawnCell, out int altarDistance);
            if (altarDistance == 1)
            {
                _isTurnOver = true;
                return CreatePlaceOnAltarAction(altar.GetCell());
            }   
            else if (altarDistance > 0 && !_hasMoved)
            {
                _hasMoved = true;
                return  CreateMoveAction(altar.GetCell());
            }
        }
        else
        {
            GCrown crown = GGridObjectRegistry.GetClosestObjectOfTypeWithPredicate<GCrown>(
                pawnCell, out int crownDistance, 
                crown=> !(crown.owner is GAltar));
            
            if (crownDistance == 1 && crown.owner)
            {
                _isTurnOver = true;
                return CreatePushAction(crown.GetCell());
            }  
            
            GPawn player = GetPotentialTargetPlayer(out int playerDistance);
            if (playerDistance == 1)
            {
                _isTurnOver = true;
                return CreatePushAction(player.GetCell());
            }
            else if (crownDistance > 0 && !_hasMoved)
            {
                _hasMoved = true;
                return CreateMoveAction(crown.GetCell());
            }    
        }
        return null;
    }

}

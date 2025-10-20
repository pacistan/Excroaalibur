using UnityEngine;

[CreateAssetMenu(menuName = "AI Behavior/Brawler", fileName = "Brawler Behaviour")]
public class GBrawlerBehavior : GSentryBehavior
{
    public override GAction GetAction()
    {
        if (_isTurnOver) return null;
        
        GPawn linkedPawn = _controller.pawn;
        GCell pawnCell = linkedPawn.currentCell;
        bool hasCrown = linkedPawn.equipment && linkedPawn.equipment is GCrown;
        GGridManager.Instance.GenerateStepMap(pawnCell);
        
        if (hasCrown)
        {
            GAltar altar = GGridObjectRegistry.GetClosestObjectOfType<GAltar>(pawnCell, out int altarDistance);
            if (altarDistance == 1)
            {
                _isTurnOver = true;
                return CreatePlaceOnAltarAction(altar.currentCell);
            }   
            else if (altarDistance > 0 && !_hasMoved)
            {
                _hasMoved = true;
                return  CreateMoveAction(altar.currentCell);
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
                return CreatePushAction(player.currentCell);
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

using UnityEngine;

[CreateAssetMenu(menuName = "AI Behavior/Brawler", fileName = "Brawler Behaviour")]
public class GBrawlerBehavior : GSentryBehavior
{
    public override GAction GetAction()
    {
        if (_isTurnOver)
            return null;
        bool hasCrown = _controller.pawn.equipment && _controller.pawn.equipment is GCrown;
        GGridManager.Instance.GenerateStepMap(_controller.pawn.currentCell);
        if (hasCrown)
        {
            GAltar altar = GetPotentialTargetAltar(out int altarDistance);
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
            GCrown crown = GetPotentialTargetCrown(out int crownDistance);
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

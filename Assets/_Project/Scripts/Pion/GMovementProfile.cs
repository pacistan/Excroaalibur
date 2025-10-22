
public enum GMovementProfileId : byte { NoMovement, Crown, Sentry }

public interface IGMovementProfile
{
    GMovementProfileId Id { get; }
    
    /** Can the pawn move through this cell when pathfinding? */
    bool CanTraverse(GCell cell);

    /** Can the pawn end a step/turn on this cell? */
    bool CanStop(GCell cell);
}

public sealed class NoMovementProfile : IGMovementProfile
{
    public GMovementProfileId Id => GMovementProfileId.NoMovement;

    public bool CanTraverse(GCell c) => false;
    public bool CanStop(GCell c) => false;
}

public sealed class CrownMovementProfile : IGMovementProfile
{
    public GMovementProfileId Id => GMovementProfileId.Crown;

    public bool CanTraverse(GCell c)
    {
        bool CellType = c&& (c.GetTileType == ETileType.Normal || c.GetTileType == ETileType.Hole);
        bool CellObject = c && c.GetGridObject<GPawn>() != null;
        return CellType && CellObject;
    }

    public bool CanStop(GCell c)
    {
        bool CellType = c && c.GetTileType == ETileType.Normal;
        bool CellObject = c;
        return CellType && CellObject;
    }
}


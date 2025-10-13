public enum EHexDirection {
    NE, E, SE, SW, W, NW
}

public static class HexDirectionExtensions {
    public static EHexDirection Opposite (this EHexDirection direction) {
        return (int)direction < 3 ? (direction + 3) : (direction - 3);
    }

    public static EHexDirection ToRight(this EHexDirection direction)
    {
        return (EHexDirection)(((int)direction + 1) % 6);
    }

    public static EHexDirection ToLeft(this EHexDirection direction)
    {
        return (EHexDirection)(((int)direction - 1) % 6);
    }

}
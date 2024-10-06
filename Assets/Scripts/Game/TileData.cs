
using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class TileData
{
    public TileIdentity Identity;
    public TileModifier Modifier;

    public int Charges;

    public static TileData GetRandom()
    {
        return new()
        {
            Identity = RandomIdentity(),
            Modifier = RandomModifier(),
            Charges = Mathf.FloorToInt(Random.value * 7) + 1
        };
    }

    public static TileIdentity RandomIdentity(bool nonEmpty = true)
    {
        TileIdentity identity = (TileIdentity)Enum.GetValues(typeof(TileIdentity)).GetValue(Mathf.FloorToInt(Random.value * Enum.GetValues(typeof(TileIdentity)).Length));
        while (identity is TileIdentity.Empty && nonEmpty)
        {
            identity = (TileIdentity)Enum.GetValues(typeof(TileIdentity)).GetValue(Mathf.FloorToInt(Random.value * Enum.GetValues(typeof(TileIdentity)).Length));
        }

        return identity;
    }

    public static TileModifier RandomModifier() => 
        (TileModifier)Enum.GetValues(typeof(TileModifier)).GetValue(Mathf.FloorToInt(Random.value * Enum.GetValues(typeof(TileModifier)).Length));
    
    
    public virtual bool ShouldGroup(TileData other) => other.Identity == Identity;
}

public enum TileIdentity
{
    Empty,
    
    Blue,
    Green,
    Yellow,
    Red,
    Pink
}

public enum TileModifier
{
    None,
    
    Row,
    Col,
    Radius,
    SqrRadius,
    Clump,
    LRDiagonal,  // bottom left to top right
    RLDiagonal,  // bottom right to top left
    CompleteDiagonal,
    
    Shield
}
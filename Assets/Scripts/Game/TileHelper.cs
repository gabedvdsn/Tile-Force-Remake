using System;
using AYellowpaper.SerializedCollections;
using UnityEngine;

public class TileHelper : MonoBehaviour
{
    public static TileHelper Instance;
    
    public SerializedDictionary<TileIdentity, Color> IdentityColors;
    public SerializedDictionary<TileModifier, Sprite> ModifierSprites;
    
    public Color GetIdentityColor(TileIdentity identity) => IdentityColors[identity];
    public Sprite GetModifierSprite(TileModifier modifier) => ModifierSprites[modifier];

    private void Awake()
    {
        Instance = this;
        
        DontDestroyOnLoad(gameObject);
    }
    
    
}

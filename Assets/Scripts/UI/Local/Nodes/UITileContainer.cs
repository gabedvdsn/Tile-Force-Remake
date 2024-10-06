using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UITileContainer : UINode, IPointerDownHandler
{
    [Space]
    
    public Image Icon;
    public Image Modifier;
    public Image Cover;
    public Image HighlightBorder;
    public TMP_Text ChargesText;
    public TMP_Text ImpactText;

    [Space] 
    
    public float BorderHighlightScale = 1.2f;
    public Color BorderColor = Color.black;
    public Color BorderHighlightColor = Color.white;
    public Color BorderSelectColor = Color.cyan;

    [Space] 
    
    public Color CoverExcludeColor;
    
    public TileData Tile;
    [HideInInspector] public int X;
    [HideInInspector] public int Y;
    
    private IHandlesTileContainers Handler;
    private bool isSelected;
    
    [HideInInspector] public bool InContext;

    protected override void Awake()
    {
        base.Awake();

        Handler = GetComponentInParent<IHandlesTileContainers>();

        Modifier.raycastTarget = false;
        HighlightBorder.raycastTarget = false;
        ChargesText.raycastTarget = false;
        ImpactText.raycastTarget = false;
    }
    
    public void SetFields(int x, int y, TileData data)
    {
        if (data is null) return;
        
        Tile = data;
        X = x;
        Y = y;

        Icon.color = TileHelper.Instance.GetIdentityColor(Tile.Identity);
        if (Tile.Modifier != TileModifier.None) Modifier.sprite = TileHelper.Instance.GetModifierSprite(Tile.Modifier);
        else
        {
            Modifier.sprite = default;
            Modifier.color = Color.clear;
        }

        HighlightBorder.color = BorderColor;
        CheckChargesText();

        isSelected = false;
    }

    private void CheckChargesText()
    {
        if (Tile.Charges > 1) ChargesText.text = Tile.Charges.ToString();
        else ChargesText.text = string.Empty;
    }

    public void PreviewImpact(int amount)
    {
        ImpactText.text = amount.ToString();
    }
    
    public void Impact(int amount)
    {
        Tile.Charges -= amount;

        CheckChargesText();
        
        if (Tile.Charges <= 0)
        {
            Tile.Identity = TileIdentity.Empty;
            Tile.Modifier = TileModifier.None;
            
            SetFields(X, Y, Tile);
        }
    }
    
    public void Highlight()
    {
        HighlightBorder.color = BorderHighlightColor;
        HighlightBorder.transform.localScale = Vector2.one * BorderHighlightScale;
    }

    public void DeHighlight()
    {
        isSelected = false;
        HighlightBorder.color = BorderColor;
        HighlightBorder.transform.localScale = Vector3.one;
        ImpactText.text = string.Empty;
    }

    public void Uncover()
    {
        Cover.color = Color.clear;
    }
    
    public void CoverExclude()
    {
        Cover.color = CoverExcludeColor;
    }

    private void OnSelect()
    {
        if (Tile.Identity == TileIdentity.Empty) return;
        
        if (isSelected)
        {
            Debug.Log("Double select");
            Handler.HandleSelectContext();
            isSelected = false;
        }
        else
        {
            Handler.HandleSelect(this);
            HighlightBorder.color = BorderSelectColor;
            isSelected = true;
        }
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        
        OnSelect();
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UIGameBoard : UINode, IHandlesTileContainers
{
    [Space]
    
    public UITileContainer ContainerPrefab;

    private List<List<UITileContainer>> Grid;

    private bool hasContext;
    private Dictionary<UITileContainer, int> SelectContext;
    
    private List<UITileContainer> CoverContext;
    private TileIdentity coveringExcept;
    
    protected override void Awake()
    {
        base.Awake();

        Grid = new List<List<UITileContainer>>();
        SelectContext = new Dictionary<UITileContainer, int>();
        CoverContext = new List<UITileContainer>();
        
        GetComponent<GridLayoutGroup>().startAxis = GridLayoutGroup.Axis.Vertical;
        GetComponent<GridLayoutGroup>().startCorner = GridLayoutGroup.Corner.LowerLeft;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ClearContext();
        }
    }

    public override void Show(bool showChildren = false)
    {
        base.Show(showChildren);
        
        CreateGame();
    }
    
    public override void Hide(bool hideChildren = true)
    {
        base.Hide(hideChildren);
    }

    private void CreateGame()
    {
        for (int x = 0; x < 10; x++)
        {
            Grid.Add(new List<UITileContainer>());
            for (int y = 0; y < 10; y++)
            {
                UITileContainer container = Instantiate(ContainerPrefab, transform);
                container.SetFields(x, y, TileData.GetRandom());
                Grid[x].Add(container);
            }
        }
    }

    private void EndGame()
    {
        foreach (UITileContainer container in Grid.SelectMany(row => row))
        {
            Destroy(container.gameObject);
        }
    }
    
    public void HandleSelectContext()
    {
        Debug.Log("Handling select context");
        
        foreach (UITileContainer selected in SelectContext.Keys) selected.Impact(SelectContext[selected]);
        
        ClearContext();
        if (coveringExcept != TileIdentity.Empty) ExcludeAllExcept(coveringExcept);
    }

    public void HandleSelect(UITileContainer container)
    {
        CollectContext(container);
    }

    private void CollectContext(UITileContainer origin)
    {
        if (hasContext) ClearContext();
        
        CollectModifierContext(origin);
        
        hasContext = true;
        foreach (UITileContainer container in SelectContext.Keys)
        {
            container.Highlight();
            container.PreviewImpact(SelectContext[container]);
        }
    }
    
    private void ClearContext()
    {
        if (!hasContext) return;
        
        foreach (UITileContainer container in SelectContext.Keys)
        {
            container.DeHighlight();
            container.InContext = false;
        }

        hasContext = false;
        SelectContext.Clear();
    }
    
    #region Modifier Context Gathering

    private void CollectModifierContext(UITileContainer origin)
    {
        switch (origin.Tile.Modifier)
        {
            case TileModifier.None:
                CollectNoneContext(origin);
                break;
            case TileModifier.Row:
                CollectRowContext(origin);
                break;
            case TileModifier.Col:
                CollectColumnContext(origin);
                break;
            case TileModifier.Radius:
                CollectRadiusContext(origin);
                break;
            case TileModifier.SqrRadius:
                CollectSqrRadiusContext(origin);
                break;
            case TileModifier.Clump:
                CollectClumpContext(origin);
                break;
            case TileModifier.LRDiagonal:
                CollectLRDiagonalContext(origin);
                break;
            case TileModifier.RLDiagonal:
                CollectRLDiagonalContext(origin);
                break;
            case TileModifier.CompleteDiagonal:
                CollectCompleteDiagonalContext(origin);
                break;
            case TileModifier.Shield:
                CollectNoneContext(origin);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    private void AddContext(UITileContainer origin, UITileContainer container)
    {

        if (origin != container)
        {
            if (SelectContext.ContainsKey(container)) SelectContext[container] += 1;
            else SelectContext[container] = 1;
        }
        else SelectContext.TryAdd(container, 1);
        
        Debug.Log($"[ {origin.Tile.Identity}-{origin.Tile.Modifier} ] Adding context for {container.Tile.Identity}-{container.Tile.Modifier} => {SelectContext[container]}");
        
        if (container.InContext) return;
        
        container.InContext = true;

        if (origin == container) return;
        
        CollectModifierContext(container);
    }

    private bool ValidCoords(int x, int y) => x is >= 0 and < 10 && y is >= 0 and < 10;
    private bool ValidTile(TileData comparison, TileData other) => comparison.ShouldGroup(other);

    private void CollectNoneContext(UITileContainer origin)
    {
        AddContext(origin, origin);
    }

    private void CollectRowContext(UITileContainer origin)
    {
        // add origin
        AddContext(origin, origin);
        
        // move out left
        for (int i = 0; i < origin.X + 1; i++)
        {
            UITileContainer check = Grid[origin.X - i][origin.Y];
            if (ValidTile(origin.Tile, check.Tile))
            {
                AddContext(origin, check);
                if (check.Tile.Modifier == TileModifier.Shield) break;
            }
        }
        
        // move out right
        for (int i = origin.X + 1; i < 10; i++)
        {
            UITileContainer check = Grid[i][origin.Y];
            if (ValidTile(origin.Tile, check.Tile))
            {
                AddContext(origin, check);
                if (check.Tile.Modifier == TileModifier.Shield) break;
            }
        }
    }
    
    private void CollectColumnContext(UITileContainer origin)
    {
        // add origin
        AddContext(origin, origin);
        
        // move out down
        for (int i = 0; i < origin.Y + 1; i++)
        {
            UITileContainer check = Grid[origin.X][origin.Y - i];
            if (ValidTile(origin.Tile, check.Tile))
            {
                AddContext(origin, check);
                if (check.Tile.Modifier == TileModifier.Shield) break;
            }
        }
        
        // move out up
        for (int i = origin.Y + 1; i < 10; i++)
        {
            UITileContainer check = Grid[origin.X][i];
            if (ValidTile(origin.Tile, check.Tile))
            {
                AddContext(origin, check);
                if (check.Tile.Modifier == TileModifier.Shield) break;
            }
        }
    }
    
    private void CollectRadiusContext(UITileContainer origin)
    {
        int charges = origin.Tile.Charges;
        for (int x = origin.X - charges; x < origin.X + charges; x++)
        {
            for (int y = origin.Y - charges; y < origin.Y + charges; y++)
            {
                if (!ValidCoords(x, y)) continue;
                if (Vector2.Distance(new Vector2(origin.X, origin.Y), new Vector2(x, y)) > charges) continue;

                UITileContainer check = Grid[x][y];
                if (ValidTile(origin.Tile, check.Tile)) AddContext(origin, check);
            }
        }
    }
    
    private void CollectSqrRadiusContext(UITileContainer origin)
    {
        int charges = origin.Tile.Charges;
        for (int x = origin.X - charges; x < origin.X + charges; x++)
        {
            for (int y = origin.Y - charges; y < origin.Y + charges; y++)
            {
                if (!ValidCoords(x, y)) continue;

                UITileContainer check = Grid[x][y];
                if (ValidTile(origin.Tile, check.Tile)) AddContext(origin, check);
            }
        }
    }
    
    private void CollectClumpContext(UITileContainer origin)
    {
        HashSet<UITileContainer> visited = new HashSet<UITileContainer>();
        Stack<UITileContainer> toVisit = new Stack<UITileContainer>();
        toVisit.Push(origin);

        while (toVisit.Count > 0)
        {
            UITileContainer curr = toVisit.Pop();
            if (visited.Contains(curr)) continue;

            if (ValidTile(origin.Tile, curr.Tile)) AddContext(origin, curr);
            visited.Add(curr);

            // left
            if (ValidCoords(curr.X - 1, curr.Y))
            {
                UITileContainer left = Grid[curr.X - 1][curr.Y];
                if (ValidTile(origin.Tile, left.Tile)) toVisit.Push(left);
            }
            // right
            if (ValidCoords(curr.X + 1, curr.Y))
            {
                UITileContainer right = Grid[curr.X + 1][curr.Y];
                if (ValidTile(origin.Tile, right.Tile)) toVisit.Push(right);
            }
            // up
            if (ValidCoords(curr.X, curr.Y + 1))
            {
                UITileContainer up = Grid[curr.X][curr.Y + 1];
                if (ValidTile(origin.Tile, up.Tile)) toVisit.Push(up);
            }
            // down
            if (ValidCoords(curr.X , curr.Y - 1))
            {
                UITileContainer down = Grid[curr.X ][curr.Y - 1];
                if (ValidTile(origin.Tile, down.Tile)) toVisit.Push(down);
            }
        }
    }

    private void CollectDiagonalContext(UITileContainer origin, int dx, int dy)
    {
        int x = origin.X;
        int y = origin.Y;

        while (true)
        {
            x += dx;
            y += dy;

            if (!ValidCoords(x, y)) break;
            if (ValidTile(origin.Tile, Grid[x][y].Tile))
            {
                AddContext(origin, Grid[x][y]);
                if (Grid[x][y].Tile.Modifier == TileModifier.Shield) break;
            }
        }
    }
    
    // Bottom left to top right
    private void CollectLRDiagonalContext(UITileContainer origin)
    {
        AddContext(origin, origin);
        
        CollectDiagonalContext(origin, -1, -1);  // bottom left
        CollectDiagonalContext(origin, 1, 1);  // top right
    }
    
    private void CollectRLDiagonalContext(UITileContainer origin)
    {
        AddContext(origin, origin);
        
        CollectDiagonalContext(origin, 1, -1);  // bottom right
        CollectDiagonalContext(origin, -1, 1);  // top left
    }
    
    private void CollectCompleteDiagonalContext(UITileContainer origin)
    {
        AddContext(origin, origin);
        
        CollectDiagonalContext(origin, -1, -1);  // bottom left
        CollectDiagonalContext(origin, 1, 1);  // top right
        
        CollectDiagonalContext(origin, 1, -1);  // bottom right
        CollectDiagonalContext(origin, -1, 1);  // top left
    }
    
    #endregion
    
    #region Special Covers
    
    public void UncoverAll()
    {
        foreach (UITileContainer container in CoverContext) container.Uncover();
        CoverContext.Clear();
        coveringExcept = TileIdentity.Empty;
    }

    public void OnlyBlue() => ExcludeAllExcept(TileIdentity.Blue);
    public void OnlyGreen() => ExcludeAllExcept(TileIdentity.Green);
    public void OnlyYellow() => ExcludeAllExcept(TileIdentity.Yellow);
    public void OnlyRed() => ExcludeAllExcept(TileIdentity.Red);
    public void OnlyPink() => ExcludeAllExcept(TileIdentity.Pink);

    private void ExcludeAllExcept(TileIdentity includeIdentity)
    {
        UncoverAll();
        coveringExcept = includeIdentity;
        
        foreach (List<UITileContainer> row in Grid)
        {
            foreach (UITileContainer container in row)
            {
                if (container.Tile.Identity != includeIdentity) CoverContext.Add(container);
            }
        }

        foreach (UITileContainer container in CoverContext) container.CoverExclude();
    }
    
    #endregion
}

public interface IHandlesTileContainers
{
    public void HandleSelectContext();
    public void HandleSelect(UITileContainer container);
}
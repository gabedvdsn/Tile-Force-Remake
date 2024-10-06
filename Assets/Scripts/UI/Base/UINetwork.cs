using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UINetwork : MonoBehaviour
{
    public UIRootNode RootNode;
    public UINode StartNode;
    public bool Log = false;
    public bool AlwaysLogHistory = true;
    
    private Stack<UINode> History;
    private List<UINode> UntrackedNavigations;
    
    protected virtual void Awake()
    {
        History = new Stack<UINode>();
        UntrackedNavigations = new List<UINode>();

        if (RootNode is null) RootNode = gameObject.AddComponent<UIRootNode>();
        else RootNode.gameObject.SetActive(true);
    }

    protected virtual void Start()
    {
        
    }

    public virtual void Prepare()
    {
        // Make sure all all GameObjects in UI hierarchy are active
        RootNode.SetActiveGameObjectChildren(gameObject, true);
        
        // Nodes find their own children recursively
        RootNode.RecFindDirectLineage(RootNode, gameObject);

        // Hide all nodes in network
        RootNode.HideChildren();
        
        // Root and start are the same
        if (RootNode == StartNode) RegisterNavigation(RootNode);
        // Root and start nodes are different, show start node & its ancestors
        else if (StartNode) StartNode.ShowBackwardsToAncestor(StartNode.Network.RootNode);
    }

    public UINode PeekLast()
    {
        if (History.Count <= 1) return null;

        UINode top = History.Pop();
        UINode last = History.Peek();

        History.Push(top);
        return last;
    }

    public bool Back()
    {
        if (History.Count <= 1) return false;

        DeRegisterNavigation(History.Peek(), 1);

        return true;
    }

    /// <summary>
    /// Navigate to node in history. If forceNavigate and node does not exist in history, navigate anyway.
    /// </summary>
    /// <param name="node"></param>
    /// <param name="forceNavigate"></param>
    /// <returns></returns>
    public bool NavigateTo(UINode node, bool forceNavigate = false)
    {
        if (Log) Debug.Log($"[ NAVIGATION ] NavigateTo {node.name}");
        
        if (DeRegisterNavigation(node, 1)) return true;
        
        if (Log) Debug.Log($"\t[ NAVIGATION ] Could not navigate via deregister");

        if (forceNavigate) return RegisterNavigation(node);

        return false;
    }

    public bool RegisterUntrackedNavigation(UINode node)
    {
        node.Show();
        if (!UntrackedNavigations.Contains(node)) UntrackedNavigations.Add(node);

        return true;
    }

    public bool DeRegisterUntrackedNavigation(UINode node)
    {
        node.Hide();
        if (UntrackedNavigations.Contains(node)) UntrackedNavigations.Remove(node);

        return true;
    }
    
    public bool RegisterNavigation(UINode node)
    {
        if (History.Contains(node)) return NavigateTo(node);

        if (Log) Debug.Log($"[ NAVIGATION ] Registering {node.name}");
        
        History.Push(node);
        node.Show();

        List<UINode> historyList = History.ToList();
        if (AlwaysLogHistory) Debug.Log($"[ NAVIGATION HISTORY ]");
        for (int i = 0; i < historyList.Count; i++)
        {
            if (AlwaysLogHistory) Debug.Log($"\t[ {historyList.Count - i - 1} ] {historyList[i]}");
        }
        
        return true;
    }

    /// <summary>
    /// Deregister a navigation from History, or up to a certain navigation with an offset
    /// </summary>
    /// <param name="node">The node to deregister</param>
    /// <param name="furtherOffset">The offset from node to deregister at</param>
    /// <returns></returns>
    public bool DeRegisterNavigation(UINode node, int furtherOffset = 0)
    {
        if (node == RootNode) return false;
        if (!History.Contains(node)) return false;

        List<UINode> temp = History.ToList();
        int index = temp.IndexOf(node);
        
        for (int i = furtherOffset; i < temp.Count - index; i++)
        {
            Debug.Log($"[ NAVIGATION ] Deregistering {History.Peek().name}");
            History.Pop().Hide();
        }

        History.Peek().Show();

        List<UINode> historyList = History.ToList();
        if (AlwaysLogHistory) Debug.Log($"[ NAVIGATION HISTORY ]");
        for (int i = 0; i < historyList.Count; i++)
        {
            if (AlwaysLogHistory) Debug.Log($"\t[ {historyList.Count - i - 1} ] {historyList[i]}");
        }
        
        return true;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(RectTransform))]
public class UINode : MonoBehaviour
{
    [Header("UI Node")]
    
    [Tooltip("Hide this node when navigating child UI")]
    public bool HideOnFurtherOpen;
    [Tooltip("Hide sibling nodes when navigating to this node")]
    public bool HideSiblingsOnOpen;
    [Tooltip("Show children nodes when navigated to")]
    public bool AlwaysShowChildren;
    [Tooltip("Deactivate dependent references on show")]
    public bool DeactivateReferencesOnOpen;
    
    public UINavigationTransitionMeta ShowTransition
    {
        get => ConcreteShowTransition;
        set
        {
            if (value is null)
            {
                ShowTransition = UINavigationTransitionMeta.NoneTransition();
                return;
            }
            
            ConcreteShowTransition = value;
            foreach (UINode child in Children) child.ShowTransition = ConcreteShowTransition;

            if (!AncestorPassthroughNode) return;
            AncestorPassthroughNode.ShowTransition = ConcreteShowTransition;
        }
    }
    public UINavigationTransitionMeta ConcreteShowTransition = UINavigationTransitionMeta.NoneTransition();
    
    public UINavigationTransitionMeta HideTransition
    {
        get => ConcreteHideTransition;
        set
        {
            if (value is null)
            {
                ShowTransition = UINavigationTransitionMeta.NoneTransition();
                return;
            }
            
            ConcreteHideTransition = value;
            foreach (UINode child in Children) child.ShowTransition = ConcreteHideTransition;

            if (!AncestorPassthroughNode) return;
            AncestorPassthroughNode.HideTransition = ConcreteHideTransition;
        }
    }
    public UINavigationTransitionMeta ConcreteHideTransition = UINavigationTransitionMeta.NoneTransition();
    
    
    [HideInInspector] public UINetwork Network;
    [HideInInspector] public UINode Parent;
    [HideInInspector] public List<UINode> Children;
    
    [HideInInspector] public List<IUIReference> DependentReferences;
    [HideInInspector] public UIPassthroughNode AncestorPassthroughNode;

    protected CanvasGroup canvasGroup;
    public bool Shown { get; private set; }

    protected virtual void Awake()
    {
        Network = GetComponentInParent<UINetwork>();
        canvasGroup = GetComponent<CanvasGroup>();
        
        Children = new List<UINode>();
        DependentReferences = new List<IUIReference>();
    }

    protected virtual void Start()
    {
        
    }

    public void RecFindDirectLineage(UINode parent, GameObject curr, UIPassthroughNode passthrough = null, int iteration = 0)
    {
        foreach (Transform childTransform in curr.transform)
        {
            string ptString = passthrough is null ? "NONE" : passthrough.name;
            string tabString = "";
            for (int i = 0; i < iteration; i++)
            {
                tabString += "\t";
            }
            if (Network.Log) Debug.Log($"{tabString}[ UI-LINEAGE ] [ {iteration} ] {parent.name} -> {curr.name} ({ptString})");
            
            UIPassthroughNode localPassthrough = childTransform.GetComponent<UIPassthroughNode>();
            if (localPassthrough)
            {
                localPassthrough.Parent = parent;
                localPassthrough.AncestorPassthroughNode = passthrough;
                localPassthrough.RecFindDirectLineage(parent, localPassthrough.gameObject, localPassthrough, iteration + 1);
            }
            else
            {
                UINode childNode = childTransform.GetComponent<UINode>();

                if (childNode)
                {
                    childNode.Parent = parent;
                    childNode.AncestorPassthroughNode = passthrough;
                
                    parent.Children.Add(childNode);
                
                    childNode.RecFindDirectLineage(childNode, childNode.gameObject, null, iteration + 1);
                }
                else RecFindDirectLineage(parent, childTransform.gameObject, passthrough, iteration + 1);
            }
        }
    }
    
    /// <summary>
    /// Open child UI nodes
    /// </summary>
    /// <param name="forwardNode"></param>
    public bool OpenForward(UINode forwardNode)
    {
        if (Network.Log) Debug.Log($"{name} is opening forward -> {forwardNode.name}");
        
        if (!IsParentOf(forwardNode)) return false;
        
        if (Network.Log) Debug.Log($"{name} succeeded in opening -> {forwardNode.name}");
        
        return OpenPublic(forwardNode, HideOnFurtherOpen);
    }

    /// <summary>
    /// Open any UI node from this node. It does not need to be a child node.
    /// </summary>
    /// <param name="publicNode"></param>
    /// <param name="hideThis"></param>
    public bool OpenPublic(UINode publicNode, bool hideThis)
    {
        if (hideThis && HideOnFurtherOpen) Hide();
        
        return Network.RegisterNavigation(publicNode);
    }

    public bool IsParentOf(UINode otherNode)
    {
        if (this == otherNode) return true;

        foreach (UINode child in Children)
        {
            if (child.IsParentOf(otherNode)) return true;
        }

        return false;
    }

    #region Node Hide/Show
    
    public virtual void Show(bool showChildren = false)
    {
        if (Network.Log) Debug.Log($"Showing {name}");
        
        Shown = true;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;

        if (ShowTransition.Duration > 0) StartCoroutine(DoTransition(true, ConcreteShowTransition));
        else canvasGroup.alpha = 1;
        
        if (HideSiblingsOnOpen) HideSiblings();

        if (!DeactivateReferencesOnOpen) SetDependentReferenceStatus(true);

        if (AncestorPassthroughNode) AncestorPassthroughNode.Show();
        
        if (showChildren || AlwaysShowChildren) ShowChildren();
    }

    public virtual void ShowBackwardsToAncestor(UINode ancestor)
    {
        // Reached root node
        if (Parent is null) return;

        if (this == ancestor)
        {
            Network.RegisterNavigation(this);
            return;
        }
        
        Parent.ShowBackwardsToAncestor(ancestor);

        Network.RegisterNavigation(this);
    }

    public virtual void Hide(bool hideChildren = true)
    {
        if (Network.Log) Debug.Log($"Hiding {name}");
        
        Shown = false;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        if (HideTransition.Duration > 0) StartCoroutine(DoTransition(false, ConcreteHideTransition));
        else canvasGroup.alpha = 0;

        SetDependentReferenceStatus(false);
        
        if (AncestorPassthroughNode) AncestorPassthroughNode.Hide();
        
        if (hideChildren) HideChildren();
    }

    public void ShowChildren()
    {
        foreach (UINode child in Children) child.Show(true);
    }

    public void HideChildren()
    {
        foreach (UINode child in Children) child.Hide();
    }
    
    public void HideSiblings()
    {
        if (Parent is null) return;
        
        foreach (UINode sibling in Parent.Children)
        {
            if (this == sibling) continue;
            sibling.Hide();
        }
    }
    
    #endregion
    
    private IEnumerator DoTransition(bool show, UINavigationTransitionMeta meta)
    {
        if (show) ToggleInteractivity(false, true);
        
        RectTransform rTransform = GetComponent<RectTransform>();
        Vector2 origin = meta.TransitionValue.Origin;
        Vector2 target = origin + meta.TransitionValue.Offset;
        
        Debug.Log($"{show} -> Transitioning {name} for {meta.Duration} from {origin} to {target}");

        if (!meta.TransitionValue.Fade) canvasGroup.alpha = 1;
        float initialAlpha = canvasGroup.alpha;
        float targetAlpha = show ? 1 : 0;
        
        float elapsedDuration = 0f;
        while (elapsedDuration < meta.Duration)
        {
            float progress = elapsedDuration / meta.Duration;
            rTransform.anchoredPosition = Vector2.Lerp(origin, target, progress);
            if (meta.TransitionValue.Fade) canvasGroup.alpha = Mathf.Lerp(initialAlpha, targetAlpha, progress);

            elapsedDuration += Time.deltaTime;
            yield return null;
        }
        
        if (show)
        {
            canvasGroup.alpha = 1;
            // If showing, node is shown, set to target position
            rTransform.anchoredPosition = target;
            ToggleInteractivity(true, true);
        }
        else
        {
            canvasGroup.alpha = 0;
            // If hiding, node is hidden, reset back to origin position
            rTransform.anchoredPosition = origin;
        }
    }
    
    #region Node Interactivity

    public void ToggleInteractivity(bool flag, bool setLineage)
    {
        canvasGroup.interactable = flag;

        if (!setLineage) return;

        foreach (UINode child in Children)
        {
            child.ToggleInteractivity(flag, true);
        }
    }

    public void ToggleIgnoreParent(bool flag, bool setLineage)
    {
        canvasGroup.ignoreParentGroups = flag;

        if (!setLineage) return;

        foreach (UINode child in Children)
        {
            child.ToggleIgnoreParent(flag, true);
        }
    }
    
    #endregion
    
    #region Dependent Behaviours

    public void RegisterDependentReference(IUIReference uiReference)
    {
        if (Network.Log) Debug.Log($"{uiReference}");
        if (!DependentReferences.Contains(uiReference)) DependentReferences.Add(uiReference);
    }

    public void SetDependentReferenceStatus(bool flag)
    {
        foreach (IUIReference uiReference in DependentReferences)
        {
            if (flag) uiReference.Activate(true);
            else uiReference.Deactivate(true);
        }
    }
    
    #endregion
    
    #region GameObject Hide/Show

    public void SetActiveGameObjectChildren(GameObject parentObj, bool onlyNodes = false)
    {
        foreach (Transform child in parentObj.transform)
        {
            if (onlyNodes)
            {
                if (child.gameObject.GetComponent<UINode>()) child.gameObject.SetActive(true);
            }
            else child.gameObject.SetActive(true);
            // ReSharper disable once Unity.InefficientPropertyAccess
            SetActiveGameObjectChildren(child.gameObject, onlyNodes);
        }
    }

    public void SetInactiveGameObjectChildren()
    {
        foreach (Transform child in transform)
        {
            RecHideGameObjectChildren(child.gameObject);
        }
    }
    
    private void RecHideGameObjectChildren(GameObject parentObj)
    {
        foreach (Transform child in parentObj.transform)
        {
            RecHideGameObjectChildren(child.gameObject);
        }
        
        gameObject.SetActive(false);
    }
    
    #endregion

    
}

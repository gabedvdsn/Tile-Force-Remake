using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIButtonNavigationBehaviour : AbstractUIBehaviour<bool, UINode>
{
    public UINode ConnectNode;
    
    [Space]
    
    public bool useTransition;

    public UINavigationTransition Transition;
    
    private Button button;

    protected override void Start()
    {
        base.Start();
        
        button = gameObject.GetComponent<Button>();
        button.onClick.AddListener(() => Perform());
    }

    public override bool Perform()
    {
        return false;
    }

    protected void SetNavigationTransitions()
    {
        if (!useTransition) return;

        ConnectNode ??= ControlNode.Network.PeekLast();
        Debug.Log($"CONNECT NODE: {ConnectNode}");
        Debug.Log($"CONTROL NODE: {ControlNode}");
        
        // Show connect
        ConnectNode.ShowTransition = Transition.ToTransition.ToMeta(Transition);
            
        // Hide control
        ControlNode.HideTransition = Transition.FromTransition.ToMeta(Transition);
    }
    
    public override void SendData(UINode data)
    {
        if (data is null) return;
        
        ConnectNode = data;
    }
    
    public override bool GetData()
    {
        return isActive;
    }

    public override void Activate(bool activateBehaviour)
    {
        if (activateBehaviour) button.interactable = true;
        base.Activate(activateBehaviour);
    }
    
    public override void Deactivate(bool deactivateBehaviour)
    {
        if (deactivateBehaviour) button.interactable = false;
        base.Deactivate(deactivateBehaviour);
    }

    private void OnDestroy()
    {
        gameObject.GetComponent<Button>().onClick.RemoveListener(() => Perform());
    }
}

[Serializable]
public class UINavigationTransition
{
    public float Duration;
    
    [Space]
    
    [Tooltip("Use this node as local instead of parent node")]
    public UINode OverrideToNode;
    public UINavigationTransitionValue FromTransition;
    public UINavigationTransitionValue ToTransition;

    [HideInInspector] public UINode ToNode;
    
    // Realtime toNode transition tracking
    private RectTransform toTransform;
    private CanvasGroup toCanvasGroup;
    private Vector2 toOrigin;
    private Vector2 toTarget;

    public void SetupToNodeTransition(CanvasGroup canvasGroup)
    {
        if (!ToNode) return;

        toTransform = ToNode.GetComponent<RectTransform>();
        toCanvasGroup = canvasGroup;

        toTarget = ToTransition.Origin;
        Debug.Log(toTarget);
        toOrigin = toTarget - ToTransition.Offset;
    }

    public void TransitionToNode(bool show, float externalProgress)
    {
        if (!ToNode) return;
        if (ToTransition.Fade) toCanvasGroup.alpha = show ? Mathf.Lerp(0, 1, externalProgress) : Mathf.Lerp(1, 0, externalProgress);
        toTransform.anchoredPosition = Vector2.Lerp(toOrigin, toTarget, ToTransition.Progress(externalProgress));
    }

    public void FinishToNodeTransition(bool show)
    {
        toCanvasGroup.alpha = show ? 1 : 0;
        toTransform.anchoredPosition = ToTransition.Origin;
    }

    public UINavigationTransition Get(UIButtonNavigationBehaviour navigationBehaviour)
    {
        ToNode = navigationBehaviour.ConnectNode;

        return OverrideToNode ? OverrideToCopy() : this;
    }
    
    public UINavigationTransition OverrideToCopy() => new()
    {
        OverrideToNode = null,
        FromTransition = FromTransition,
        ToTransition = ToTransition,
        ToNode = OverrideToNode
    };

    public static UINavigationTransition NonOffsetTransition(float duration)
    {
        return new()
        {
            Duration = duration,
            FromTransition = UINavigationTransitionValue.IncompleteTransition(true)
        };
    }
}

[Serializable]
public class UINavigationTransitionValue
{
    public bool Fade;
    [FormerlySerializedAs("Target")] public Vector2 Origin;
    public Vector2 Offset;
    public AnimationCurve Timing;
    
    private bool complete = true;

    public float Progress(float externalProgress) => complete ? Timing.Evaluate(externalProgress) : 0f;

    public static UINavigationTransitionValue IncompleteTransition(bool fade)
    {
        return new()
        {
            Fade = fade,
            Origin = Vector2.zero,
            Offset = Vector2.zero,
            Timing = AnimationCurve.Constant(0, 1, 1),
            complete = false
        };
    }
    
    public UINavigationTransitionMeta ToMeta(UINavigationTransition transition)
    {
        return new()
        {
            Duration = transition.Duration,
            TransitionValue = this
        };
    }
}

public class UINavigationTransitionMeta
{
    public float Duration;
    public UINavigationTransitionValue TransitionValue;

    public static UINavigationTransitionMeta NoneTransition()
    {
        return new()
        {
            Duration = 0f,
            TransitionValue = UINavigationTransitionValue.IncompleteTransition(false)
        };
    }

    public static UINavigationTransitionMeta NoneTransition(float duration)
    {
        return new()
        {
            Duration = duration,
            TransitionValue = UINavigationTransitionValue.IncompleteTransition(true)
        };
    }
}
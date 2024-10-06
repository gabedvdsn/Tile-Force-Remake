using UnityEngine;

public abstract class AbstractUIBehaviour<S, R> : MonoBehaviour, IUIReference
{
    [HideInInspector] public UINode ControlNode;

    protected bool isActive;
    
    protected virtual void Start()
    {
        ControlNode = GetComponentInParent<UINode>();       
        ControlNode.RegisterDependentReference(this);
    }

    public abstract bool Perform();

    public virtual void Activate(bool activateBehaviour)
    {
        isActive = true;
    }

    public virtual void Deactivate(bool deactivateBehaviour)
    {
        isActive = false;
    }

    public abstract void SendData(R data);
    public abstract S GetData();
}

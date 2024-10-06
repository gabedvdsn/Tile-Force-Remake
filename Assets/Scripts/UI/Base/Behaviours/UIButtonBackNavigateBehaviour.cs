using UnityEngine;

public class UIButtonBackNavigateBehaviour : UIButtonNavigationBehaviour
{
    [Tooltip("Should always show parent instead of historical previous node. Overrides null ConnectNode navigation.")]
    public bool LocalBack;
    
    public override bool Perform()
    {
        SetNavigationTransitions();
        
        if (ConnectNode is not null && !LocalBack)
        {
            if (ControlNode.Network.Log) Debug.Log($"[ UI-BEHAVIOUR ] Back navigate to {ConnectNode.name}");
            return ControlNode.Network.NavigateTo(ConnectNode);
        }

        if (LocalBack)
        {
            // Failed to deregister control node, must not exist in history
            if (!ControlNode.Network.DeRegisterNavigation(ControlNode)) return ControlNode.Network.NavigateTo(ControlNode.Network.RootNode);

            return ControlNode.Network.RegisterNavigation(ControlNode.Parent);
        }
        
        if (ControlNode.Network.Log) Debug.Log($"[ UI-BEHAVIOUR ] Back navigate");
        return ControlNode.Network.Back();
    }
}

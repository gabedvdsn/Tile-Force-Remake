using System.Collections;
using UnityEngine;

public class UIButtonPublicNavigationBehaviour : UIButtonNavigationBehaviour
{
    public override bool Perform()
    {
        SetNavigationTransitions();
        
        if (ControlNode.Network.Log) Debug.Log($"[ UI-BEHAVIOUR ] Open public to {ConnectNode.name}");
        return ControlNode.OpenPublic(ConnectNode, ControlNode.HideOnFurtherOpen);
    }
}

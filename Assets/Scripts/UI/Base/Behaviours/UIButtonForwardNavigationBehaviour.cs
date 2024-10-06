using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIButtonForwardNavigationBehaviour : UIButtonNavigationBehaviour
{
    public override bool Perform()
    {
        SetNavigationTransitions();
        
        if (ControlNode.Network.Log) Debug.Log($"[ UI-BEHAVIOUR ] Open forward to {ConnectNode.name}");
        return ControlNode.OpenForward(ConnectNode);
    }
}

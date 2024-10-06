using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UIButtonEventBehaviour : UIButtonNavigationBehaviour
{
    public UnityEvent PerformEvent;
    
    public override bool Perform()
    {
        if (ControlNode.Network.Log) Debug.Log($"[ UI-BEHAVIOUR ] Event invoked from {ControlNode.name}");
        PerformEvent?.Invoke();

        return true;
    }

    
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIRootNode : UINode
{
    protected override void Start()
    {
        HideOnFurtherOpen = false;
        
        // Prepare the tree
        Network.Prepare();
    }
}

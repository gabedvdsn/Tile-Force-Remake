using System;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;

public class UIPassthroughNode : UINode
{
    protected override void Awake()
    {
        base.Awake();

        // A stable edge node and default node cannot both exist on the same object. Remove stable edge.
        UINode[] nodesOnObject = GetComponents<UINode>();
        if (nodesOnObject.Length > 1)
        {
            Destroy(this);
        }
    }

    private void OnValidate()
    {
        if (GetComponent<Image>()) throw new Exception("Passthrough node GameObjects cannot have an image component.");
    }
}

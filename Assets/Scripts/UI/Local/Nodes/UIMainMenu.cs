using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class UIMainMenu : UINode
{

    public void OnClickQuit()
    {
#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
    }
}

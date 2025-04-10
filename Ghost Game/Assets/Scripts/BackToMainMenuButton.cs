using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackToMainMenuButton : MonoBehaviour
{
    SceneSwitchManager sceneSwitchManager;

    // Start is called before the first frame update
    void Start()
    {
        sceneSwitchManager = GameObject.FindFirstObjectByType<SceneSwitchManager>();
    }

    public void LoadMainMenuScene()
    {
        sceneSwitchManager.LoadStartMenuScene();
    }
}

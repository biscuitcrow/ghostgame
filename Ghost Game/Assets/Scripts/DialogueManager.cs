using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    #region // <------- SINGLETON PATTERN -------> //
    private static DialogueManager _instance;
    public static DialogueManager Instance
    {
        get
        {
            // Create logic to create the instance
            if (_instance == null)
            {
                GameObject obj = new GameObject("Dialogue Manager");
                obj.AddComponent<DialogueManager>();
            }

            return _instance;
        }
    }

    private void Awake()
    {
        _instance = this;
    }
    #endregion

    [TextArea(5, 10)] public string startTeachMovementText;
    [TextArea(5, 10)] public string startTeachThrowText;
    [TextArea(5, 10)] public string throwTextOne;
    [TextArea(5, 10)] public string throwTextTwo;
    [TextArea(5, 10)] public string prepToFinishTutOne;
    [TextArea(5, 10)] public string prepToFinishTutTwo;
    [TextArea(5, 10)] public string NPCLivedText;

}

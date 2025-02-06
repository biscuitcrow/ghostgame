using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    public enum ObjectType
    {
        Throwable, Togglable
    }

    public enum PhobiaType
    {
        Lights, Audio, Movement
    }

    [Header("Object Details")]
    public string objectName;
    public float objectWeight;
    public ObjectType objectType;
    public PhobiaType phobiaType;
    public Outline outline;
    public bool isToggledOn; // Only relevant for togglable objects

    private void Start()
    {
        outline = gameObject.AddComponent<Outline>();
        outline.enabled = false;
        outline.OutlineColor = Color.green;
    }

}

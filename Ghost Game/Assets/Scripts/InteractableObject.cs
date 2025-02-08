using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    private LayerMask NPCLayer;
    public bool isCanScareNPC;

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
    private float scareRadius = 5f;

    [Header("Toggalable Details")]
    public bool isToggledOn; 
    public GameObject[] listOfGameObjectsToToggle;
    private float toggleCooldown = 0.5f; 

    

    private void Start()
    {
        outline = gameObject.AddComponent<Outline>();
        outline.enabled = false;
        outline.OutlineColor = Color.green;

        isCanScareNPC = false; 
        NPCLayer = LayerMask.GetMask("NPC");
    }

    private void ScareAllNPCsInRange(Vector3 scarePoint)
    {
        // Scare all NPCs within range of the collision
        foreach (Collider c in Physics.OverlapSphere(scarePoint, scareRadius, NPCLayer))
        {
            NPCBehaviour NPCScript = c.gameObject.GetComponent<NPCBehaviour>();
            NPCScript.currentScarePosition = scarePoint;
            NPCScript.isNPCScared = true;
        }
    }

    // <----------------------------------------- TOGGABLABLES ----------------------------------------- > //

    public void ToggleObject()
    {
        isToggledOn = !isToggledOn;
        foreach(GameObject obj in listOfGameObjectsToToggle)
        {
            obj.SetActive(!obj.activeSelf);
        }

        ScareAllNPCsInRange(transform.position);
    }


    // <---------------------------------- THROWABLES (AND COLLISIONS) ---------------------------------- > //

    private void OnCollisionEnter(Collision other)
    {
        // If this object's can scare NPC flag is still up (set in PlayerController script)
        if (isCanScareNPC)
        {
            // Get the first contact point of the collision
            Vector3 contactPoint = other.GetContact(0).point;

            ScareAllNPCsInRange(contactPoint);

            // Drop the can scare NPC flag on this object
            isCanScareNPC = false;
        }
    }

    // <--------------------------------------------- GIZMOS --------------------------------------------- > //

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, scareRadius);
    }


}

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


    [Header("Object Details")]
    public string objectName;
    public float objectWeight;
    public ObjectType objectType;
    public Outline outline;
    private float scareRadius = 5f;
    public float baseFearIncrease = 10f;
    private float phobiaMult = 2f;
    private float finalFearIncrease; 

    [Header("Toggalable Details")]
    public bool isToggledOn; 
    public GameObject[] listOfGameObjectsToToggle;
    private float toggleCooldown = 0.2f;

    private Tags tagList;
    public bool isOnCooldown;

    

    private void Start()
    {
        tagList = gameObject.GetComponent<Tags>();

        outline = gameObject.AddComponent<Outline>();
        outline.enabled = false;
        outline.OutlineColor = Color.green;

        isCanScareNPC = false;
        isOnCooldown = false; 
        NPCLayer = LayerMask.GetMask("NPC");
    }

    private void ScareAllNPCsInRange(Vector3 scarePoint)
    {
        // Scare all NPCs within range of the collision
        foreach (Collider c in Physics.OverlapSphere(scarePoint, scareRadius, NPCLayer))
        {
            // NPC movement set scare point
            NPCBehaviour NPCScript = c.gameObject.GetComponent<NPCBehaviour>();
            NPCScript.currentScarePosition = scarePoint;
            NPCScript.isNPCScared = true;

            // Check for phobia tags: for each tag this object has, check if the NPC has the tag
            bool hasPhobia = false;
            Tags NPCtagList =  c.gameObject.GetComponent<Tags>();
            if (tagList != null)
            {
                foreach (Tag t in tagList.allTags)
                {
                    if (NPCtagList.HasTag(t))
                    {
                        hasPhobia = true;
                    }
                }
            }
            CalculateFearIncrease(hasPhobia);
            NPCScript.IncreaseFearMeter(finalFearIncrease); 
        }

        // Start the object cooldown
        StartCoroutine("ObjectCooldown");
    }

    void CalculateFearIncrease(bool isPhobia)
    {
        if (isPhobia)
        {
            // Activate the phobia multiplier
            finalFearIncrease = baseFearIncrease * phobiaMult;
        }
        else
        {
            finalFearIncrease = baseFearIncrease;
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

    IEnumerator ObjectCooldown()
    {
        isOnCooldown = true;
        yield return new WaitForSeconds(toggleCooldown);
        isOnCooldown = false; 
    }

    // <---------------------------------- THROWABLES (AND COLLISIONS) ---------------------------------- > //

    private void OnCollisionEnter(Collision other)
    {
        // If this object's can scare NPC flag is still up (set in PlayerController script), so each thrown object can only scare once when landing
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

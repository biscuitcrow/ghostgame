using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    #region // <------- VARIABLE DEFINITIONS -------> //

    private LayerMask NPCLayer;
    public bool isCanScareNPC;

    public enum ObjectType
    {
        Throwable, Togglable
    }

    [Header("Object Details")]
    public string objectName;
    public float objectWeight = 0;
    public ObjectType objectType;
    public Outliner outline;
    [SerializeField] float scareRadius = 5f; // If the item is bigger it might be better to have a larger radius
    public float baseFearIncrease = 10f;
    private float finalFearIncrease; 

    [Header("Toggalable Details")]
    public bool isToggledOn; 
    public GameObject[] listOfGameObjectsToToggle;
    public ParticleSystem[] listOfPSToToggle;
    public GameObject[] listOfPSToSpawn;
    private float toggleCooldown = 0.2f;

    private Tags tagList;
    public bool isOnCooldown;
    private AbilitiesManager abilitiesManager;

    #endregion

    private void Start()
    {
        abilitiesManager = AbilitiesManager.Instance;
        tagList = gameObject.GetComponent<Tags>();

        outline = gameObject.AddComponent<Outliner>();
        outline.OutlineColor = Color.green;
        outline.enabled = false;

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
            finalFearIncrease = baseFearIncrease * abilitiesManager.phobiaMult;
        }
        else
        {
            finalFearIncrease = baseFearIncrease + abilitiesManager.bonusFearIncrease;
        }
    }

    public void ToggleOutline(bool isOutlineOn)
    {
        if (outline != null)
        {
            outline.enabled = isOutlineOn;
        }
    }

    // <----------------------------------------- TOGGABLABLES ----------------------------------------- > //

    public void ToggleObject()
    {
        //Camera shake
        //GameManager.Instance.CameraShake();

        isToggledOn = !isToggledOn;

        // Toggles effects
        foreach(GameObject obj in listOfGameObjectsToToggle)
        {
            obj.SetActive(!obj.activeSelf);
        }

        // Spawns visual effects particle systems
        foreach (ParticleSystem obj in listOfPSToToggle)
        {
            if (isToggledOn)
            {
                obj.gameObject.SetActive(true);
                obj.Play();
            }
            else
                obj.Stop();
        }

        // Spawns visual effects particle systems
        foreach (GameObject obj in listOfPSToSpawn)
        {
            Instantiate(obj, transform.position, Quaternion.identity);
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
            //Camera shake
            GameManager.Instance.CameraShake();
             

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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations;
using EditorAttributes;
using UnityEngine.UI;

public class NPCBehaviour : MonoBehaviour
{
    #region // <------- VARIABLE DEFINITIONS -------> //
    private Transform player;
    private NavMeshAgent agent;
    private Transform NPCStartingPoint;
    [SerializeField] private bool isUsingFixedDestPoints = true;
    private float maxNavMeshDist = 2f; // Maximum value is twice the agent's height according to Unity's documentation
    private float minNPCToWalkPointDist = 1.5f; // Distance of NPC from walk point before it starts recalculating the walkpoint while on patrol
    private float scareCooldown = 0.5f;
    private bool isScareCooldownRunning = false;
    private bool isHauntedCooldownRunning = false;
    public LayerMask groundLayerMask;
    private Animator NPCAnimator;

    [Header("Fear Meter")]
    public GameObject fearMeterObj;
    [SerializeField] private FloatingFearMeter fearMeter;
    [SerializeField] private GameObject fearMeterPrefab;
    private Image scaredIcon;

    [Header("NPC Details")]
    public string NPCName;
    public float currentFear;
    public float maxFear;
    public bool isExorcist;
    public Sprite profileSprite;
    private bool isNPCActive = true;
    public Outliner outline;

    [Header("NPC Phobia")]
    public bool isPhobiaRevealed = false;

    [Header("Leaving House")]
    public bool isNPCLeavingHouse;

    [Header("Patrolling")]
    public Vector3 walkPoint;
    private bool isWalkPointSet;
    public float walkPointRange;
    public float walkSpeed = 15f;
    [SerializeField] private List<Transform> destinationPoints; // Is set in the script on start
  
    [Header("Running")]
    public float runPointRange = 1.5f;
    public float runSpeed = 10f;

    [Header("Normal Sight Range")]
    public float normalSightRange = 3f;
    private bool isGhostInSightRange;

    [Header("Make NPC Scared")]
    public bool isNPCScared; // Note: This bool flag is only used for object scares (raised in the object script) 
    public Vector3 currentScarePosition; // Assign this using the interactable object script

    [Header("Exorcist Specific Stats")]
    public float exorcistSightRange = 5f;
    private float exorcistAttractionRange = 7f;
    private float exorcistRunSpeed = 7f;
    private float NPCsightAngle = 90;
    public float vigilanceCooldown = 1f; // Time taken for the exorcist to not see the ghost before the exorcist stops chasing

    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem skullPS;

    #endregion

    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
        agent = GetComponent<NavMeshAgent>();
        NPCAnimator = GetComponentInChildren<Animator>();
        NPCStartingPoint = GameObject.FindWithTag("NPC Starting Point").transform;
        isNPCScared = false;
        isNPCLeavingHouse = false;

        // Reset the fear meter
        currentFear = 0;

        SetUpFearMeterUI();
        SetUpDestinationPointsList();

        outline = gameObject.AddComponent<Outliner>();
        outline.OutlineColor = Color.green;
        outline.enabled = false;
    }

    void SetUpDestinationPointsList()
    { 
        Transform holder = GameObject.FindWithTag("Destination Points").transform;
        for (int i = 0; i < holder.childCount; i++)
        { 
            destinationPoints.Add(holder.GetChild(i).transform);
        }
    }

    void SetUpFearMeterUI()
    {
        fearMeterObj =  Instantiate(fearMeterPrefab);
        fearMeter = fearMeterObj.GetComponentInChildren<FloatingFearMeter>();
        scaredIcon = fearMeterObj.transform.Find("Scared Icon").GetComponent<Image>();
        PositionConstraint fearMeterPositionConstraint = fearMeterObj.GetComponent<PositionConstraint>();

        // Create new source variable
        ConstraintSource source = new ConstraintSource();
        // Initializes the variable for x's transform
        source.sourceTransform = gameObject.GetComponent<Transform>();
        // Initializes the variable for x's weight just like the 
        // Weight option in the position constraint component
        source.weight = 1;
        // Add the source to the position constraint
        fearMeterPositionConstraint.AddSource(source);
    }

    void CheckIfPullGhost()
    {
        // Manipulating the attraction power
        if (CheckNPCDistanceFromGhost() <= exorcistAttractionRange)
        {
            PullGhost();
            NPCAnimator.SetBool("NPCShooting", true);
        }
        else
        {
            if (AudioManager.instance.CheckIfPlaying("Exorcist Suck"))
            {
                AudioManager.instance.Stop("Exorcist Suck");
            }

            NPCAnimator.SetBool("NPCShooting", false);
            player.gameObject.GetComponent<PlayerController>().NPCforceVector = Vector3.zero;
        }
    }

    void PullGhost()
    {
        if (player.gameObject.activeSelf)
        {
            if (!AudioManager.instance.CheckIfPlaying("Exorcist Suck"))
            {
                AudioManager.instance.Play("Exorcist Suck");
            }
            Vector3 forceDir = transform.position - player.position;
            player.gameObject.GetComponent<PlayerController>().NPCforceVector = forceDir * Time.deltaTime * 0.5f;
        }
    }

    public void ToggleStopNavMeshAgent(bool isStopped)
    {
        gameObject.GetComponent<NavMeshAgent>().isStopped = isStopped;
    }

    void Update()
    { 
        
        // Exorcist NPC behaviour
        if (isExorcist) 
        {
            if (isNPCActive)
            {
                CheckIfGhostIsInNPCSight(exorcistSightRange);
                CheckIfPullGhost();
            }
            
            if (isNPCLeavingHouse)
            {
                LeaveHouse();
                float distanceToStartPoint = (transform.position - NPCStartingPoint.position).magnitude;
                if (distanceToStartPoint <= 1f)
                {
                    NPCLived();
                }

            }
            else if (isGhostInSightRange && CheckIfGhostIsInNPCSightAngle())
            {
                // Start chasing down the ghost
                agent.speed = exorcistRunSpeed; // Made it slow, it's really hard if it's run speed
                agent.SetDestination(player.position);
            }
            else
            {
                agent.speed = walkSpeed;
                Patrolling();
            }
        }

        #region Normal NPC behaviour
        // Normal NPC behaviour
        else
        {
            CheckIfGhostIsInNPCSight(normalSightRange);

            // Movement of NPC
            if (isNPCLeavingHouse)
            {
                LeaveHouse();
                float distanceToStartPoint = (transform.position - NPCStartingPoint.position).magnitude;
                if (distanceToStartPoint <= 1f)
                {
                    NPCLived();
                }

            }
            else
            {
                // If the NPC has been scared, make them run away from the scare 
                if (isNPCScared)
                {
                    MakeNPCScared(currentScarePosition);
                }
                // If the NPC can see the ghost within their sight range, execute some kind of running behaviour
                // Fear meter is increased in the player controller script
                else if (isGhostInSightRange && player.GetComponent<PlayerController>().isGhostVisible)
                {
                    RunningAway(player.position);
                }
                // Otherwise, patrol randomly as per normal (as long as the scare cooldown is over)
                else if (!isScareCooldownRunning)
                {
                    agent.speed = walkSpeed;
                    Patrolling();
                }
            }
        }
        #endregion

        // This is decoupled from movement code so you can scare NPC even when it is leaving the house, it just won't run away from ghost
        // Uses the scare range of the ghost's haunt ability
        if (CheckNPCDistanceFromGhost() < AbilitiesManager.Instance.ghostScareVisibilityRadius)
        {
            outline.enabled = true;
            if (player.GetComponent<PlayerController>().isGhostVisible)
            {
                if (!isHauntedCooldownRunning)
                {
                    // Scares the NPC when ghost becomes visible
                    IncreaseFearMeter(AbilitiesManager.Instance.ghostVisibilityScareValue);
                    StartCoroutine("HauntedCooldown");
                }
            }
        }
        else
        {
            outline.enabled = false;
        }
    }



    // <---------------------------------- SCARE NPC ---------------------------------- > //

    // This method is exclusively for the movement
    void MakeNPCScared(Vector3 scarePosition)
    {
        // Run away from the position of the scare
        RunningAway(scarePosition);

        // Starts a cool down so it doesn't instantly go back into patrol mode
        StartCoroutine("StartScareCooldown");

        isNPCScared = false; // Putting down this flag immediately ensures that MakeNPCScared() only gets called once per scare
        print("NPC is has been scared by object.");
        
    }

    public void IncreaseFearMeter(float increaseValue)
    {
        NPCAnimator.SetBool("NPCRunning", true);
        VFXManager.Instance.InstantiateStartedNPCPS(gameObject.transform);

        // Displays scared icon
        StartCoroutine("DisplayScaredIcon");

        // Scared SFX
        AudioManager.instance.Play(GameManager.Instance.listOfAllNPCScaredSoundNames[Random.Range(0, GameManager.Instance.listOfAllNPCScaredSoundNames.Count)]);

        currentFear += increaseValue; 
        fearMeter.UpdateFearMeterUI(currentFear, maxFear);
        print("Fear meter increased and updated. Increase value: " + increaseValue);

        if (currentFear >= maxFear)
        {
            NPCDied();
        }
    }

    [Button("Kill NPC/NPC Died")]
    public void NPCDied()
    {
        isNPCActive = false;
        outline.enabled = false;
        Instantiate(skullPS, transform.position, Quaternion.identity);
        ToggleStopNavMeshAgent(true);

        // Play NPC death animation
        NPCAnimator.SetTrigger("NPCDied");

        Destroy(fearMeterObj);
        //Destroy(this.gameObject); 
        GameManager.Instance.NPCDied();

    }

    [Button("NPC Lived")]
    public void NPCLived()
    {
        isNPCActive = false;
        outline.enabled = false;
        // Play NPC lived animation and sounds
        VFXManager.Instance.InstantiateRemovalPS(gameObject.transform);
        Destroy(fearMeterObj);
        Destroy(this.gameObject);
        GameManager.Instance.NPCLived();
    }

    private IEnumerator StartScareCooldown()
    {
        isScareCooldownRunning = true;
        yield return new WaitForSeconds (scareCooldown);
        isNPCScared = false;
        isScareCooldownRunning = false;
    }

    private IEnumerator DisplayScaredIcon()
    {
        if (fearMeter.gameObject != null)
        {
            scaredIcon.gameObject.SetActive(true);
            UIManager.Instance.FadeUIGameObject(scaredIcon.gameObject, 0, 1, 0.05f);
            UIManager.Instance.ScalePulseUIGameObject(scaredIcon.gameObject, 0.005f, 0.1f);
        }
        yield return new WaitForSeconds(0.5f);
        if (fearMeter.gameObject != null)
        {
            UIManager.Instance.FadeUIGameObject(scaredIcon.gameObject, 1, 0, 0.05f);
            UIManager.Instance.ScalePulseUIGameObject(scaredIcon.gameObject, 0.005f, 0.1f);
            scaredIcon.gameObject.SetActive(false);
        }
    }

    private IEnumerator HauntedCooldown()
    {
        isHauntedCooldownRunning = true;
        if (isExorcist)
        {
            ToggleStopNavMeshAgent(true);
        }
        yield return new WaitForSeconds(0.6f);
        isHauntedCooldownRunning = false;
        if (isExorcist)
        {
            ToggleStopNavMeshAgent(false);
        }
    }


    void CheckIfGhostIsInNPCSight(float NPCsightRange)
    {
        // Checks if ghost player is in sight range
        // Rn is checking using pure position calculations, eventually can use line of sight calculations to see if the ghost is being blocked by objects
        isGhostInSightRange = (CheckNPCDistanceFromGhost() <= NPCsightRange) ? true : false;
    }

    public float CheckNPCDistanceFromGhost()
    {
        float distanceFromGhost = (transform.position - player.position).magnitude;
        return distanceFromGhost;
    }

    bool CheckIfGhostIsInNPCSightAngle()
    {
        float sightAngle = Mathf.Deg2Rad * NPCsightAngle;
        Vector3 sightVector = transform.forward;
        Vector3 dirVector = (player.position - transform.position).normalized;
        float angle = Mathf.Acos(Vector3.Dot(sightVector, dirVector));
        
        Debug.DrawRay(transform.position, sightVector * exorcistSightRange, Color.green, 0.05f);
        Debug.DrawRay(transform.position, dirVector * exorcistSightRange, Color.green, 0.05f);

        if (Mathf.Abs(angle) < sightAngle && angle > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    // <---------------------------------- NPC MOVEMENT ---------------------------------- > //

    void Patrolling()
    {
        NPCAnimator.SetBool("NPCRunning", false);
        if (!isWalkPointSet)
        {
            SearchWalkPoint();
        }
        else
        {
            agent.SetDestination(walkPoint);
        }

        // Starts finding a new walk point when the NPC is within 1f of the current walk point
        float distanceToWalkPoint = (transform.position - walkPoint).magnitude;
        if (distanceToWalkPoint < minNPCToWalkPointDist) 
        {
            isWalkPointSet = false;
        }
    }

    void SearchWalkPoint()
    {
        if (isUsingFixedDestPoints)
        {
            // Randomly choosing out of an array of fixed destination points
            if (destinationPoints.Count > 0)
            {
                int i = Random.Range(0, destinationPoints.Count);
                walkPoint = destinationPoints[i].position;
                //print("walkPoint set to: " + destinationPoints[i].name);
            }
        }
        else
        {
            // Random Walkpoint Calculations
            // Calculate random walkpoint in range
            float randomZ = Random.Range(-walkPointRange, walkPointRange);
            float randomX = Random.Range(-walkPointRange, walkPointRange);

        
            walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);
        }

        VerifyWalkPointOnNavMesh(walkPoint, true);
        
    }

    void RunningAway(Vector3 scarePosition)
    {
        print("NPC running away from player");
        agent.speed = runSpeed;

        Vector3 dirAwayFromScare = (transform.position - scarePosition).normalized;
        Vector3 pointAwayFromScare = transform.position + (dirAwayFromScare * runPointRange);

        VerifyWalkPointOnNavMesh(pointAwayFromScare, false);

        agent.SetDestination(walkPoint);
    }

    void LeaveHouse()
    {
        VerifyWalkPointOnNavMesh(walkPoint, false);
        agent.SetDestination(NPCStartingPoint.position);
    }

    void VerifyWalkPointOnNavMesh(Vector3 pointToCheck, bool setWalkPointBool)
    {
        // Check if generated walk point is actually near enough to the NavMesh, aka if it's actually walkable
        if (NavMesh.SamplePosition(pointToCheck, out NavMeshHit hit, maxNavMeshDist, NavMesh.AllAreas))
        {
            // Sets the walk point to the nearest point on the NavMesh;
            walkPoint = hit.position;
            if (setWalkPointBool)
            {
                isWalkPointSet = true;
            }
        }
    }

    /*
    private void OnTriggerEnter(Collider other)
    {
        print("Collided with smth.");
        if (isExorcist && other.gameObject.tag == "Player")
        {
            GameManager.Instance.ExorcistKilledGhost();
            print("Exorcist has killed the player");
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        print("Collided with smth.");
        if (isExorcist && other.gameObject.tag == "Player")
        {
            GameManager.Instance.ExorcistKilledGhost();
            print("Exorcist has killed the player");
        }
    }

    */

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, normalSightRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(walkPoint, 2f);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, exorcistSightRange);
    }
}

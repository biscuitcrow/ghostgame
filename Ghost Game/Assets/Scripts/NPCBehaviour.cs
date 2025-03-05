using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations;
using EditorAttributes;

public class NPCBehaviour : MonoBehaviour
{
    #region // <------- VARIABLE DEFINITIONS -------> //
    private Transform player;
    private NavMeshAgent agent;
    public GameObject fearMeterObj;
    private Transform NPCStartingPoint;
    [SerializeField] private bool isUsingFixedDestPoints = true;
    private float maxNavMeshDist = 2f; // Maximum value is twice the agent's height according to Unity's documentation
    private float minNPCToWalkPointDist = 1.5f; // Distance of NPC from walk point before it starts recalculating the walkpoint while on patrol
    private float scareCooldown = 0.5f;
    private bool isScareCooldownRunning = false;
    private bool isHauntedCooldownRunning = false;
    public LayerMask groundLayerMask;
    [SerializeField] private FloatingFearMeter fearMeter;
    [SerializeField] private GameObject fearMeterPrefab;

    [Header("NPC Details")]
    public float currentFear;
    public float maxFear;

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
    public float sightRange = 3f;
    private bool isPlayerInSightRange;

    [Header("Make NPC Scared")]
    public bool isNPCScared; // Note: This bool flag is only used for object scares (raised in the object script) 
    public Vector3 currentScarePosition; // Assign this using the interactable object script

    #endregion

    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
        agent = GetComponent<NavMeshAgent>();
        NPCStartingPoint = GameObject.FindWithTag("NPC Starting Point").transform;
        isNPCScared = false;
        isNPCLeavingHouse = false;
        
        // Reset the fear meter
        currentFear = 0;

        SetUpFearMeterUI();
        SetUpDestinationPointsList();
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
     
    void Update()
    {
        CheckIfGhostIsVisibleToNPC();

        // This is decoupled so you can scare NPC even when it is leaving the house, it just won't run away from ghost
        if (isPlayerInSightRange && player.GetComponent<PlayerController>().isGhostVisible)
        {
            if (!isHauntedCooldownRunning)
            {
                IncreaseFearMeter(AbilitiesManager.Instance.ghostVisibilityScareValue);
                StartCoroutine("HauntedCooldown");
            }
        }

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
            else if (isPlayerInSightRange && player.GetComponent<PlayerController>().isGhostVisible) 
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
        currentFear += increaseValue; 
        fearMeter.UpdateFearMeterUI(currentFear, maxFear);
        print("Fear meter increased and updated. Increase value: " + increaseValue);

        if (currentFear >= maxFear)
        {
            NPCDied();
        }
    }

    [Button("Kill NPC/NPC Died")]
    private void NPCDied()
    {
        // Play NPC death animation and sounds
        Destroy(fearMeterObj);
        Destroy(this.gameObject); 
        GameManager.Instance.NPCDied();
    }

    private void NPCLived()
    {
        // Play NPC lived animation and sounds
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


    private IEnumerator HauntedCooldown()
    {
        isHauntedCooldownRunning = true;
        yield return new WaitForSeconds(0.6f);
        isHauntedCooldownRunning = false;
    }


    void CheckIfGhostIsVisibleToNPC()
    {
        // Checks if ghost player is in sight range
        // Rn is checking using pure position calculations, eventually can use line of sight calculations to see if the ghost is being blocked by objects
        float distanceFromPlayer = (transform.position - player.position).magnitude;
        isPlayerInSightRange = (distanceFromPlayer <= sightRange) ? true : false;
    }


    // <---------------------------------- NPC MOVEMENT ---------------------------------- > //

    void Patrolling()
    {
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, sightRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(walkPoint, 2f);
    }
}

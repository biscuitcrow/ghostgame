using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCBehaviour : MonoBehaviour
{
    private Transform player;
    private NavMeshAgent agent;
    private float maxNavMeshDist = 2f; // Maximum value is twice the agent's height according to Unity's documentation
    private float minNPCToWalkPointDist = 1.5f; // Distance of NPC from walk point before it starts recalculating the walkpoint while on patrol
    private float scareCooldown = 0.5f;
    private bool isScareCooldownRunning = false;
    public LayerMask groundLayerMask;
    

    [Header("Patrolling")]
    public Vector3 walkPoint;
    private bool isWalkPointSet;
    public float walkPointRange;
    public float walkSpeed = 15f;
  

    [Header("Running")]
    public float runPointRange = 1.5f;
    public float runSpeed = 10f;
    public float sightRange = 3f;
    private bool isPlayerInSightRange;

    [Header("Make NPC Scared")]
    public bool isNPCScared; // Note: This bool flag is only used for object scares (raised in the object script) 
    public Vector3 currentScarePosition; // Assign this using the interactable object script 

    [Header("Is Ghost Currently Visible to NPC?")]
    // Determines if the NPC can currently see the ghost player
    public bool isGhostVisible;


     
    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
        agent = GetComponent<NavMeshAgent>();
        isNPCScared = false;

        //For testing, rmb to delete
        //isGhostVisible = true;
    }
     
    void Update()
    {
        CheckIfGhostIsVisibleToNPC();

       
        // If the NPC has been scared, make them run away from the scare and increase their fear meter
        if (isNPCScared)
        {
            MakeNPCScared();
        }
        // If the NPC can see the ghost within their sight range, execute some kind of running behaviour
        else if (isPlayerInSightRange && isGhostVisible)
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

    void MakeNPCScared()
    {
        // Run away from the position of the scare
        RunningAway(currentScarePosition);

        // Starts a cool down so it doesn't instantly go back into patrol mode
        StartCoroutine("StartScareCooldown");
        IncreaseFearMeter();

        isNPCScared = false; // Putting down this flag immediately ensures that MakeNPCScared() only gets called once per scare
        print("NPC is has been scared by object.");
        
    }

    /*
    void ResetScareCooldown()
    {
        isScareCooldownRunning = false;
        StopCoroutine(StartScareCooldown()); //Resets the scare cooldown
        isNPCScared = false;
    }
    */

    IEnumerator StartScareCooldown()
    {
        isScareCooldownRunning = true;
        yield return new WaitForSeconds (scareCooldown);
        isNPCScared = false;
        isScareCooldownRunning = false;
    }

    void IncreaseFearMeter()
    {

    }

    void CheckIfGhostIsVisibleToNPC()
    {
        // Checks if ghost player is in sight range
        // Rn is checking using pure position calculations, eventually can use line of sight calculations to see if the ghost is being blocked by objects
        float distanceFromPlayer = (transform.position - player.position).magnitude;
        isPlayerInSightRange = (distanceFromPlayer <= sightRange) ? true : false;
    }

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
        // Calculate random walkpoint in range
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        
        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);
       
        // Check if generated walk point is actually near enough to the NavMesh, aka if it's actually walkable
        if (NavMesh.SamplePosition(walkPoint, out NavMeshHit hit, maxNavMeshDist, NavMesh.AllAreas))
        {
            // Sets the walk point to the nearest point on the NavMesh;
            walkPoint = hit.position;
            isWalkPointSet = true;
        }
    }

    void RunningAway(Vector3 scarePosition)
    {
        print("NPC running away from player");

        agent.speed = runSpeed;

        Vector3 dirAwayFromScare = (transform.position - scarePosition).normalized;
        Vector3 pointAwayFromScare= transform.position + (dirAwayFromScare * runPointRange);

        // Check if generated walk point is actually near enough to the NavMesh, aka if it's actually walkable
        if (NavMesh.SamplePosition(pointAwayFromScare, out NavMeshHit hit, maxNavMeshDist, NavMesh.AllAreas))
        {
            // Sets the walk point to the nearest point on the NavMesh;
            walkPoint = hit.position;
            //isWalkPointSet = true;
        }

        agent.SetDestination(walkPoint);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, sightRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(walkPoint, 2f);
    }
}

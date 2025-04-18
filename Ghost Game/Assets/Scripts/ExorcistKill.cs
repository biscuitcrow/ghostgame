using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExorcistKill : MonoBehaviour
{

    private bool hasKilled = false;

    private void OnTriggerEnter(Collider other)
    {
        print("Collided with smth.");
        if (other.gameObject.tag == "Player" && !hasKilled)
        {
            hasKilled = true;
            transform.GetComponentInParent<NPCBehaviour>().ToggleStopNavMeshAgent(true);
            GameManager.Instance.ExorcistKilledGhost();
            print("Exorcist has killed the player");

            // Play Exorcist kill anim

            // Play player death anim
            VFXManager.Instance.InstantiateRemovalPS(other.gameObject.transform);

            // Player death SFX
            AudioManager.instance.Play("Ghost Caught");
            AudioManager.instance.Stop("Exorcist Suck");

        }
    }
}

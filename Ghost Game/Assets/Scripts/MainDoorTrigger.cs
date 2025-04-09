using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainDoorTrigger : MonoBehaviour
{
    /*
    [Header("Main Doors Animators")]
    [SerializeField] Animator mainDoorL;
    [SerializeField] Animator mainDoorR;
    */


    public void ToggleMainDoors(bool isDoorOpen)
    {
        GameObject[] doors = GameObject.FindGameObjectsWithTag("Main Door Trigger");
        foreach (GameObject door in doors)
        {
            door.GetComponent<Animator>().SetBool("isDoorOpen", isDoorOpen);
        }
        AudioManager.instance.Play("Door Slam");
    }

}

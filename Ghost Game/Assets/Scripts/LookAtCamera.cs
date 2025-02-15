using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    public Transform mainCamera;

    void Start()
    {
        mainCamera = GameObject.FindWithTag("MainCamera").transform;
    }

    void Update()
    {
        //transform.LookAt(mainCamera, Vector3.up);
        transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
    }
}

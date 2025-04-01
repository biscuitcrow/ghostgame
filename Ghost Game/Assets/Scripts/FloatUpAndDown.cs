using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatUpAndDown : MonoBehaviour
{
    private Vector3 originalPos;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float amplitude = 0.2f;


    void Start()
    {
        Transform transform = gameObject.transform;
        originalPos = transform.localPosition;
    }

    void Update()
    {
        transform.localPosition = new Vector3(originalPos.x, originalPos.y + amplitude * Mathf.Sin(Time.time * speed), originalPos.z);
    }
}

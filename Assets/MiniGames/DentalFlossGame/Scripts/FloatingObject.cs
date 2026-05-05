using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FloatingObject : MonoBehaviour
{
    public float speed = 2f;
    public float amplitude = 0.2f;
    private float localTime = 0f;
    private float yStart;

    void Start()
    {
        yStart = transform.position.y;
        localTime = 0f;
    }
    void Update()
    {
        localTime += Time.deltaTime;
        float newY = yStart + Mathf.Sin(localTime * speed) * amplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}

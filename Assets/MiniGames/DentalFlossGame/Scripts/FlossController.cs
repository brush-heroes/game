using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlossController : MonoBehaviour
{
    public float lerpSpeed = 10f;
    private Vector3 lastPosition;
    public bool isMovingUp { get; private set; }

    void Start() => lastPosition = transform.position;

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;

            transform.position = Vector3.Lerp(transform.position, mousePos, lerpSpeed * Time.deltaTime);

            isMovingUp = transform.position.y > lastPosition.y + 0.01f;
            lastPosition = transform.position;

            transform.localScale = new Vector3(mousePos.x > 0 ? 1 : -1, 1, 1);
        }
        else
        {
            isMovingUp = false;
        }
    }
}
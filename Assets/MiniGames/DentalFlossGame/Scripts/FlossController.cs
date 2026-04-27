using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlossController : MonoBehaviour
{
    public float lerpSpeed = 10f;
    private Vector3 lastPosition;
    public bool isMovingUp { get; private set; }

    private Vector3 offset;
    private bool isDragging = false;

    void Start() => lastPosition = transform.position;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;

            Collider2D hit = Physics2D.OverlapPoint(mousePos);
            if (hit != null && (hit.gameObject == gameObject || hit.transform.parent == transform))
            {
                isDragging = true;
                offset = transform.position - mousePos;
            }
        }

        if (Input.GetMouseButton(0) && isDragging)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;

            Vector3 targetPos = mousePos + offset;
            transform.position = Vector3.Lerp(transform.position, targetPos, lerpSpeed * Time.deltaTime);

            isMovingUp = transform.position.y > lastPosition.y + 0.01f;
            lastPosition = transform.position;

            transform.localScale = new Vector3(mousePos.x > 0 ? 0.775f : -0.775f, 0.8625f, 1f);
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            isMovingUp = false;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlossController : MonoBehaviour
{
    public float speed = 15f;
    public float lerpSpeed = 10f;

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;

            transform.position = Vector3.Lerp(transform.position, mousePos, lerpSpeed * Time.deltaTime);

            if (mousePos.x > 0)
            {
                transform.localScale = new Vector3(1, 1, 1);
            }
            else
            {
                transform.localScale = new Vector3(-1, 1, 1);
            }
        }
    }
}

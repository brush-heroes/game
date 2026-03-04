using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlossController : MonoBehaviour
{
    public float speed = 15f;
    public float lerpSpeed = 10f;

    void Update()
    {
        // Detectar entrada de mouse o touch (momentáneamente para probar en PC)
        if (Input.GetMouseButton(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0; // Mantener siempre en el plano 2D

            // Movimiento suave hacia la posición del mouse
            transform.position = Vector3.Lerp(transform.position, mousePos, lerpSpeed * Time.deltaTime);

            // Lógica de Mirror (Abrazo)
            // Si el mouse está a la derecha del centro, invertimos la escala en X
            if (mousePos.x > 0)
            {
                transform.localScale = new Vector3(-1, 1, 1);
            }
            else
            {
                transform.localScale = new Vector3(1, 1, 1);
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dirt : MonoBehaviour
{
    // Se ejecuta cuando algo entra en el Trigger 2D
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Verificamos si lo que nos tocó es la seda dental (Tag: Player)
        if (other.CompareTag("Player"))
        {
            // Aquí podrías llamar al ScoreSystem para sumar puntos
            // FindObjectOfType<ScoreSystem>().AddScore(10);

            Debug.Log("¡Suciedad limpia!");
            Destroy(gameObject); // Eliminamos la mancha
        }
    }
}

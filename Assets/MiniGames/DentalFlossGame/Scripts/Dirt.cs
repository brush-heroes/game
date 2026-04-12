using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dirt : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            FlossController floss = collision.GetComponentInParent<FlossController>();

            if (floss != null && floss.isMovingUp)
            {
                GameManager.Instance.AddScore(10);
                Debug.Log("Clean!");
                Destroy(gameObject);
            }
            else
            {
                Debug.Log("Wrong!");
            }
        }
    }
}
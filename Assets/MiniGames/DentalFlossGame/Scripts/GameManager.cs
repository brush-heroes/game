using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Debug.Log("Juego de Dental Floss iniciado");
    }

    public void AddScore(int amount)
    {
        ScoreSystem.Instance.AddScore(amount);
    }
}

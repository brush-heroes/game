using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeethProgression : MonoBehaviour
{
    public Sprite dirtyTeeth;
    public Sprite mediumTeeth;
    public Sprite cleanTeeth;

    public int mediumLimit = 50;
    public int cleanLimit = 100;

    public List<SpriteRenderer> teethRenderers;

    // Referencia al controlador de la seda
    private FlossController flossController;

    void Start()
    {
        flossController = FindObjectOfType<FlossController>();
        UpdateSprites(dirtyTeeth);
    }

    void Update()
    {
        int score = GameManager.Instance.totalScore;

        if (score >= cleanLimit)
        {
            UpdateSprites(cleanTeeth);
        }
        else if (score >= mediumLimit)
        {
            UpdateSprites(mediumTeeth);
        }
        else
        {
            UpdateSprites(dirtyTeeth);
        }
    }

    void UpdateSprites(Sprite newSprite)
    {
        foreach (SpriteRenderer renderer in teethRenderers)
        {
            if (renderer.sprite != newSprite)
            {
                renderer.sprite = newSprite;
                VoltearDiente(renderer.transform); // Llama a la lógica de volteo cada vez que cambia de estado
            }
        }
    }

    // Nueva lógica de volteo basada en la posición del aplicador
    void VoltearDiente(Transform targetTransform)
    {
        if (flossController != null)
        {
            // Comprobamos la dirección del aplicador en el eje X
            bool sedaDesdeIzquierda = flossController.transform.localScale.x < 0;

            // Ajustamos la escala del padre o del objeto mismo para girar el sprite
            Vector3 currentScale = targetTransform.parent.localScale;
            targetTransform.parent.localScale = new Vector3(sedaDesdeIzquierda ? -1f : 1f, currentScale.y, currentScale.z);
        }
    }
}
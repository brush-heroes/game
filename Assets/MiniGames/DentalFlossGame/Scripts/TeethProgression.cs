using System.Collections.Generic;
using UnityEngine;

public class TeethProgression : MonoBehaviour
{
    public Sprite dirtyTeeth;
    public Sprite mediumTeeth;
    public Sprite cleanTeeth;

    public int mediumLimit = 500;
    public int cleanLimit = 1000;

    public List<SpriteRenderer> teethRenderers;

    private Sprite currentActiveSprite;
    private bool estaMirandoDerecha = true;

    void Start()
    {
        currentActiveSprite = dirtyTeeth;
        UpdateSprites(currentActiveSprite);
    }

    void Update()
    {
        if (GameManager.Instance == null) return;

        int score = GameManager.Instance.totalScore;
        Sprite newSprite;

        if (score >= cleanLimit)
            newSprite = cleanTeeth;
        else if (score >= mediumLimit)
            newSprite = mediumTeeth;
        else
            newSprite = dirtyTeeth;

        if (newSprite != currentActiveSprite)
        {
            currentActiveSprite = newSprite;
            UpdateSprites(currentActiveSprite);
        }
    }

    void UpdateSprites(Sprite newSprite)
    {
        foreach (SpriteRenderer renderer in teethRenderers)
        {
            if (renderer != null)
            {
                renderer.sprite = newSprite;
            }
        }
    }

    public void FlipDientes(bool mirandoDerecha)
    {
        estaMirandoDerecha = mirandoDerecha;

        foreach (SpriteRenderer renderer in teethRenderers)
        {
            if (renderer != null)
            {
                Vector3 scale = renderer.transform.localScale;
                scale.x = mirandoDerecha ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
                renderer.transform.localScale = scale;
            }
        }
    }
}
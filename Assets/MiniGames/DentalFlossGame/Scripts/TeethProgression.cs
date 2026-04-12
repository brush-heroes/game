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

    void Start()
    {
        UpdateSprites(dirtyTeeth);
    }

    void Update()
    {
        int score = GameManager.Instance.totalScore;

        if(score >= cleanLimit)
        {
            UpdateSprites(cleanTeeth);
        }
        else if(score >= mediumLimit)
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
        foreach(SpriteRenderer renderer in teethRenderers)
        {
            if(renderer.sprite != newSprite)
            {
                renderer.sprite = newSprite;
            }
        }
    }

}

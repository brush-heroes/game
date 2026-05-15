using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TipManager : MonoBehaviour
{
    [TextArea]
    public string[] tips;
    public TextMeshProUGUI tipText;

    void Start()
    {
        MostrarTipAleatorio();
    }

    public void MostrarTipAleatorio()
    {
        if (tips != null && tips.Length > 0 && tipText != null)
        {
            int indiceAleatorio = Random.Range(0, tips.Length);
            tipText.text = tips[indiceAleatorio];
        }
    }
}
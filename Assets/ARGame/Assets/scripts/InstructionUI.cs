using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class InstructionUI : MonoBehaviour
{
    [SerializeField] SessionManager sessionManager;

    [FormerlySerializedAs("instructionTextTMP")]
    [SerializeField] TMP_Text instructionText;
    [SerializeField] Text instructionTextLegacy;

    public void RefreshInstruction()
    {
        if (sessionManager == null)
        {
            Debug.LogError("SessionManager not assigned!");
            return;
        }

        string text = sessionManager.CurrentInstructionText;

        Debug.Log("[InstructionUI] Refreshing: " + text);

        if (string.IsNullOrEmpty(text))
        {
            Debug.LogWarning("Instruction is empty!");
            return;
        }

        if (instructionText != null)
            instructionText.text = text;
        else if (instructionTextLegacy != null)
            instructionTextLegacy.text = text;
    }
}

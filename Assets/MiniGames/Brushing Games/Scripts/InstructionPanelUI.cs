using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InstructionPanelUI : MonoBehaviour
{
    [Header("Panel")]
    public GameObject panelRoot;

    [Header("Text")]
    public TMP_Text instructionTextTMP;
    public Text instructionTextLegacy;

    [Header("Continue Button")]
    public Button continueButton;

    private Action currentOnContinue;

    private void Awake()
    {
        if (panelRoot == null)
            panelRoot = gameObject;

        if (continueButton != null)
            continueButton.onClick.AddListener(HandleContinueClicked);
    }

    public void Show(string message, Action onContinue)
    {
        currentOnContinue = onContinue;

        if (instructionTextTMP != null)
            instructionTextTMP.text = message;
        else if (instructionTextLegacy != null)
            instructionTextLegacy.text = message;

        if (panelRoot != null)
            panelRoot.SetActive(true);
    }

    public void Hide()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    private void HandleContinueClicked()
    {
        Action cb = currentOnContinue;
        currentOnContinue = null;
        Hide();
        cb?.Invoke();
    }
}


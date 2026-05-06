using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CameraTestInput : MonoBehaviour
{
    public CameraTransitionController cameraController;

    public StageTimer stageTimer;

    public BrushController brush;
    public TongueSwipeCleaningManager tongueSwipeCleaningManager;
    public TongueGameManager tongueGameManager;
    public Button startButton;
    public GameObject brushOff;
    public GameObject mouth;
    public GameObject ninoSucio;
    [Header("Instruction Panel")]
    public InstructionPanelUI instructionPanel;
    private bool tongueSequenceStarted;
    private bool gameStarted;

    private void Start()
    {
        ResolveStartButtonIfNeeded();

        if (instructionPanel != null)
            instructionPanel.Hide();

        if (brush != null)
            brush.SetFollowEnabled(false);

        if (BrushGameManager.Instance != null)
        {
            BrushGameManager.Instance.OutsideRightCompleted += HandleOutsideRightCompleted;
            BrushGameManager.Instance.OutsideLeftCompleted += HandleOutsideLeftCompleted;
            BrushGameManager.Instance.RightSideCompleted += HandleRightSideCompleted;
            BrushGameManager.Instance.LeftSideCompleted += HandleLeftSideCompleted;
        }

        if (stageTimer != null)
        {
            stageTimer.RightSequenceCompleted += HandleRightSequenceCompleted;
            stageTimer.LeftSequenceCompleted += HandleLeftSequenceCompleted;
        }

        if (tongueSwipeCleaningManager != null)
            tongueSwipeCleaningManager.CleaningCompleted += HandleTongueSwipeCompleted;

        if (startButton != null)
            startButton.onClick.AddListener(OnStartButtonPressed);
    }

    private void OnDestroy()
    {
        if (BrushGameManager.Instance != null)
        {
            BrushGameManager.Instance.OutsideRightCompleted -= HandleOutsideRightCompleted;
            BrushGameManager.Instance.OutsideLeftCompleted -= HandleOutsideLeftCompleted;
            BrushGameManager.Instance.RightSideCompleted -= HandleRightSideCompleted;
            BrushGameManager.Instance.LeftSideCompleted -= HandleLeftSideCompleted;
        }

        if (stageTimer != null)
        {
            stageTimer.RightSequenceCompleted -= HandleRightSequenceCompleted;
            stageTimer.LeftSequenceCompleted -= HandleLeftSequenceCompleted;
        }

        if (tongueSwipeCleaningManager != null)
            tongueSwipeCleaningManager.CleaningCompleted -= HandleTongueSwipeCompleted;

        if (startButton != null)
            startButton.onClick.RemoveListener(OnStartButtonPressed);
    }

    public void OnStartButtonPressed()
    {
        if (gameStarted)
            return;

        gameStarted = true;
        if (startButton != null)
            startButton.gameObject.SetActive(false);

        ShowInstructionStartGame();
    }

    private void ShowInstructionStartGame()
    {
        if (instructionPanel == null)
        {
            BeginFirstChewingInstruction();
            return;
        }

        instructionPanel.Show(
            "Usa el cepillo para limpiar la suciedad que se ve encima de los dientes.\n" +
            "Limpia en sentido de derecha a izquierda, siguiendo la forma de arco.",
            BeginFirstChewingInstruction
        );
    }

    private void BeginFirstChewingInstruction()
    {
        cameraController.GoToNormalView();

        if (brush != null)
        {
            brush.SetFollowEnabled(true);
            brush.SetStartPose();
            brush.SetZoomMode(false);
        }

        if (ninoSucio != null)
            ninoSucio.SetActive(false);

        if (BrushGameManager.Instance != null)
            BrushGameManager.Instance.StartFromChewingRight();
    }

    private void ResolveStartButtonIfNeeded()
    {
        if (startButton != null)
            return;

        GameObject buttonObject = GameObject.Find("StartButton");
        if (buttonObject == null)
            buttonObject = GameObject.Find("startButton");

        if (buttonObject != null)
            startButton = buttonObject.GetComponent<Button>();

        if (startButton == null)
        {
            Debug.LogWarning("CameraTestInput: No se encontro 'StartButton'. Asignalo en el inspector.");
        }
    }

    private void HandleRightSideCompleted()
    {
        ShowInstructionOutsideRight(
            () => StartCoroutine(StartRightWithDelay())
        );
    }

    private void HandleOutsideRightCompleted()
    {
        if (brush != null)
            brush.MirrorDirection();
    }

    private void HandleLeftSideCompleted()
    {
        StartCoroutine(StartLeftWithDelay());
    }

    private void HandleOutsideLeftCompleted()
    {
        if (brush != null)
            brush.MirrorDirection();
    }

    private void HandleRightSequenceCompleted()
    {
        ReturnToNormalView();
        if (brush != null)
            brush.RestoreSavedPose();
    }

    private void HandleLeftSequenceCompleted()
    {
        if (tongueSequenceStarted)
            return;

        tongueSequenceStarted = true;
        StartCoroutine(ReturnThenShowTongueInstruction());
    }

    private void ReturnToNormalView()
    {
        cameraController.GoToNormalView();
        brush.SetZoomMode(false);
    }

    private IEnumerator ReturnThenShowTongueInstruction()
    {
        ReturnToNormalView();
        yield return new WaitForSeconds(1.1f);

        bool continuePressed = false;

        if (instructionPanel == null)
        {
            continuePressed = true;
        }
        else
        {
            instructionPanel.Show(
                "Lengua:\n" +
                "1) Haz el movimiento de limpieza de adentro hacia afuera.\n" +
                "2) Luego elimina las suciedades y bacterias.",
                () => continuePressed = true
            );
        }

        while (!continuePressed)
            yield return null;

        yield return StartCoroutine(StartTongueWithDelay());
    }

    private void ShowInstructionOutsideRight(Action onContinue)
    {
        if (instructionPanel == null)
        {
            onContinue?.Invoke();
            return;
        }

        instructionPanel.Show(
            "Minijuego bacteria \n" +
            "Haz movimientos circulares horizontales con el cepillo para eliminar las bacterias.",
            onContinue
        );
    }

    private IEnumerator StartRightWithDelay()
    {
        if (brush != null)
            brush.SetOutsideRightMinigamePose();

        cameraController.GoToOutsideZoomView();

        yield return new WaitForSeconds(1.1f);

        brush.SetZoomMode(true);

        if (stageTimer != null)
        {
            stageTimer.StartRightSequence();
        }
    }

    private IEnumerator StartLeftWithDelay()
    {
        if (brush != null)
            brush.SetOutsideLeftMinigamePose();

        cameraController.GoToLeftOutsideZoomView();

        yield return new WaitForSeconds(1.1f);

        brush.SetZoomMode(true);

        if (stageTimer != null)
        {
            stageTimer.StartLeftSequence();
        }
    }

    private IEnumerator StartTongueWithDelay()
    {
        cameraController.GoToTongueZoomView();

        yield return new WaitForSeconds(1.1f);

        if (tongueSwipeCleaningManager != null)
        {
            tongueSwipeCleaningManager.StartCleaningMechanic();
            yield break;
        }

        if (brushOff != null)
            brushOff.SetActive(false);

        if (tongueGameManager != null)
            tongueGameManager.StartTongueGame();
    }

    private void HandleTongueSwipeCompleted()
    {
        if (tongueSwipeCleaningManager != null)
            tongueSwipeCleaningManager.ClearSpawnedDirt();

        if (brushOff != null)
            brushOff.SetActive(false);

        if (tongueGameManager != null)
            tongueGameManager.StartTongueGame();
    }

}
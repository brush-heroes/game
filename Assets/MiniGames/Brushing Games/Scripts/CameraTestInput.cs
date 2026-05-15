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

    [Header("Final del juego")]
    [Tooltip("Objeto raíz TongueStage de la jerarquía. Si queda vacío, se busca por nombre al iniciar.")]
    public GameObject tongueStage;
    [Tooltip("Sprite del niño limpio (score > umbral). Arrastra tu PNG aquí.")]
    public Sprite ninoCleanSprite;
    [Tooltip("Puntos mínimos para mostrar al niño limpio (debe superar este valor).")]
    public int cleanChildScoreThreshold = 1400;

    [Header("Textos de instrucciones")]
    [TextArea(2, 3)]
    public string startInstructionLine1 =
        "Usa el cepillo para limpiar la suciedad que se ve encima de los dientes.";
    [TextArea(2, 3)]
    public string startInstructionLine2 =
        "Limpia en el sentido de las flechas, siguiendo la forma de arco.";

    [Header("Transición final")]
    [Tooltip("Escala inicial del niño al aparecer (1 = tamaño normal).")]
    [Range(0.5f, 1f)]
    public float finaleChildStartScale = 0.88f;

    [Header("Instruction Panel")]
    public InstructionPanelUI instructionPanel;

    private bool tongueSequenceStarted;
    private bool gameStarted;
    private bool gameFinished;
    private SpriteRenderer ninoRenderer;
    private Sprite ninoDirtySprite;
    private Vector3 ninoBaseLocalScale = Vector3.one;

    private void Start()
    {
        ResolveStartButtonIfNeeded();
        ResolveFinaleReferencesIfNeeded();

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

        if (tongueGameManager != null)
            tongueGameManager.TongueGameCompleted += HandleAllMinigamesCompleted;

        if (ninoSucio != null)
        {
            ninoBaseLocalScale = ninoSucio.transform.localScale;
            ninoRenderer = ninoSucio.GetComponent<SpriteRenderer>();
            if (ninoRenderer != null)
                ninoDirtySprite = ninoRenderer.sprite;
        }

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

        if (tongueGameManager != null)
            tongueGameManager.TongueGameCompleted -= HandleAllMinigamesCompleted;

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
            startInstructionLine1 + "\n" + startInstructionLine2,
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

    private void ResolveFinaleReferencesIfNeeded()
    {
        if (tongueStage == null)
        {
            GameObject found = GameObject.Find("TongueStage");
            if (found != null)
                tongueStage = found;
        }

        if (ninoSucio != null && ninoRenderer == null)
        {
            ninoRenderer = ninoSucio.GetComponent<SpriteRenderer>();
            if (ninoRenderer != null && ninoDirtySprite == null)
                ninoDirtySprite = ninoRenderer.sprite;
        }

        if (ninoCleanSprite == null)
        {
            Debug.LogWarning(
                "CameraTestInput: Asigna 'Nino Clean Sprite' en BrushGameManager > Final del juego " +
                "(sprite del niño limpio). Si no, siempre se usará el niño sucio.");
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

        if (brush != null)
        {
            brush.SetZoomMode(true);
            brush.RefreshLateralPoseAfterZoom();
        }

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

        if (brush != null)
        {
            brush.SetZoomMode(true);
            brush.RefreshLateralPoseAfterZoom();
        }

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

    private void HandleAllMinigamesCompleted()
    {
        if (gameFinished)
            return;

        gameFinished = true;
        StartCoroutine(FinishGameSequence());
    }

    private IEnumerator FinishGameSequence()
    {
        if (instructionPanel != null)
            instructionPanel.Hide();

        if (brush != null)
        {
            brush.SetFollowEnabled(false);
            brush.SetZoomMode(false);
        }

        if (brushOff != null)
            brushOff.SetActive(false);

        if (mouth != null)
            mouth.SetActive(false);

        if (tongueStage != null)
            tongueStage.SetActive(false);

        if (stageTimer != null)
            stageTimer.StopTimer();

        PrepareFinalChildHidden();

        if (cameraController != null)
        {
            yield return cameraController.CoGoToInitialStartView(ApplyFinaleChildReveal);
        }
        else
        {
            ApplyFinaleChildReveal(1f);
        }

        ApplyFinaleChildReveal(1f);
    }

    private void PrepareFinalChildHidden()
    {
        if (ninoSucio == null)
            return;

        int score = BrushingScoreManager.Instance != null
            ? BrushingScoreManager.Instance.CurrentScore
            : 0;

        bool showCleanChild = score > cleanChildScoreThreshold;

        if (ninoRenderer != null)
        {
            if (showCleanChild && ninoCleanSprite != null)
                ninoRenderer.sprite = ninoCleanSprite;
            else if (ninoDirtySprite != null)
                ninoRenderer.sprite = ninoDirtySprite;
        }

        ninoSucio.SetActive(true);
        ApplyFinaleChildReveal(0f);
    }

    private void ApplyFinaleChildReveal(float progress)
    {
        if (ninoSucio == null)
            return;

        float eased = Mathf.SmoothStep(0f, 1f, progress);
        float scaleFactor = Mathf.Lerp(finaleChildStartScale, 1f, eased);
        ninoSucio.transform.localScale = ninoBaseLocalScale * scaleFactor;

        if (ninoRenderer != null)
        {
            Color color = ninoRenderer.color;
            color.a = eased;
            ninoRenderer.color = color;
        }
    }

}
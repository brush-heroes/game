using UnityEngine;
using UnityEngine.EventSystems;

public class FlossController : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    public float lerpSpeed = 20f;
    public float minY = -0.33f;
    public float maxY = 1.66f;

    public float rightMinX = 2.10f;
    public float rightMaxX = 4.40f;
    public float leftMinX = -4.40f;
    public float leftMaxX = -2.10f;

    [Range(0.1f, 0.5f)]
    public float margenJaladoX = 0.2f;

    [Header("Referencias")]
    public GameObject curvedFlossObj;
    public SpriteRenderer normalFlossRenderer;

    [Header("Puntuación")]
    public int pointsPerSecondCorrect = 15;
    public int pointsPerSecondCurve = 40;
    public int bonusOnComplete = 25;

    private readonly Vector3 escalaInicialSeda = new Vector3(-0.6567135f, 0.6286655f, 0.4942825f);
    private readonly Vector3 posicionInicialSedaDerecha = new Vector3(3.15f, 1.66f, 0f);
    private readonly Vector3 posicionInicialSedaIzquierda = new Vector3(-3.15f, 1.66f, 0f);
    private readonly Vector3 escalaCurva = new Vector3(0.948254f, 0.9928277f, 0.4942825f);
    private readonly Vector3 posicionCurva = new Vector3(0.071f, -0.198f, 0f);

    private bool isDragging = false;
    private bool enModoCurva = false;
    private bool estaEnLadoDerecho = true;
    private float targetX;
    private Vector3 lastPosition;
    private float currentMinX, currentMaxX;
    private float currentMinXJalado, currentMaxXJalado;
    private float contadorPuntaje = 0f;
    private bool puedeSerClickeado = true;

    void Start()
    {
        transform.position = posicionInicialSedaDerecha;
        transform.localScale = escalaInicialSeda;
        estaEnLadoDerecho = true;

        ActualizarLimitesSegunLado();

        if (curvedFlossObj != null)
        {
            curvedFlossObj.transform.localPosition = posicionCurva;
            curvedFlossObj.transform.localScale = escalaCurva;
            curvedFlossObj.SetActive(false);
        }
        targetX = transform.position.x;
        lastPosition = transform.position;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && puedeSerClickeado && GameManager.Instance != null && GameManager.Instance.isGameActive)
        {
            if (!IsPointerOverUI())
            {
                isDragging = true;
                EvaluacionInicio();
                contadorPuntaje = 0f;
            }
        }

        if (isDragging && GameManager.Instance != null && GameManager.Instance.isGameActive)
        {
            PerformBoundedMovement();
            EvaluateFlossingQuality();
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;

            if (!enModoCurva)
            {
                EvaluacionSalida();
            }
            else
            {
                contadorPuntaje = 0f;
                Debug.Log("Seda curva pausada - continúa presionando para seguir subiendo");
            }
        }

        if (transform.position.y >= maxY - 0.05f && enModoCurva)
        {
            enModoCurva = false;
            SetCurvedFloss(false);
            CambiarDeLado();

            GameManager.Instance.AddScore(bonusOnComplete);
        }
    }

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;

        if (EventSystem.current.IsPointerOverGameObject())
        {
            return true;
        }

        return false;
    }

    public void DesactivarClicks()
    {
        puedeSerClickeado = false;
    }

    void ActualizarLimitesSegunLado()
    {
        if (estaEnLadoDerecho)
        {
            currentMinX = rightMinX;
            currentMaxX = rightMaxX;
            currentMinXJalado = rightMinX - margenJaladoX;
            currentMaxXJalado = rightMaxX + margenJaladoX;
        }
        else
        {
            currentMinX = leftMinX;
            currentMaxX = leftMaxX;
            currentMinXJalado = leftMinX - margenJaladoX;
            currentMaxXJalado = leftMaxX + margenJaladoX;
        }
    }

    void PerformBoundedMovement()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;

        float targetY = Mathf.Clamp(mousePos.y, minY, maxY);

        if (transform.position.y <= minY + 0.1f)
            targetX = Mathf.Clamp(mousePos.x, currentMinXJalado, currentMaxXJalado);
        else
            targetX = Mathf.Clamp(mousePos.x, currentMinX, currentMaxX);

        Vector3 targetPos = new Vector3(targetX, targetY, 0f);
        float smoothedY = Mathf.Lerp(transform.position.y, targetPos.y, lerpSpeed * Time.deltaTime);
        float finalX = Mathf.Lerp(transform.position.x, targetX, lerpSpeed * Time.deltaTime);

        transform.position = new Vector3(finalX, smoothedY, 0f);
        lastPosition = transform.position;
    }

    void EvaluateFlossingQuality()
    {
        if (enModoCurva)
        {
            SetCurvedFloss(true);

            if (isDragging)
            {
                contadorPuntaje += Time.deltaTime;
                if (contadorPuntaje >= 0.5f)
                {
                    GameManager.Instance.AddScore(pointsPerSecondCurve);
                    contadorPuntaje = 0f;
                    Debug.Log("Modo curva: +" + pointsPerSecondCurve + " puntos");
                }
            }
            return;
        }

        bool estaAbajo = transform.position.y <= minY + 0.05f;

        if (estaAbajo && isDragging)
        {
            bool jalandoHaciaDiente = estaEnLadoDerecho ? transform.position.x > currentMaxX : transform.position.x < currentMinX;

            if (jalandoHaciaDiente)
            {
                enModoCurva = true;
                SetCurvedFloss(true);
                GameManager.Instance.AddScore(pointsPerSecondCurve / 2);
                Debug.Log("Modo curva activado. Sube la seda hasta arriba sin soltar");
                contadorPuntaje = 0f;
                return;
            }
        }

        if (transform.position.y < maxY && isDragging)
        {
            contadorPuntaje += Time.deltaTime;
            if (contadorPuntaje >= 0.5f)
            {
                GameManager.Instance.AddScore(pointsPerSecondCorrect);
                contadorPuntaje = 0f;
            }
        }
    }

    void SetCurvedFloss(bool active)
    {
        if (curvedFlossObj != null) curvedFlossObj.SetActive(active);
        if (normalFlossRenderer != null) normalFlossRenderer.enabled = !active;
    }

    void CambiarDeLado()
    {
        estaEnLadoDerecho = !estaEnLadoDerecho;
        ActualizarLimitesSegunLado();

        float absoluteX = Mathf.Abs(escalaInicialSeda.x);
        transform.localScale = new Vector3(estaEnLadoDerecho ? -absoluteX : absoluteX, escalaInicialSeda.y, escalaInicialSeda.z);

        Vector3 nuevaPos = estaEnLadoDerecho ? posicionInicialSedaDerecha : posicionInicialSedaIzquierda;
        transform.position = nuevaPos;
        targetX = nuevaPos.x;
        lastPosition = transform.position;

        TeethProgression teeth = FindObjectOfType<TeethProgression>();
        if (teeth != null)
        {
            teeth.FlipDientes(estaEnLadoDerecho);
        }

        Debug.Log(estaEnLadoDerecho ? "Cambiado a lado DERECHO" : "Cambiado a lado IZQUIERDO");
    }

    void EvaluacionInicio()
    {
        Debug.Log("Comenzó movimiento de seda");
    }

    void EvaluacionSalida()
    {
        Debug.Log("Movimiento normal terminado");
    }
}
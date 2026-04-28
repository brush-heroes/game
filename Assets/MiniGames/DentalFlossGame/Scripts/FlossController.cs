using UnityEngine;

public class FlossController : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    public float lerpSpeed = 15f;
    public float thresholdSerrucho = 0.05f; // Sensibilidad para detectar el zigzag

    [Header("Referencias de Sprites")]
    public Sprite spriteRecto;
    public Sprite spriteFormaC;
    private SpriteRenderer sRenderer;

    [Header("Mecánica de Limpieza")]
    public TeethProgression progressionScript; // Referencia al script que cambia los dientes
    public GameObject maskDerecha;
    public GameObject maskIzquierda;

    private Vector3 lastPosition;
    private Vector3 offset;
    private bool isDragging = false;
    private bool estaEnContactoConDiente = false;

    void Start()
    {
        lastPosition = transform.position;
        sRenderer = GetComponentInChildren<SpriteRenderer>();
        sRenderer.sprite = spriteRecto;
    }

    void Update()
    {
        HandleInput();
        if (isDragging)
        {
            PerformMovement();
            CheckTechnicalQuality();
        }
    }

    void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = GetMouseWorldPos();
            Collider2D hit = Physics2D.OverlapPoint(mousePos);
            if (hit != null && (hit.gameObject == gameObject || hit.transform.parent == transform))
            {
                isDragging = true;
                offset = transform.position - mousePos;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            sRenderer.sprite = spriteRecto; // Al soltar, vuelve a forma recta
        }
    }

    void PerformMovement()
    {
        Vector3 mousePos = GetMouseWorldPos();
        Vector3 targetPos = mousePos + offset;
        transform.position = Vector3.Lerp(transform.position, targetPos, lerpSpeed * Time.deltaTime);

        // Lógica de FLIP y MÁSCARAS
        // Si X es positivo (derecha), usamos escala positiva y máscara derecha
        bool mirandoderecha = transform.position.x > 0;
        transform.localScale = new Vector3(mirandoderecha ? 0.775f : -0.775f, 0.8625f, 1f);

        // Activa la máscara según hacia donde mire el aplicador
        maskDerecha.SetActive(mirandoderecha);
        maskIzquierda.SetActive(!mirandoderecha);
    }

    void CheckTechnicalQuality()
    {
        // 1. Detectar si estamos haciendo "Serrucho" (movimiento lateral rápido)
        float deltaX = Mathf.Abs(transform.position.x - lastPosition.x);

        // 2. Detectar si estamos abrazando el diente (Forma en C)
        // Cambiamos el sprite si estamos cerca del centro y el jugador inclina el aplicador
        if (estaEnContactoConDiente)
        {
            sRenderer.sprite = spriteFormaC; //

            // 3. Sumar progreso si la técnica es correcta
            if (deltaX > thresholdSerrucho)
            {
                progressionScript.SumarProgreso(Time.deltaTime * 10f); // Limpieza gradual
            }
        }
        else
        {
            sRenderer.sprite = spriteRecto;
        }

        lastPosition = transform.position;
    }

    Vector3 GetMouseWorldPos()
    {
        Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        pos.z = 0;
        return pos;
    }

    // Usamos los colisionadores verdes que dibujaste
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Diente")) estaEnContactoConDiente = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Diente")) estaEnContactoConDiente = false;
    }
}
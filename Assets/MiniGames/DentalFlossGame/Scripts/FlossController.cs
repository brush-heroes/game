using UnityEngine;

public class FlossController : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    public float lerpSpeed = 15f;
    public float thresholdSerrucho = 0.05f;

    [Header("Referencias de la Seda")]
    public GameObject curvedFlossObj;

    [Header("Referencia de la Máscara")]
    public GameObject maskTeeth;

    private Vector3 lastPosition;
    private Vector3 offset;
    private bool isDragging = false;
    private bool estaEnContactoConDiente = false;

    void Start()
    {
        lastPosition = transform.position;
        if (curvedFlossObj != null) curvedFlossObj.SetActive(false);
    }

    void Update()
    {
        if (isDragging)
        {
            PerformMovement();
            CheckTechnicalQuality();
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            if (curvedFlossObj != null) curvedFlossObj.SetActive(false);
        }
    }

    private void OnMouseDown()
    {
        Vector3 mousePos = GetMouseWorldPos();
        isDragging = true;
        offset = transform.position - mousePos;
    }

    void PerformMovement()
    {
        Vector3 mousePos = GetMouseWorldPos();
        Vector3 targetPos = mousePos + offset;

        // Movimiento suave hacia la posición objetivo
        transform.position = Vector3.Lerp(transform.position, targetPos, lerpSpeed * Time.deltaTime);

        // Determinamos la dirección basándonos en el cambio de posición (hacia dónde se mueve)
        float movementX = transform.position.x - lastPosition.x;

        // Solo actualizamos el flip si hay un movimiento significativo en X
        if (Mathf.Abs(movementX) > 0.01f)
        {
            bool mirandoDerecha = movementX > 0;
            // Cambia la escala para voltear el aplicador conservando sus proporciones
            transform.localScale = new Vector3(mirandoDerecha ? 0.775f : -0.775f, 0.8625f, 1f);
        }

        lastPosition = transform.position;
    }

    void CheckTechnicalQuality()
    {
        float deltaX = Mathf.Abs(transform.position.x - lastPosition.x);

        if (estaEnContactoConDiente)
        {
            if (curvedFlossObj != null) curvedFlossObj.SetActive(true);

            if (deltaX > thresholdSerrucho)
            {
                GameManager.Instance.totalScore += Mathf.RoundToInt(Time.deltaTime * 20f);
            }
        }
        else
        {
            if (curvedFlossObj != null) curvedFlossObj.SetActive(false);
        }
    }

    Vector3 GetMouseWorldPos()
    {
        Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        pos.z = 0;
        return pos;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Diente"))
        {
            estaEnContactoConDiente = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Diente"))
        {
            estaEnContactoConDiente = false;
        }
    }
}
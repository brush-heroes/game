using UnityEngine;

public class FlossController : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    public float moveSpeed = 15f;
    public float thresholdSerrucho = 0.05f;

    [Header("Referencias de la Seda")]
    public GameObject curvedFlossObj;

    private Rigidbody2D rb;
    private Vector3 offset;
    private bool isDragging = false;
    private bool estaEnEspacioInterdental = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Configuramos el Rigidbody de forma segura y compatible
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0;

        if (curvedFlossObj != null) curvedFlossObj.SetActive(false);
    }

    void Update()
    {
        if (isDragging)
        {
            MoveWithPhysics();
            CheckTechnicalQuality();
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            rb.velocity = Vector2.zero; // Corregido: usando velocity estándar
            if (curvedFlossObj != null) curvedFlossObj.SetActive(false);
        }
    }

    private void OnMouseDown()
    {
        Vector3 mousePos = GetMouseWorldPos();
        isDragging = true;
        offset = transform.position - mousePos;

        // Corregido el flip para que mire hacia el mouse correctamente
        bool clickIzquierda = mousePos.x > 0;
        transform.localScale = new Vector3(clickIzquierda ? 0.775f : -0.775f, 0.8625f, 1f);
    }

    void MoveWithPhysics()
    {
        Vector3 mousePos = GetMouseWorldPos();
        Vector3 targetPos = mousePos + offset;

        Vector2 force = (targetPos - transform.position) * moveSpeed;
        rb.velocity = force; // Corregido: usando velocity estándar
    }

    void CheckTechnicalQuality()
    {
        if (estaEnEspacioInterdental)
        {
            if (curvedFlossObj != null) curvedFlossObj.SetActive(true);

            float deltaX = Mathf.Abs(rb.velocity.x); // Corregido: usando velocity estándar
            if (deltaX > thresholdSerrucho)
            {
                GameManager.Instance.totalScore += Mathf.RoundToInt(Time.deltaTime * 30f);
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
        if (collision.CompareTag("Interdental"))
        {
            estaEnEspacioInterdental = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Interdental"))
        {
            estaEnEspacioInterdental = false;
        }
    }
}
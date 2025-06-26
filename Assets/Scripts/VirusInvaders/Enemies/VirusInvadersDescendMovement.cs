using UnityEngine;

public class VirusInvadersDescendMovement : MonoBehaviour, IVirusInvadersMovement
{
    [Header("Configuración de Movimiento de Descenso")]
    public float descendSpeed = 2f;
    public float horizontalOscillation = 0.3f;
    public float oscillationFrequency = 1.5f;
    
    private Vector3 startPosition;
    private float timeOffset;
    private Rigidbody2D rb;
    
    public void Initialize()
    {
        rb = GetComponent<Rigidbody2D>();
        startPosition = transform.position;
        timeOffset = Random.Range(0f, 2f * Mathf.PI);
    }
    
    void Start()
    {
        Initialize();
    }
    
    public void UpdateMovement(Transform target)
    {
        if (rb == null) return;
        
        // Descender con oscilación horizontal
        float horizontalOffset = Mathf.Sin(Time.time * oscillationFrequency + timeOffset) * horizontalOscillation;
        
        Vector2 targetPosition = new Vector2(
            startPosition.x + horizontalOffset,
            transform.position.y
        );
        
        Vector2 horizontalDirection = (targetPosition - (Vector2)transform.position).normalized;
        
        rb.linearVelocity = new Vector2(
            horizontalDirection.x * descendSpeed * 0.5f,
            -descendSpeed
        );
    }
    
    public void SetMovementSpeed(float speed)
    {
        descendSpeed = speed;
    }
    
    public void SetMovementParameters(params float[] parameters)
    {
        if (parameters.Length > 0) descendSpeed = parameters[0];
        if (parameters.Length > 1) horizontalOscillation = parameters[1];
        if (parameters.Length > 2) oscillationFrequency = parameters[2];
    }
}

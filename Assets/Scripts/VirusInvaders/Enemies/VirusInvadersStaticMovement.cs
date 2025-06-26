using UnityEngine;

public class VirusInvadersStaticMovement : MonoBehaviour, IVirusInvadersMovement
{
    [Header("Configuración de Movimiento Estático")]
    public float oscillationAmplitude = 0.5f;
    public float oscillationSpeed = 2f;
    public float descendSpeed = 1f; // Para mantener compatibilidad
    
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
        
        // Movimiento estático con oscilación + descenso lento
        float oscillation = Mathf.Sin(Time.time * oscillationSpeed + timeOffset) * oscillationAmplitude;
        
        Vector2 newVelocity = new Vector2(
            (oscillation - (transform.position.x - startPosition.x)) * 2f,
            -descendSpeed
        );
        
        rb.linearVelocity = newVelocity;
    }
    
    public void SetMovementSpeed(float speed)
    {
        descendSpeed = speed;
    }
    
    public void SetMovementParameters(params float[] parameters)
    {
        if (parameters.Length > 0) oscillationAmplitude = parameters[0];
        if (parameters.Length > 1) oscillationSpeed = parameters[1];
        if (parameters.Length > 2) descendSpeed = parameters[2];
    }
}

using UnityEngine;

public class VirusInvadersChaseMovement : MonoBehaviour, IVirusInvadersMovement
{
    [Header("Configuración de Movimiento de Persecución")]
    public float chaseSpeed = 3f;
    public float detectionRange = 5f;
    public float stoppingDistance = 1.5f;
    public bool useSmoothing = true;
    public float smoothingFactor = 2f;
    
    private Rigidbody2D rb;
    
    public void Initialize()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    
    void Start()
    {
        Initialize();
    }
    
    public void UpdateMovement(Transform target)
    {
        if (rb == null || target == null) return;
        
        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        
        // Perseguir si está dentro del rango de detección y no demasiado cerca
        if (distanceToTarget <= detectionRange && distanceToTarget > stoppingDistance)
        {
            Vector2 direction = (target.position - transform.position).normalized;
            Vector2 targetVelocity = direction * chaseSpeed;
            
            if (useSmoothing)
            {
                rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, targetVelocity, smoothingFactor * Time.deltaTime);
            }
            else
            {
                rb.linearVelocity = targetVelocity;
            }
        }
        else if (distanceToTarget <= stoppingDistance)
        {
            // Detenerse cuando esté demasiado cerca
            rb.linearVelocity = Vector2.zero;
        }
        else
        {
            // Descenso lento cuando el objetivo está lejos
            rb.linearVelocity = new Vector2(0, -chaseSpeed * 0.3f);
        }
    }
    
    public void SetMovementSpeed(float speed)
    {
        chaseSpeed = speed;
    }
    
    public void SetMovementParameters(params float[] parameters)
    {
        if (parameters.Length > 0) chaseSpeed = parameters[0];
        if (parameters.Length > 1) detectionRange = parameters[1];
        if (parameters.Length > 2) stoppingDistance = parameters[2];
        if (parameters.Length > 3) smoothingFactor = parameters[3];
    }
}

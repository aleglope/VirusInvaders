using UnityEngine;

public class VirusInvadersMovementController : MonoBehaviour
{
    private IVirusInvadersMovement movementComponent;
    private Transform target;
    private bool isGamePaused = false;
    
    public void Initialize(IVirusInvadersMovement movement, Transform playerTarget)
    {
        movementComponent = movement;
        target = playerTarget;
        
        // Suscribirse a eventos de pausa
        VirusInvadersGameManager.OnGamePaused += OnGamePaused;
    }
    
    void OnDestroy()
    {
        // Desuscribirse de eventos
        VirusInvadersGameManager.OnGamePaused -= OnGamePaused;
    }
    
    void OnGamePaused(bool paused)
    {
        isGamePaused = paused;
    }
    
    void Update()
    {
        if (movementComponent != null && !isGamePaused)
        {
            movementComponent.UpdateMovement(target);
        }
    }
} 
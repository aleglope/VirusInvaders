using UnityEngine;

public interface IVirusInvadersMovement
{
    void UpdateMovement(Transform target);
    void SetMovementSpeed(float speed);
    void SetMovementParameters(params float[] parameters);
    void Initialize();
} 
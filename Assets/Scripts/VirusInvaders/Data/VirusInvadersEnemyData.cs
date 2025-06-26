using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Data", menuName = "VirusInvaders/Enemy Data")]
public class VirusInvadersEnemyData : ScriptableObject
{
    [Header("Basic Configuration")]
    public string enemyName;
    public float health = 100f;
    public float attackDamage = 25f;
    public float moveSpeed = 2f;
    public int pointsValue = 10;
    
    [Header("Visual Configuration")]
    public VirusType virusType = VirusType.Classic;
    public Color enemyColor = Color.white;
    public float scale = 1.0f;
    public string sortingLayerName = "Characters";
    public int sortingOrder = 10;
    
    [Header("Movement Configuration")]
    public MovementType movementType = MovementType.Static;
    public float[] movementParameters = new float[4] { 0.5f, 2f, 1f, 2f }; // amplitude, speed, descend, smoothing
    
    [Header("Combat Configuration")]
    public float detectionRange = 1.5f;
    public float attackRange = 1.5f;
    public float timeBetweenAttacks = 2f;
    
    [Header("Animation Configuration")]
    public float animationSpeed = 0.15f;
    
    [Header("Difficulty Configuration")]
    public int minimumDifficultyLevel = 0;
    
    [Header("Collider Configuration")]
    [Tooltip("Radio del Circle Collider en unidades de Unity")]
    public float colliderRadius = 0.5f;
    [Tooltip("Multiplicador adicional basado en la escala del enemigo")]
    public bool scaleColliderWithSize = true;
    [Tooltip("Multiplicador para ajustar el tamaño del collider")]
    public float colliderSizeMultiplier = 1.0f;
    
    [Header("Score Configuration")]
    [Tooltip("Multiplicador de puntos específico para este tipo de enemigo")]
    public float pointsMultiplier = 1.0f;
    [Tooltip("Bonus points awarded for special conditions")]
    public int bonusPoints = 0;
    [Tooltip("Does this enemy award combo points?")]
    public bool awardsCombo = true;
    
    // Método auxiliar para obtener string del tipo de virus para carga de sprites
    public string GetVirusTypeString()
    {
        return virusType switch
        {
            VirusType.Classic => "classic",
            VirusType.Green => "green",
            VirusType.BlueRimLight => "blue-rim-light",
            VirusType.RedRimLight => "red-rim-light",
            _ => "classic"
        };
    }
    
    // Método auxiliar para calcular valor final de puntos con multiplicador
    public int GetFinalPointsValue()
    {
        return Mathf.RoundToInt((pointsValue + bonusPoints) * pointsMultiplier);
    }
} 
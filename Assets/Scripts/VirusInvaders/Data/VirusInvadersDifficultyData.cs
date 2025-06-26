using UnityEngine;

[CreateAssetMenu(fileName = "New Difficulty Data", menuName = "VirusInvaders/Difficulty Data")]
public class VirusInvadersDifficultyData : ScriptableObject
{
    [Header("Difficulty Configuration")]
    public int difficultyLevel = 0;
    public string difficultyName = "Easy";
    public int pointsRequired = 0;
    public Color difficultyColor = Color.white;
    
    [Header("Enemy Spawn Configuration")]
    public VirusInvadersEnemyData[] availableEnemies;
    public float spawnRate = 2f;
    public int maxEnemiesOnScreen = 5;
    public int minEnemiesOnScreen = 1;
    
    [Header("Gameplay Modifiers")]
    [Range(0.5f, 3f)]
    public float enemySpeedMultiplier = 1f;
    [Range(0.5f, 3f)]
    public float enemyHealthMultiplier = 1f;
    [Range(0.5f, 3f)]
    public float spawnRateMultiplier = 1f;
    [Range(0.5f, 2f)]
    public float enemyDamageMultiplier = 1f;
    
    [Header("Visual Effects")]
    public bool changeEnemyColors = true;
    public Color[] enemyColorVariations = { Color.white, Color.cyan, Color.yellow, Color.red };
} 
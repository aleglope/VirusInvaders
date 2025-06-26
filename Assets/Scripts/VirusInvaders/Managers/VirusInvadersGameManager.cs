using UnityEngine;
using System.Collections.Generic;
using System;

public class VirusInvadersGameManager : MonoBehaviour
{
    [Header("Configuración del Juego")]
    public VirusInvadersDifficultyData[] difficultyLevels;
    public Transform[] spawnPoints;
    public Transform player;
    
    [Header("Configuración de Puntuación")]
    public int currentScore = 0;
    public int currentDifficultyLevel = 0;
    
    [Header("Multiplicadores de Puntuación & Bonos")]
    public float baseScoreMultiplier = 1f;
    public float difficultyScoreMultiplier = 1f;
    public float comboMultiplier = 1f;
    public int comboCount = 0;
    public float comboTimeWindow = 3f;
    public int maxComboCount = 10;
    
    [Header("Sistema de Bonos")]
    public bool enableBonusSystem = true;
    public int rapidKillBonus = 50;
    public int perfectAccuracyBonus = 100;
    public int survivalTimeBonus = 10; // Puntos por segundo sobrevivido
    
    // Eventos
    public static event Action<int> OnScoreChanged;
    public static event Action<int> OnDifficultyChanged;
    public static event Action<VirusInvadersEnemyData> OnEnemyDefeated;
    public static event Action<int, Vector3> OnScoreGained; // cantidad de puntos, posición para números flotantes
    public static event Action<int> OnComboChanged;
    public static event Action<string, Vector3> OnBonusAchieved; // texto de bonus, posición
    public static event Action<int> OnGameOver; // puntuación final
    public static event Action<bool> OnGamePaused; // estado de pausa
    public static event Action OnGameReset; // reinicio de juego para componentes legacy
    
    // Variables privadas
    private float lastSpawnTime;
    private float lastKillTime;
    private float gameStartTime;
    private List<GameObject> activeEnemies = new List<GameObject>();
    private VirusInvadersDifficultyData currentDifficulty;
    private List<float> recentKillTimes = new List<float>();
    private bool isGameOver = false;
    private bool isGamePaused = false;
    
    // Seguimiento de estadísticas
    private int totalEnemiesKilled = 0;
    private int totalShotsFired = 0;
    private int totalHits = 0;
    
    // Patrón Singleton
    public static VirusInvadersGameManager Instance { get; private set; }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        InitializeGame();
    }
    
    void Update()
    {
        if (isGameOver) return;
        
        HandleEnemySpawning();
        CheckDifficultyProgression();
        CleanupDestroyedEnemies();
        UpdateComboSystem();
        UpdateSurvivalBonus();
    }
    
    void InitializeGame()
    {
        currentScore = 0;
        currentDifficultyLevel = 0;
        gameStartTime = Time.time;
        comboCount = 0;
        totalEnemiesKilled = 0;
        totalShotsFired = 0;
        totalHits = 0;
        isGameOver = false;
        
        if (difficultyLevels.Length > 0)
        {
            currentDifficulty = difficultyLevels[0];
            difficultyScoreMultiplier = 1f; // Multiplicador de dificultad base
        }
        
        // Buscar jugador si no está asignado
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        }
        
        // Suscribirse al evento de muerte del jugador (prevenir suscripciones duplicadas)
        SubscribeToPlayerEvents();
        
        // Inicializar eventos
        OnScoreChanged?.Invoke(currentScore);
        OnComboChanged?.Invoke(comboCount);
    }
    
    void SubscribeToPlayerEvents()
    {
        if (player != null)
        {
            VirusInvadersPlayerController playerController = player.GetComponent<VirusInvadersPlayerController>();
            if (playerController != null)
            {
                // Desuscribirse primero para prevenir duplicados
                playerController.OnPlayerDeath -= TriggerGameOver;
                // Luego suscribirse
                playerController.OnPlayerDeath += TriggerGameOver;
            }
        }
    }
    
    void UpdateComboSystem()
    {
        // Reiniciar combo si ha pasado demasiado tiempo desde la última muerte
        if (comboCount > 0 && Time.time - lastKillTime > comboTimeWindow)
        {
            ResetCombo();
        }
        
        // Limpiar tiempos de muerte antiguos
        recentKillTimes.RemoveAll(time => Time.time - time > comboTimeWindow);
    }
    
    void UpdateSurvivalBonus()
    {
        // Otorgar bonus de supervivencia cada 10 segundos
        float survivalTime = Time.time - gameStartTime;
        int survivalBonusPoints = Mathf.FloorToInt(survivalTime / 10f) * survivalTimeBonus;
        
        // Esto necesitaría ser rastreado para evitar dar bonus múltiples veces
        // La implementación depende de los requisitos específicos
    }
    
    void ResetCombo()
    {
        comboCount = 0;
        comboMultiplier = 1f;
        OnComboChanged?.Invoke(comboCount);
    }
    
    void HandleEnemySpawning()
    {
        if (currentDifficulty == null) return;
        
        if (Time.time - lastSpawnTime >= currentDifficulty.spawnRate)
        {
            if (activeEnemies.Count < currentDifficulty.maxEnemiesOnScreen)
            {
                SpawnRandomEnemy();
                lastSpawnTime = Time.time;
            }
        }
    }
    
    void SpawnRandomEnemy()
    {
        if (currentDifficulty.availableEnemies.Length == 0 || spawnPoints.Length == 0) return;
        
        // Seleccionar tipo de enemigo y punto de aparición aleatorio
        VirusInvadersEnemyData enemyData = currentDifficulty.availableEnemies[
            UnityEngine.Random.Range(0, currentDifficulty.availableEnemies.Length)
        ];
        
        Transform spawnPoint = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];
        
        GameObject enemy = CreateEnemy(enemyData, spawnPoint.position);
        activeEnemies.Add(enemy);
    }
    
    GameObject CreateEnemy(VirusInvadersEnemyData data, Vector3 position)
    {
        // Crear GameObject del enemigo
        GameObject enemy = new GameObject($"Enemy_{data.enemyName}");
        enemy.transform.position = position;
        enemy.tag = "Enemy";
        enemy.layer = LayerMask.NameToLayer("Enemies");
        
        // Agregar el nuevo componente EnemyController
        VirusInvadersEnemyController enemyController = enemy.AddComponent<VirusInvadersEnemyController>();
        enemyController.enemyData = data;
        
        return enemy;
    }
    
    void CheckDifficultyProgression()
    {
        int targetDifficulty = currentDifficultyLevel;
        
        for (int i = difficultyLevels.Length - 1; i >= 0; i--)
        {
            if (currentScore >= difficultyLevels[i].pointsRequired)
            {
                targetDifficulty = i;
                break;
            }
        }
        
        if (targetDifficulty != currentDifficultyLevel)
        {
            ChangeDifficulty(targetDifficulty);
        }
    }
    
    void ChangeDifficulty(int newDifficultyLevel)
    {
        int previousLevel = currentDifficultyLevel;
        currentDifficultyLevel = newDifficultyLevel;
        currentDifficulty = difficultyLevels[currentDifficultyLevel];
        
        // Actualizar multiplicador de puntuación por dificultad
        difficultyScoreMultiplier = 1f + (newDifficultyLevel * 0.25f); // 25% de aumento por nivel
        
        OnDifficultyChanged?.Invoke(currentDifficultyLevel);
        
        // Otorgar bonus de dificultad
        if (enableBonusSystem && newDifficultyLevel > previousLevel)
        {
            int difficultyBonus = 200 * (newDifficultyLevel + 1);
            Vector3 bonusPosition = player != null ? player.position : Vector3.zero;
            
            AddScore(difficultyBonus, bonusPosition, isBonus: true);
            OnBonusAchieved?.Invoke($"LEVEL UP! +{difficultyBonus}", bonusPosition);
        }
        
        // Actualizar enemigos existentes con nuevos modificadores de dificultad
        UpdateExistingEnemies();
    }
    
    void UpdateExistingEnemies()
    {
        foreach (GameObject enemy in activeEnemies)
        {
            if (enemy != null)
            {
                VirusInvadersEnemyController enemyController = enemy.GetComponent<VirusInvadersEnemyController>();
                if (enemyController != null && enemyController.enemyData != null)
                {
                    // Aplicar modificadores de dificultad actual a enemigos existentes
                    // El EnemyController manejará los ajustes de dificultad automáticamente
                }
            }
        }
    }
    
    void CleanupDestroyedEnemies()
    {
        activeEnemies.RemoveAll(enemy => enemy == null);
    }
    
    // Método AddScore mejorado con multiplicadores y posición
    public void AddScore(int basePoints, Vector3 position = default, bool isBonus = false)
    {
        // Calcular puntuación final con todos los multiplicadores
        float finalMultiplier = baseScoreMultiplier * difficultyScoreMultiplier * comboMultiplier;
        int finalScore = Mathf.RoundToInt(basePoints * finalMultiplier);
        
        currentScore += finalScore;
        
        // Disparar eventos
        OnScoreChanged?.Invoke(currentScore);
        OnScoreGained?.Invoke(finalScore, position);
        
        // Crear efecto de número flotante en la posición del enemigo (solo para puntuaciones normales, no bonos)
        if (position != default && !isBonus)
        {
            string displayText = $"+{finalScore}";
            Color numberColor = Color.green;
            
            if (comboCount > 1)
            {
                displayText += $" x{comboCount}";
                numberColor = Color.cyan;
            }
            
            // Crear número flotante manualmente para evitar problemas de dependencias
            CreateFloatingScoreText(position, displayText, numberColor);
        }
    }
    
    // Método auxiliar para crear texto flotante sin dependencias externas
    void CreateFloatingScoreText(Vector3 position, string text, Color color)
    {
        // Intentar encontrar el componente VirusInvadersFloatingNumber, si está disponible
        try
        {
            GameObject floatingTextObj = new GameObject("FloatingScore");
            floatingTextObj.transform.position = position;
            
            // Intentar agregar VirusInvadersFloatingNumber si existe
            var floatingScript = floatingTextObj.AddComponent<VirusInvadersFloatingNumber>();
            if (floatingScript != null)
            {
                floatingScript.Initialize(text, color);
            }
        }
        catch (System.Exception)
        {
            // Fallback: solo debug log si los números flotantes fallan
        }
    }
    
    // Sobrecarga para compatibilidad hacia atrás
    public void AddScore(int points)
    {
        AddScore(points, Vector3.zero, false);
    }
    
    public void EnemyDefeated(VirusInvadersEnemyData enemyData, Vector3 enemyPosition = default)
    {
        totalEnemiesKilled++;
        lastKillTime = Time.time;
        recentKillTimes.Add(lastKillTime);
        
        // Actualizar sistema de combos
        comboCount = Mathf.Min(comboCount + 1, maxComboCount);
        comboMultiplier = 1f + (comboCount - 1) * 0.1f; // 10% de aumento por combo
        
        OnComboChanged?.Invoke(comboCount);
        
        // Calcular puntuación base
        int baseScore = enemyData.pointsValue;
        
        // Verificar bonus de muerte rápida
        if (enableBonusSystem && recentKillTimes.Count >= 3)
        {
            float timeDiff = recentKillTimes[recentKillTimes.Count - 1] - recentKillTimes[recentKillTimes.Count - 3];
            if (timeDiff <= 2f) // 3 muertes en 2 segundos
            {
                baseScore += rapidKillBonus;
                OnBonusAchieved?.Invoke("FUEGO RAPIDO!", enemyPosition);
            }
        }
        
        AddScore(baseScore, enemyPosition);
        OnEnemyDefeated?.Invoke(enemyData);
    }
    
    // Sobrecarga para compatibilidad hacia atrás
    public void EnemyDefeated(VirusInvadersEnemyData enemyData)
    {
        EnemyDefeated(enemyData, Vector3.zero);
    }
    
    // Métodos públicos para seguimiento de estadísticas
    public void RegisterShot()
    {
        totalShotsFired++;
    }
    
    public void RegisterHit()
    {
        totalHits++;
        
        // Verificar bonus de puntería perfecta
        if (enableBonusSystem && totalShotsFired > 0)
        {
            float accuracy = (float)totalHits / totalShotsFired;
            if (accuracy >= 0.95f && totalShotsFired >= 10) // 95% de precisión con al menos 10 disparos
            {
                Vector3 bonusPosition = player != null ? player.position : Vector3.zero;
                AddScore(perfectAccuracyBonus, bonusPosition, isBonus: true);
                OnBonusAchieved?.Invoke("PUNTERIA PERFECTA!", bonusPosition);
            }
        }
    }
    
    // Getters públicos para UI
    public float GetAccuracy()
    {
        return totalShotsFired > 0 ? (float)totalHits / totalShotsFired : 0f;
    }
    
    public float GetSurvivalTime()
    {
        return Time.time - gameStartTime;
    }
    
    public int GetComboCount()
    {
        return comboCount;
    }
    
    public float GetScoreMultiplier()
    {
        return baseScoreMultiplier * difficultyScoreMultiplier * comboMultiplier;
    }
    
    // Método para resetear estado del juego
    public void ResetGame()
    {
        // Primero: Limpiar todos los objetos dinámicos mientras el juego está pausado
        CleanupAllDynamicObjects();
        
        // Segundo: Resetear todas las variables del juego
        isGameOver = false;
        currentScore = 0;
        currentDifficultyLevel = 0;
        comboCount = 0;
        comboMultiplier = 1f;
        totalEnemiesKilled = 0;
        totalShotsFired = 0;
        totalHits = 0;
        lastSpawnTime = 0f;
        lastKillTime = 0f;
        gameStartTime = Time.time;
        
        // Limpiar lista de tiempos de muerte recientes
        recentKillTimes.Clear();
        
        // Limpiar todos los enemigos activos
        foreach (GameObject enemy in activeEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }
        activeEnemies.Clear();
        
        // Tercero: Re-establecer conexión con el jugador
        ReestablishPlayerConnection();
        
        // Cuarto: Resetear dificultad al estado inicial
        if (difficultyLevels.Length > 0)
        {
            currentDifficulty = difficultyLevels[0];
            difficultyScoreMultiplier = 1f;
        }
        
        // Quinto: Disparar eventos de reset ANTES de despausar
        OnScoreChanged?.Invoke(currentScore);
        OnComboChanged?.Invoke(comboCount);
        OnDifficultyChanged?.Invoke(currentDifficultyLevel);
        
        // Notificar a componentes legacy para que se reseteén
        OnGameReset?.Invoke();
        // Sexto: Reanudar juego (esto debe ser ÚLTIMO)
        isGamePaused = false;
        OnGamePaused?.Invoke(false);
    }
    
    // Método para re-establecer conexión con el jugador después del reset
    void ReestablishPlayerConnection()
    {
        // Encontrar jugador (debería existir si ResetPlayer fue llamado primero)
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        }
        
        if (player != null)
        {
            // Suscribirse al evento de muerte del jugador (con desuscripción limpia primero)
            VirusInvadersPlayerController playerController = player.GetComponent<VirusInvadersPlayerController>();
            if (playerController != null)
            {
                // Desuscripción limpia
                playerController.OnPlayerDeath -= TriggerGameOver;
                // Suscripción fresca
                playerController.OnPlayerDeath += TriggerGameOver;
            }
        }
    }
    
    // Método para limpiar todos los objetos dinámicos creados durante el gameplay
    void CleanupAllDynamicObjects()
    {
        // Limpiar balas
        GameObject[] bullets = GameObject.FindGameObjectsWithTag("Bullet");
        foreach (GameObject bullet in bullets)
        {
            if (bullet != null)
            {
                Destroy(bullet);
            }
        }
        
        // Limpiar textos de puntuación flotantes
        VirusInvadersFloatingNumber[] floatingNumbers = FindObjectsByType<VirusInvadersFloatingNumber>(FindObjectsSortMode.None);
        foreach (VirusInvadersFloatingNumber floatingNum in floatingNumbers)
        {
            if (floatingNum != null)
            {
                Destroy(floatingNum.gameObject);
            }
        }
        // Limpiar efectos de explosión
        VirusInvadersBoomEffect[] boomEffects = FindObjectsByType<VirusInvadersBoomEffect>(FindObjectsSortMode.None);
        foreach (VirusInvadersBoomEffect effect in boomEffects)
        {
            if (effect != null)
            {
                Destroy(effect.gameObject);
            }
        }
        
        // Limpiar medkits
        VirusInvadersMedKit[] medkits = FindObjectsByType<VirusInvadersMedKit>(FindObjectsSortMode.None);
        foreach (VirusInvadersMedKit medkit in medkits)
        {
            if (medkit != null)
            {
                Destroy(medkit.gameObject);
            }
        }
        
        // *** RESETEAR ENEMIGOS CORONAVIRUS LEGACY ***
        // En lugar de destruirlos, resetearlos al estado inicial
        VirusInvadersCoronavirusEnemy[] legacyEnemies = FindObjectsByType<VirusInvadersCoronavirusEnemy>(FindObjectsSortMode.None);
        foreach (VirusInvadersCoronavirusEnemy legacyEnemy in legacyEnemies)
        {
            if (legacyEnemy != null)
            {
                // Forzar parada de cualquier corrutina en curso (animaciones de muerte, procesos de reaparición, etc.)
                legacyEnemy.StopAllCoroutines();
                
                // Resetear el enemigo completamente - esto será llamado nuevamente por OnGameReset
                // pero es bueno asegurar limpieza inmediata
            }
        }
        
        // Limpiar cualquier GameObject creado por patrones de nombres
        string[] dynamicObjectNames = {
            "FloatingScore",
            "VirusInvadersBoomEffect", 
            "VirusInvadersMedKit",
            "VirusInvadersFloatingNumber"
        };
        
        int dynamicObjectsDestroyed = 0;
        foreach (string objectName in dynamicObjectNames)
        {
            GameObject[] objects = GameObject.FindGameObjectsWithTag("Untagged");
            foreach (GameObject obj in objects)
            {
                if (obj != null && obj.name.Contains(objectName))
                {
                    Destroy(obj);
                    dynamicObjectsDestroyed++;
                }
            }
        }
    }

    void TriggerGameOver()
    {
        if (isGameOver) 
        {
            return;
        }
        
        isGameOver = true;
        isGamePaused = true;
        
        // Pausar todos los elementos del juego
        OnGamePaused?.Invoke(true);
        OnGameOver?.Invoke(currentScore);
    }
}

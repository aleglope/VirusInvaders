using UnityEngine;
using TMPro;

public class VirusInvadersGameUI : MonoBehaviour
{
    [Header("VirusInvaders - Referencias de Texto UI")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI comboText;
    public TextMeshProUGUI difficultyText;
    public TextMeshProUGUI accuracyText;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI gameOverText;
    
    [Header("VirusInvaders - Configuración")]
    public bool animateScore = true;
    public float scoreAnimationSpeed = 3f;
    
    [Header("VirusInvaders - Configuración de Entrada")]
    public KeyCode restartKey = KeyCode.R;
    
    // Variables privadas
    private int displayedScore = 0;
    private int targetScore = 0;
    private bool isGameOver = false;
    
    void Start()
    {
        InitializeUI();
        SubscribeToEvents();
    }
    
    void OnDestroy()
    {
        UnsubscribeFromEvents();
    }
    
    void InitializeUI()
    {
        UpdateScore(0);
        UpdateCombo(0);
        UpdateDifficulty(1);
        UpdateAccuracy(0f);
        UpdateTime(0f);
        
        // Ocultar combo inicialmente
        if (comboText != null)
            comboText.gameObject.SetActive(false);
            
        // Ocultar game over inicialmente
        if (gameOverText != null)
            gameOverText.gameObject.SetActive(false);
    }
    
    void SubscribeToEvents()
    {

        
        VirusInvadersGameManager.OnScoreChanged += OnScoreChanged;
        VirusInvadersGameManager.OnComboChanged += OnComboChanged;
        VirusInvadersGameManager.OnDifficultyChanged += OnDifficultyChanged;
        VirusInvadersGameManager.OnBonusAchieved += OnBonusAchieved;
        VirusInvadersGameManager.OnGameOver += OnGameOver;
        

    }
    
    void UnsubscribeFromEvents()
    {
        VirusInvadersGameManager.OnScoreChanged -= OnScoreChanged;
        VirusInvadersGameManager.OnComboChanged -= OnComboChanged;
        VirusInvadersGameManager.OnDifficultyChanged -= OnDifficultyChanged;
        VirusInvadersGameManager.OnBonusAchieved -= OnBonusAchieved;
        VirusInvadersGameManager.OnGameOver -= OnGameOver;
    }
    
    void Update()
    {
        // Animar conteo de puntuación
        if (animateScore && displayedScore != targetScore)
        {
            displayedScore = Mathf.RoundToInt(Mathf.Lerp(displayedScore, targetScore, scoreAnimationSpeed * Time.deltaTime));
            UpdateScoreText();
        }
        
        // Manejar entrada de reinicio
        if (isGameOver)
        {
            // Debug: Verificar detección de tecla R
            if (Input.GetKeyDown(restartKey))
            {
        
                RestartGame();
            }
            
            // Debug: Mostrar que estamos en estado de game over cada 60 frames
            if (Time.frameCount % 60 == 0)
            {
    
            }
        }
        
        // Actualizar estadísticas en tiempo real
        if (VirusInvadersGameManager.Instance != null && !isGameOver)
        {
            float accuracy = VirusInvadersGameManager.Instance.GetAccuracy() * 100f;
            float survivalTime = VirusInvadersGameManager.Instance.GetSurvivalTime();
            
            UpdateAccuracy(accuracy);
            UpdateTime(survivalTime);
        }
    }
    
    // Manejadores de eventos
    void OnScoreChanged(int newScore)
    {
        targetScore = newScore;
        if (!animateScore)
        {
            displayedScore = newScore;
            UpdateScoreText();
        }
    }
    
    void OnComboChanged(int newCombo)
    {
        UpdateCombo(newCombo);
    }
    
    void OnDifficultyChanged(int newDifficulty)
    {
        UpdateDifficulty(newDifficulty + 1); // Mostrar basado en 1
    }
    
    void OnBonusAchieved(string bonusText, Vector3 position)
    {
        // Crear texto flotante de bonificación
        VirusInvadersFloatingNumber.CreateBonusFloatingNumber(position, bonusText);
    }
    
    void OnGameOver(int finalScore)
    {
        isGameOver = true;
        
        // Crear gameOverText si no existe
        if (gameOverText == null)
        {
            CreateGameOverText();
        }
        
        if (gameOverText != null)
        {
            gameOverText.text = $"GAME OVER\nFINAL SCORE: {finalScore:000000}\nPRESS {restartKey} TO RESTART";
            gameOverText.color = Color.red;
            gameOverText.fontSize = 36;
            gameOverText.alignment = TMPro.TextAlignmentOptions.Center;
            gameOverText.gameObject.SetActive(true);
        }
    }
    
    void CreateGameOverText()
    {
        // Encontrar o crear un Canvas
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("VirusInvaders_GameOverCanvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }
        
        // Crear GameObject de texto GameOver
        GameObject gameOverGO = new GameObject("VirusInvaders_GameOverText");
        gameOverGO.transform.SetParent(canvas.transform, false);
        
        // Configurar RectTransform para cubrir el centro de la pantalla
        RectTransform rt = gameOverGO.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        
        // Agregar componente TextMeshProUGUI
        gameOverText = gameOverGO.AddComponent<TextMeshProUGUI>();
        gameOverText.text = "GAME OVER";
        gameOverText.fontSize = 36;
        gameOverText.color = Color.red;
        gameOverText.alignment = TMPro.TextAlignmentOptions.Center;
        gameOverText.fontStyle = TMPro.FontStyles.Bold;
        
        // Inicialmente oculto
        gameOverGO.SetActive(false);
        

    }
    
    // Métodos de actualización
    void UpdateScore(int score)
    {
        displayedScore = score;
        targetScore = score;
        UpdateScoreText();
    }
    
    void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = $"SCORE: {displayedScore:000000}";
        }
    }
    
    void UpdateCombo(int combo)
    {
        if (comboText != null)
        {
            if (combo > 1)
            {
                comboText.text = $"COMBO x{combo}";
                comboText.gameObject.SetActive(true);
                
                // Cambiar color basado en el nivel de combo
                if (combo >= 10)
                    comboText.color = Color.red;
                else if (combo >= 5)
                    comboText.color = Color.yellow;
                else
                    comboText.color = Color.cyan;
            }
            else
            {
                comboText.gameObject.SetActive(false);
            }
        }
    }
    
    void UpdateDifficulty(int level)
    {
        if (difficultyText != null)
        {
            difficultyText.text = $"LEVEL: {level}";
        }
    }
    
    void UpdateAccuracy(float accuracy)
    {
        if (accuracyText != null)
        {
            accuracyText.text = $"ACCURACY: {accuracy:F1}%";
            
            // Codificar precisión por color
            if (accuracy >= 90f)
                accuracyText.color = Color.green;
            else if (accuracy >= 70f)
                accuracyText.color = Color.yellow;
            else if (accuracy >= 50f)
                accuracyText.color = new Color(1f, 0.5f, 0f); // Color naranja
            else
                accuracyText.color = Color.red;
        }
    }
    
    void UpdateTime(float time)
    {
        if (timeText != null)
        {
            int minutes = Mathf.FloorToInt(time / 60f);
            int seconds = Mathf.FloorToInt(time % 60f);
            timeText.text = $"TIME: {minutes:00}:{seconds:00}";
        }
    }
    
    void RestartGame()
    {
        // Paso 1: Establecer estado de UI
        isGameOver = false;
        
        // Paso 2: Ocultar texto de game over
        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(false);
        }
        
        // Paso 3: Resetear jugador PRIMERO (mientras aún está pausado)
        ResetPlayer();
        
        // Paso 4: Resetear estado del juego a través del GameManager
        if (VirusInvadersGameManager.Instance != null)
        {
            VirusInvadersGameManager.Instance.ResetGame();
        }
        
        // Paso 5: Resetear elementos de UI AL FINAL
        InitializeUI();
    }
    
    void ResetPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            VirusInvadersPlayerController playerController = player.GetComponent<VirusInvadersPlayerController>();
            if (playerController != null)
            {
                playerController.ResetPlayer();
            }
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

[System.Serializable]
public class VirusInvadersScoreDisplay : MonoBehaviour
{
    [Header("VirusInvaders - Configuración de Pantalla de Puntuación")]
    public int currentScore = 0;
    public bool useCanvasUI = true; // True para UI Canvas, False para texto de espacio mundial
    public bool animateScoreChanges = true;
    public float animationDuration = 0.8f;
    
    [Header("VirusInvaders - Configuración Visual")]
    public string scorePrefix = "SCORE: ";
    public string scoreFormat = "000000"; // Formato para mostrar puntuación (ej., 000050)
    public Color scoreColor = Color.white;
    public Color scoreIncreaseColor = Color.green;
    public float colorFlashDuration = 0.3f;
    
    [Header("VirusInvaders - Tipografía")]
    public int fontSize = 24;
    public FontStyles fontStyle = FontStyles.Bold;
    public TextAlignmentOptions textAlignment = TextAlignmentOptions.Center;
    
    [Header("VirusInvaders - Efectos")]
    public bool enablePulseEffect = true;
    public float pulseScale = 1.2f;
    public float pulseDuration = 0.15f;
    public bool enableFloatingNumbers = true;
    public GameObject floatingNumberPrefab;
    
    // Referencias privadas
    private TextMeshProUGUI canvasText;
    private TextMeshPro worldText;
    private int displayedScore = 0;
    private Color originalColor;
    private Vector3 originalScale;
    private RectTransform rectTransform;
    
    // Corrutinas de animación
    private Coroutine scoreAnimationCoroutine;
    private Coroutine colorFlashCoroutine;
    private Coroutine pulseCoroutine;
    
    void Start()
    {
        InitializeScoreDisplay();
        SubscribeToEvents();
    }
    
    void OnDestroy()
    {
        UnsubscribeFromEvents();
    }
    
    void InitializeScoreDisplay()
    {
        displayedScore = currentScore;
        
        if (useCanvasUI)
        {
            InitializeCanvasUI();
        }
        else
        {
            InitializeWorldSpaceUI();
        }
        
        originalScale = transform.localScale;
        UpdateScoreDisplay();
    }
    
    void InitializeCanvasUI()
    {
        canvasText = GetComponent<TextMeshProUGUI>();
        
        if (canvasText == null)
        {
            canvasText = gameObject.AddComponent<TextMeshProUGUI>();
        }
        
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            rectTransform = gameObject.AddComponent<RectTransform>();
        }
        
        ConfigureTextComponent(canvasText);
        
        // Configurar ordenamiento de canvas
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            canvas.sortingOrder = 100;
        }
    }
    
    void InitializeWorldSpaceUI()
    {
        worldText = GetComponent<TextMeshPro>();
        
        if (worldText == null)
        {
            worldText = gameObject.AddComponent<TextMeshPro>();
        }
        
        ConfigureTextComponent(worldText);
        
        // Configurar propiedades de texto de espacio mundial usando renderer
        if (worldText.renderer != null)
        {
            worldText.renderer.sortingLayerName = "UI";
            worldText.renderer.sortingOrder = 50;
        }
    }
    
    void ConfigureTextComponent(TMP_Text textComponent)
    {
        textComponent.text = GetFormattedScore(currentScore);
        textComponent.fontSize = fontSize;
        textComponent.fontStyle = fontStyle;
        textComponent.alignment = textAlignment;
        textComponent.color = scoreColor;
        
        originalColor = scoreColor;
    }
    
    void SubscribeToEvents()
    {
        VirusInvadersGameManager.OnScoreChanged += OnScoreChanged;
        VirusInvadersGameManager.OnDifficultyChanged += OnDifficultyChanged;
    }
    
    void UnsubscribeFromEvents()
    {
        VirusInvadersGameManager.OnScoreChanged -= OnScoreChanged;
        VirusInvadersGameManager.OnDifficultyChanged -= OnDifficultyChanged;
    }
    
    void OnScoreChanged(int newScore)
    {
        int scoreIncrease = newScore - currentScore;
        currentScore = newScore;
        
        if (animateScoreChanges)
        {
            AnimateScoreChange(scoreIncrease);
        }
        else
        {
            displayedScore = currentScore;
            UpdateScoreDisplay();
        }
        
        // Crear efecto de número flotante
        if (enableFloatingNumbers && scoreIncrease > 0)
        {
            CreateFloatingNumber(scoreIncrease);
        }
    }
    
    void OnDifficultyChanged(int newDifficultyLevel)
    {
        // Parpadear color para indicar cambio de dificultad
        if (colorFlashCoroutine != null)
        {
            StopCoroutine(colorFlashCoroutine);
        }
        colorFlashCoroutine = StartCoroutine(FlashColor(Color.yellow));
    }
    
    void AnimateScoreChange(int scoreIncrease)
    {
        if (scoreAnimationCoroutine != null)
        {
            StopCoroutine(scoreAnimationCoroutine);
        }
        
        scoreAnimationCoroutine = StartCoroutine(AnimateScoreCoroutine(scoreIncrease));
    }
    
    IEnumerator AnimateScoreCoroutine(int scoreIncrease)
    {
        int startScore = displayedScore;
        int targetScore = currentScore;
        float elapsed = 0f;
        
        // Efecto de pulso para aumento de puntuación
        if (enablePulseEffect && scoreIncrease > 0)
        {
            if (pulseCoroutine != null)
            {
                StopCoroutine(pulseCoroutine);
            }
            pulseCoroutine = StartCoroutine(PulseEffect());
        }
        
        // Parpadeo de color para puntuación positiva
        if (scoreIncrease > 0)
        {
            if (colorFlashCoroutine != null)
            {
                StopCoroutine(colorFlashCoroutine);
            }
            colorFlashCoroutine = StartCoroutine(FlashColor(scoreIncreaseColor));
        }
        
        // Animar conteo de puntuación
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / animationDuration;
            
            // Usar suavizado para animación fluida
            float easedProgress = EaseOutCubic(progress);
            displayedScore = Mathf.RoundToInt(Mathf.Lerp(startScore, targetScore, easedProgress));
            
            UpdateScoreDisplay();
            yield return null;
        }
        
        displayedScore = targetScore;
        UpdateScoreDisplay();
    }
    
    IEnumerator PulseEffect()
    {
        Vector3 targetScale = originalScale * pulseScale;
        float elapsed = 0f;
        
        // Escalar hacia arriba
        while (elapsed < pulseDuration / 2f)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / (pulseDuration / 2f);
            transform.localScale = Vector3.Lerp(originalScale, targetScale, progress);
            yield return null;
        }
        
        elapsed = 0f;
        
        // Escalar hacia abajo
        while (elapsed < pulseDuration / 2f)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / (pulseDuration / 2f);
            transform.localScale = Vector3.Lerp(targetScale, originalScale, progress);
            yield return null;
        }
        
        transform.localScale = originalScale;
    }
    
    IEnumerator FlashColor(Color flashColor)
    {
        TMP_Text activeText = useCanvasUI ? (TMP_Text)canvasText : (TMP_Text)worldText;
        
        float elapsed = 0f;
        
        // Parpadear a nuevo color
        while (elapsed < colorFlashDuration / 2f)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / (colorFlashDuration / 2f);
            activeText.color = Color.Lerp(originalColor, flashColor, progress);
            yield return null;
        }
        
        elapsed = 0f;
        
        // Parpadear de vuelta al original
        while (elapsed < colorFlashDuration / 2f)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / (colorFlashDuration / 2f);
            activeText.color = Color.Lerp(flashColor, originalColor, progress);
            yield return null;
        }
        
        activeText.color = originalColor;
    }
    
    void CreateFloatingNumber(int scoreIncrease)
    {
        if (floatingNumberPrefab == null) return;
        
        Vector3 spawnPosition = transform.position;
        if (useCanvasUI && rectTransform != null)
        {
            spawnPosition = rectTransform.position;
        }
        
        GameObject floatingNumber = Instantiate(floatingNumberPrefab, spawnPosition, Quaternion.identity);
        
        // Configurar el número flotante
        VirusInvadersFloatingNumber floatingScript = floatingNumber.GetComponent<VirusInvadersFloatingNumber>();
        if (floatingScript != null)
        {
            floatingScript.Initialize($"+{scoreIncrease}", scoreIncreaseColor);
        }
    }
    
    void UpdateScoreDisplay()
    {
        string formattedScore = GetFormattedScore(displayedScore);
        
        if (useCanvasUI && canvasText != null)
        {
            canvasText.text = formattedScore;
        }
        else if (worldText != null)
        {
            worldText.text = formattedScore;
        }
    }
    
    string GetFormattedScore(int score)
    {
        return scorePrefix + score.ToString(scoreFormat);
    }
    
    // Función de suavizado para animaciones fluidas
    float EaseOutCubic(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3f);
    }
    
    // Métodos públicos
    public void SetScore(int newScore)
    {
        currentScore = newScore;
        displayedScore = newScore;
        UpdateScoreDisplay();
    }
    
    public void ForceUpdateDisplay()
    {
        if (VirusInvadersGameManager.Instance != null)
        {
            currentScore = VirusInvadersGameManager.Instance.currentScore;
            displayedScore = currentScore;
            UpdateScoreDisplay();
        }
    }
    
    // Método factory estático
    public static GameObject CreateScoreDisplay(Vector3 position, Transform parent = null, bool useCanvas = true)
    {
        GameObject scoreDisplayGO = new GameObject("VirusInvadersScoreDisplay");
        scoreDisplayGO.transform.position = position;
        
        if (parent != null)
        {
            scoreDisplayGO.transform.SetParent(parent);
        }
        
        VirusInvadersScoreDisplay scoreDisplay = scoreDisplayGO.AddComponent<VirusInvadersScoreDisplay>();
        scoreDisplay.useCanvasUI = useCanvas;
        
        return scoreDisplayGO;
    }
} 
using UnityEngine;
using TMPro;
using System.Collections;

[System.Serializable]
public class VirusInvadersFloatingNumber : MonoBehaviour
{
    [Header("VirusInvaders - Configuración de Número Flotante")]
    public float animationDuration = 1.5f;
    public float moveDistance = 2f;
    public AnimationCurve movementCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));
    public AnimationCurve scaleCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));
    public AnimationCurve alphaCurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0f));
    
    [Header("VirusInvaders - Configuración Visual")]
    public int fontSize = 5;
    public FontStyles fontStyle = FontStyles.Bold;
    public Color defaultColor = Color.white;
    public float initialScale = 1f;
    public float maxScale = 1.5f;
    
    [Header("VirusInvaders - Movimiento")]
    public Vector3 moveDirection = Vector3.up;
    public bool randomizeDirection = false;
    public float randomAngle = 30f;
    
    [Header("VirusInvaders - Física")]
    public bool useGravity = false;
    public float gravityStrength = 0.5f;
    
    // Referencias privadas
    private TextMeshPro textComponent;
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private Color originalColor;
    private bool isInitialized = false;
    
    void Start()
    {
        if (!isInitialized)
        {
            Initialize("+10", defaultColor);
        }
    }
    
    public void Initialize(string text, Color color)
    {
        SetupTextComponent();
        
        textComponent.text = text;
        textComponent.color = color;
        originalColor = color;
        
        startPosition = transform.position;
        
        // Calcular dirección de movimiento
        Vector3 direction = moveDirection.normalized;
        if (randomizeDirection)
        {
            float angle = Random.Range(-randomAngle, randomAngle);
            direction = Quaternion.Euler(0, 0, angle) * direction;
        }
        
        targetPosition = startPosition + direction * moveDistance;
        
        isInitialized = true;
        StartCoroutine(AnimateFloatingNumber());
    }
    
    void SetupTextComponent()
    {
        textComponent = GetComponent<TextMeshPro>();
        
        if (textComponent == null)
        {
            textComponent = gameObject.AddComponent<TextMeshPro>();
        }
        
        // Configurar propiedades del texto
        textComponent.fontSize = fontSize;
        textComponent.fontStyle = fontStyle;
        textComponent.alignment = TextAlignmentOptions.Center;
        
        // Configurar ordenamiento para TextMeshPro usando renderer
        if (textComponent.renderer != null)
        {
            textComponent.renderer.sortingLayerName = "UI";
            textComponent.renderer.sortingOrder = 100;
        }
        
        // Establecer escala inicial
        transform.localScale = Vector3.one * initialScale;
    }
    
    IEnumerator AnimateFloatingNumber()
    {
        float elapsed = 0f;
        
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / animationDuration;
            
            // Animación de posición
            float moveProgress = movementCurve.Evaluate(progress);
            Vector3 currentPosition = Vector3.Lerp(startPosition, targetPosition, moveProgress);
            
            // Aplicar gravedad si está habilitada
            if (useGravity)
            {
                float gravityOffset = -0.5f * gravityStrength * (elapsed * elapsed);
                currentPosition.y += gravityOffset;
            }
            
            transform.position = currentPosition;
            
            // Animación de escala
            float scaleProgress = scaleCurve.Evaluate(progress);
            float currentScale = Mathf.Lerp(initialScale, maxScale, scaleProgress);
            transform.localScale = Vector3.one * currentScale;
            
            // Animación de alpha
            float alphaProgress = alphaCurve.Evaluate(progress);
            Color currentColor = originalColor;
            currentColor.a = alphaProgress;
            textComponent.color = currentColor;
            
            yield return null;
        }
        
        // Destruir después de que la animación termine
        Destroy(gameObject);
    }
    
    // Métodos públicos para configuración en tiempo de ejecución
    public void SetText(string text)
    {
        if (textComponent != null)
        {
            textComponent.text = text;
        }
    }
    
    public void SetColor(Color color)
    {
        originalColor = color;
        if (textComponent != null)
        {
            textComponent.color = color;
        }
    }
    
    public void SetDuration(float duration)
    {
        animationDuration = duration;
    }
    
    public void SetMoveDistance(float distance)
    {
        moveDistance = distance;
        
        // Recalcular posición objetivo si ya está inicializado
        if (isInitialized)
        {
            Vector3 direction = moveDirection.normalized;
            if (randomizeDirection)
            {
                float angle = Random.Range(-randomAngle, randomAngle);
                direction = Quaternion.Euler(0, 0, angle) * direction;
            }
            targetPosition = startPosition + direction * moveDistance;
        }
    }
    
    // Método factory estático
    public static GameObject CreateFloatingNumber(Vector3 position, string text, Color color)
    {
        GameObject floatingNumberGO = new GameObject("VirusInvadersFloatingNumber");
        floatingNumberGO.transform.position = position;
        
        VirusInvadersFloatingNumber floatingNumber = floatingNumberGO.AddComponent<VirusInvadersFloatingNumber>();
        floatingNumber.Initialize(text, color);
        
        return floatingNumberGO;
    }
    
    // Configuraciones predefinidas para casos de uso comunes
    public static GameObject CreateScoreFloatingNumber(Vector3 position, int scoreValue)
    {
        Color color = scoreValue > 0 ? Color.green : Color.red;
        string text = scoreValue > 0 ? $"+{scoreValue}" : scoreValue.ToString();
        
        GameObject floatingNumber = CreateFloatingNumber(position, text, color);
        VirusInvadersFloatingNumber script = floatingNumber.GetComponent<VirusInvadersFloatingNumber>();
        
        // Configurar para mostrar puntuación
        script.fontSize = 12;
        script.fontStyle = FontStyles.Bold;
        script.moveDistance = 1.5f;
        script.animationDuration = 1.2f;
        
        return floatingNumber;
    }
    
    public static GameObject CreateBonusFloatingNumber(Vector3 position, string bonusText)
    {
        GameObject floatingNumber = CreateFloatingNumber(position, bonusText, Color.yellow);
        VirusInvadersFloatingNumber script = floatingNumber.GetComponent<VirusInvadersFloatingNumber>();
        
        // Configurar para mostrar bonificación
        script.fontSize = 10;
        script.fontStyle = FontStyles.Italic;
        script.moveDistance = 2f;
        script.animationDuration = 2f;
        script.randomizeDirection = true;
        script.randomAngle = 45f;
        
        return floatingNumber;
    }
} 
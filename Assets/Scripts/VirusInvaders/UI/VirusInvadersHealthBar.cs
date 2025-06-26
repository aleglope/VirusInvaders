using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class VirusInvadersHealthBar : MonoBehaviour
{
    [Header("VirusInvaders - Health Bar Configuration")]
    public float maxHealth = 100f;
    public float currentHealth = 100f;
    public bool useImageComponent = true; // True para UI Canvas, False para SpriteRenderer
    
    [Header("VirusInvaders - Visual Settings")]
    public bool smoothTransition = true;
    public float transitionSpeed = 2f;
    public bool changeColorWithHealth = true;
    
    [Header("VirusInvaders - Color Thresholds")]
    public Color fullHealthColor = Color.green;
    public Color mediumHealthColor = Color.yellow;
    public Color lowHealthColor = Color.red;
    [Range(0f, 1f)]
    public float mediumHealthThreshold = 0.7f;
    [Range(0f, 1f)]
    public float lowHealthThreshold = 0.3f;
    
    // Referencias privadas
    private SpriteRenderer spriteRenderer;
    private Image imageComponent;
    private Vector3 originalScale;
    private Vector3 targetScale;
    private Color originalColor;
    private float displayedHealth;
    
    // Para UI Canvas
    private RectTransform rectTransform;
    private Vector2 originalSize;
    
    void Start()
    {
        InitializeHealthBar();
    }
    
    void InitializeHealthBar()
    {
        displayedHealth = currentHealth;
        
        if (useImageComponent)
        {
            InitializeUIHealthBar();
        }
        else
        {
            InitializeWorldHealthBar();
        }
    }
    
    void InitializeUIHealthBar()
    {
        imageComponent = GetComponent<Image>();
        
        if (imageComponent == null)
        {
            Debug.LogError("VirusInvaders: Se requiere un componente Image para modo UI Canvas!");
            return;
        }
        
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            originalSize = rectTransform.sizeDelta;
        }
        
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            canvas.sortingOrder = 100;
        }
        
        imageComponent.type = Image.Type.Filled;
        imageComponent.fillMethod = Image.FillMethod.Horizontal;
        imageComponent.fillOrigin = 0;
    }
    
    void InitializeWorldHealthBar()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        
        originalScale = transform.localScale;
        targetScale = originalScale;
        originalColor = spriteRenderer.color;
        
        spriteRenderer.sortingLayerName = "UI";
        spriteRenderer.sortingOrder = 50;
    }
    
    void Update()
    {
        if (smoothTransition && !Mathf.Approximately(displayedHealth, currentHealth))
        {
            displayedHealth = Mathf.Lerp(displayedHealth, currentHealth, transitionSpeed * Time.deltaTime);
            
            if (Mathf.Abs(displayedHealth - currentHealth) < 0.1f)
            {
                displayedHealth = currentHealth;
            }
            
            UpdateHealthDisplay();
        }
        
        if (!useImageComponent && Vector3.Distance(transform.localScale, targetScale) > 0.001f)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, transitionSpeed * 2f * Time.deltaTime);
        }
    }
    
    public void SetHealth(float newHealth)
    {
        float previousHealth = currentHealth;
        currentHealth = Mathf.Clamp(newHealth, 0f, maxHealth);
        
        if (!smoothTransition)
        {
            displayedHealth = currentHealth;
            UpdateHealthDisplay();
        }
    }
    
    public void TakeDamage(float damage)
    {
        SetHealth(currentHealth - damage);
    }
    
    public void Heal(float healAmount)
    {
        SetHealth(currentHealth + healAmount);
    }
    
    public void SetMaxHealth(float newMaxHealth)
    {
        float healthPercentage = currentHealth / maxHealth;
        maxHealth = newMaxHealth;
        currentHealth = healthPercentage * maxHealth;
        displayedHealth = currentHealth;
        UpdateHealthDisplay();
    }
    
    void UpdateHealthDisplay()
    {
        float healthPercentage = displayedHealth / maxHealth;
        
        if (useImageComponent && imageComponent != null)
        {
            imageComponent.fillAmount = healthPercentage;
            
            if (changeColorWithHealth)
            {
                imageComponent.color = GetHealthColor(healthPercentage);
            }
        }
        else if (spriteRenderer != null)
        {
            targetScale = originalScale;
            targetScale.x = originalScale.x * healthPercentage;
            
            if (!smoothTransition)
            {
                transform.localScale = targetScale;
            }
            
            if (changeColorWithHealth)
            {
                spriteRenderer.color = GetHealthColor(healthPercentage);
            }
        }
    }
    
    Color GetHealthColor(float healthPercentage)
    {
        if (healthPercentage > mediumHealthThreshold)
        {
            float t = (healthPercentage - mediumHealthThreshold) / (1f - mediumHealthThreshold);
            return Color.Lerp(mediumHealthColor, fullHealthColor, t);
        }
        else if (healthPercentage > lowHealthThreshold)
        {
            float t = (healthPercentage - lowHealthThreshold) / (mediumHealthThreshold - lowHealthThreshold);
            return Color.Lerp(lowHealthColor, mediumHealthColor, t);
        }
        else
        {
            return lowHealthColor;
        }
    }
    
    // Métodos públicos de consulta
    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }
    
    public bool IsAlive()
    {
        return currentHealth > 0f;
    }
    
    public bool IsFullHealth()
    {
        return Mathf.Approximately(currentHealth, maxHealth);
    }
    
    public bool IsLowHealth()
    {
        return GetHealthPercentage() <= lowHealthThreshold;
    }
    
    public void SetHealthBarSprite(Sprite healthBarSprite)
    {
        if (useImageComponent && imageComponent != null)
        {
            imageComponent.sprite = healthBarSprite;
        }
        else if (spriteRenderer != null)
        {
            spriteRenderer.sprite = healthBarSprite;
        }
    }
    
    // Método factory estático
    public static GameObject CreateHealthBar(Vector3 position, bool useUI = false)
    {
        GameObject healthBarGO = new GameObject("VirusInvadersHealthBar");
        healthBarGO.transform.position = position;
        
        VirusInvadersHealthBar healthBar = healthBarGO.AddComponent<VirusInvadersHealthBar>();
        healthBar.useImageComponent = useUI;
        
        if (useUI)
        {
            healthBarGO.AddComponent<RectTransform>();
            Image img = healthBarGO.AddComponent<Image>();
            
            Sprite barSprite = Resources.Load<Sprite>("VirusInvaders/Sprites/blood_red_bar");
            if (barSprite != null)
            {
                img.sprite = barSprite;
            }
        }
        else
        {
            SpriteRenderer sr = healthBarGO.AddComponent<SpriteRenderer>();
            
            Sprite barSprite = Resources.Load<Sprite>("VirusInvaders/Sprites/blood_red_bar");
            if (barSprite != null)
            {
                sr.sprite = barSprite;
            }
        }
        
        return healthBarGO;
    }
} 
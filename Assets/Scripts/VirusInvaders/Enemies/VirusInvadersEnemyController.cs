using UnityEngine;
using System.Collections;

public class VirusInvadersEnemyController : MonoBehaviour
{
    [Header("Datos del Enemigo")]
    public VirusInvadersEnemyData enemyData;
    
    // Referencias del script de enemigo original
    private SpriteRenderer spriteRenderer;
    private Transform player;
    private Rigidbody2D rb;
    private CircleCollider2D col;
    private IVirusInvadersMovement movementComponent;
    
    // Gestión de estado
    private bool isDead = false;
    private float currentHealth;
    private float lastAttackTime = 0f;
    private EnemyState currentState = EnemyState.Idle;
    private bool isGamePaused = false;
    
    // Sistema de animación (mantenido del original)
    private Sprite[] spritesIdle;
    private Sprite[] spritesPulse;
    private Sprite[] spritesAttack;
    private Sprite[] spritesHit;
    private Sprite[] spritesDeath;
    private Sprite spriteDefault;
    
    private int currentFrame = 0;
    private float frameTime = 0f;
    private Sprite[] currentAnimation;
    private bool animationLoop = true;
    private string currentAnimationState = "";
    
    void Start()
    {
        if (enemyData == null)
        {
            Debug.LogError("Enemy Data is null! Please assign an EnemyData ScriptableObject.");
            return;
        }
        
        SetupComponents();
        CreateDefaultSprite();
        LoadSprites();
        FindPlayer();
        SetupMovementComponent();
        
        // Suscribirse a eventos de pausa
        VirusInvadersGameManager.OnGamePaused += OnGamePaused;
        
        // Suscribirse a eventos de reset  
        VirusInvadersGameManager.OnGameReset += ResetEnemy;
        
        // Aplicar modificadores de dificultad a la salud
        float difficultyHealthMultiplier = 1f;
        if (VirusInvadersGameManager.Instance != null && VirusInvadersGameManager.Instance.difficultyLevels.Length > 0)
        {
            var currentDifficulty = VirusInvadersGameManager.Instance.difficultyLevels[VirusInvadersGameManager.Instance.currentDifficultyLevel];
            difficultyHealthMultiplier = currentDifficulty.enemyHealthMultiplier;
        }
        
        currentHealth = enemyData.health * difficultyHealthMultiplier;
        StartSafeAnimation();
    }
    
    void OnDestroy()
    {
        // Desuscribirse de eventos
        VirusInvadersGameManager.OnGamePaused -= OnGamePaused;
        VirusInvadersGameManager.OnGameReset -= ResetEnemy;
    }
    
    void OnGamePaused(bool paused)
    {
        isGamePaused = paused;
        
        if (paused)
        {
            // Detener movimiento cuando esté pausado
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }
        }
    }
    
    void SetupComponents()
    {
        // Configuración de SpriteRenderer
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        
        spriteRenderer.color = enemyData.enemyColor;
        spriteRenderer.sortingLayerName = enemyData.sortingLayerName;
        spriteRenderer.sortingOrder = enemyData.sortingOrder;
        
        // Configuración de Rigidbody2D
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        
        // Configuración de Collider
        col = GetComponent<CircleCollider2D>();
        if (col == null)
        {
            col = gameObject.AddComponent<CircleCollider2D>();
        }
        if (enemyData.scaleColliderWithSize)
        {
            col.radius = enemyData.colliderRadius * enemyData.scale * enemyData.colliderSizeMultiplier;
        }
        else
        {
            col.radius = enemyData.colliderRadius * enemyData.colliderSizeMultiplier;
        }
        col.isTrigger = true;
        
        // Configuración de escala
        transform.localScale = Vector3.one * enemyData.scale;
        
        // Ajuste de posición Z
        Vector3 pos = transform.position;
        pos.z = -1f;
        transform.position = pos;
        
        // Tags y layers
        gameObject.tag = "Enemy";
        gameObject.layer = LayerMask.NameToLayer("Enemies");
    }
    
    void SetupMovementComponent()
    {
        // Remover cualquier componente de movimiento existente
        var existingMovements = GetComponents<IVirusInvadersMovement>();
        foreach (var movement in existingMovements)
        {
            if (movement is MonoBehaviour mb)
            {
                DestroyImmediate(mb);
            }
        }
        
        // Agregar el componente de movimiento apropiado
        switch (enemyData.movementType)
        {
            case MovementType.Static:
                movementComponent = gameObject.AddComponent<VirusInvadersStaticMovement>();
                break;
            case MovementType.Descend:
                movementComponent = gameObject.AddComponent<VirusInvadersDescendMovement>();
                break;
            case MovementType.Chase:
                movementComponent = gameObject.AddComponent<VirusInvadersChaseMovement>();
                break;
        }
        
        if (movementComponent != null)
        {
            movementComponent.Initialize();
            movementComponent.SetMovementSpeed(enemyData.moveSpeed);
            movementComponent.SetMovementParameters(enemyData.movementParameters);
        }
    }
    
    // Mantener toda la lógica de carga de sprites y animación del script original
    void CreateDefaultSprite()
    {
        Texture2D texture = new Texture2D(64, 64);
        Color[] pixels = new Color[64 * 64];
        Vector2 center = new Vector2(32, 32);
        
        for (int x = 0; x < 64; x++)
        {
            for (int y = 0; y < 64; y++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                
                if (distance <= 28)
                {
                    pixels[y * 64 + x] = new Color(0.8f, 0.3f, 0.3f, 1f);
                }
                else if (distance <= 30)
                {
                    pixels[y * 64 + x] = new Color(0.6f, 0.2f, 0.2f, 1f);
                }
                else
                {
                    pixels[y * 64 + x] = Color.clear;
                }
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        spriteDefault = Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f));
        spriteRenderer.sprite = spriteDefault;
    }
    
    void LoadSprites()
    {
        string virusTypeString = enemyData.GetVirusTypeString();
        
        string[] basePaths = new string[]
        {
            $"VirusInvaders/Coronavirus/virus_spriteanimation/{virusTypeString}/",
            $"Spine/Coronavirus/virus_spriteanimation/{virusTypeString}/",
            $"Coronavirus/virus_spriteanimation/{virusTypeString}/",
            $"virus_spriteanimation/{virusTypeString}/"
        };
        
        foreach (string basePath in basePaths)
        {
            spritesIdle = LoadAnimation(basePath, "idle1");
            if (spritesIdle != null && spritesIdle.Length > 0)
            {
                spritesPulse = LoadAnimation(basePath, "pulse") ?? spritesIdle;
                spritesAttack = LoadAnimation(basePath, "attack") ?? spritesIdle;
                spritesHit = LoadAnimation(basePath, "hit") ?? spritesIdle;
                spritesDeath = LoadAnimation(basePath, "death") ?? spritesIdle;
                return;
            }
        }
        
        // Fallback a sprites por defecto
        spritesIdle = new Sprite[] { spriteDefault };
        spritesPulse = spritesIdle;
        spritesAttack = spritesIdle;
        spritesHit = spritesIdle;
        spritesDeath = spritesIdle;
    }
    
    Sprite[] LoadAnimation(string basePath, string animation)
    {
        System.Collections.Generic.List<Sprite> sprites = new System.Collections.Generic.List<Sprite>();
        string prefix = basePath + $"coronavirus-{enemyData.GetVirusTypeString()}-{animation}_";
        
        for (int i = 0; i < 30; i++)
        {
            Sprite sprite = Resources.Load<Sprite>(prefix + i.ToString("00"));
            if (sprite == null)
            {
                sprite = Resources.Load<Sprite>(prefix + i.ToString());
            }
            
            if (sprite != null)
            {
                sprites.Add(sprite);
            }
            else if (sprites.Count > 0)
            {
                break;
            }
        }
        
        return sprites.Count > 0 ? sprites.ToArray() : null;
    }
    
    void FindPlayer()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                VirusInvadersPlayerController playerController = FindFirstObjectByType<VirusInvadersPlayerController>();
                if (playerController != null)
                {
                    player = playerController.transform;
                }
            }
        }
    }
    
    void Update()
    {
        if (isDead || player == null || isGamePaused) return;
        
        UpdateAnimation();
        UpdateBehavior();
        UpdateMovement();
        LookAtPlayer();
    }
    
    void UpdateMovement()
    {
        if (movementComponent != null && currentState != EnemyState.Attacking)
        {
            movementComponent.UpdateMovement(player);
        }
    }
    
    void UpdateBehavior()
    {
        if (currentState == EnemyState.Hit || currentState == EnemyState.Dying) return;
        
        float distance = Vector2.Distance(transform.position, player.position);
        EnemyState newState = currentState;
        
        if (distance <= enemyData.attackRange)
        {
            newState = EnemyState.Attacking;
        }
        else if (distance <= enemyData.detectionRange)
        {
            newState = EnemyState.Moving;
        }
        else
        {
            newState = EnemyState.Idle;
        }
        
        if (newState != currentState)
        {
            currentState = newState;
            
            switch (currentState)
            {
                case EnemyState.Idle:
                    ChangeAnimation(spritesIdle, true, "idle");
                    break;
                case EnemyState.Moving:
                    ChangeAnimation(spritesPulse, true, "pulse");
                    break;
                case EnemyState.Attacking:
                    ChangeAnimation(spritesAttack, false, "attack");
                    break;
            }
        }
        
        // Manejar ataques
        if (currentState == EnemyState.Attacking)
        {
            if (rb != null) rb.linearVelocity = Vector2.zero;
            
            if (Time.time - lastAttackTime >= enemyData.timeBetweenAttacks)
            {
                StartCoroutine(ExecuteAttack());
                lastAttackTime = Time.time;
            }
        }
    }
    
    // Mantener todos los métodos de animación del script original
    void UpdateAnimation()
    {
        if (currentAnimation == null || currentAnimation.Length == 0) 
        {
            spriteRenderer.sprite = spriteDefault;
            return;
        }
        
        frameTime += Time.deltaTime;
        
        if (frameTime >= enemyData.animationSpeed)
        {
            currentFrame++;
            
            if (currentFrame >= currentAnimation.Length)
            {
                if (animationLoop)
                {
                    currentFrame = 0;
                }
                else
                {
                    currentFrame = currentAnimation.Length - 1;
                }
            }
            
            if (currentFrame >= 0 && currentFrame < currentAnimation.Length && currentAnimation[currentFrame] != null)
            {
                spriteRenderer.sprite = currentAnimation[currentFrame];
            }
            else
            {
                spriteRenderer.sprite = spriteDefault;
            }
            
            frameTime = 0f;
        }
    }
    
    void ChangeAnimation(Sprite[] newAnimation, bool loop, string name)
    {
        if (currentAnimationState == name && currentAnimation == newAnimation) return;
        
        if (newAnimation != null && newAnimation.Length > 0)
        {
            currentAnimation = newAnimation;
            animationLoop = loop;
            currentFrame = 0;
            frameTime = 0f;
            currentAnimationState = name;
            
            if (currentAnimation[0] != null)
            {
                spriteRenderer.sprite = currentAnimation[0];
            }
            else
            {
                spriteRenderer.sprite = spriteDefault;
            }
        }
    }
    
    void StartSafeAnimation()
    {
        if (spritesIdle != null && spritesIdle.Length > 0)
        {
            ChangeAnimation(spritesIdle, true, "idle");
        }
        else
        {
            spriteRenderer.sprite = spriteDefault;
        }
    }
    
    IEnumerator ExecuteAttack()
    {
        // *** PASO 1: Dar tiempo para que la animación se establezca correctamente ***
        yield return new WaitForEndOfFrame(); // Esperar al final del frame actual
        yield return new WaitForSeconds(enemyData.animationSpeed * 0.5f); // Esperar medio frame extra
        
        // *** PASO 2: Esperar el tiempo dramático antes del daño ***
        // Esperamos 2 frames adicionales para que el jugador vea venir el ataque
        float tiempoAntesDelDaño = 2f * enemyData.animationSpeed; // 2 frames = 0.3s típicamente
        
        yield return new WaitForSeconds(tiempoAntesDelDaño);
        
        // *** VERIFICAR QUE EL JUGADOR SIGUE EN RANGO DE ATAQUE ***
        if (player != null && !isDead)
        {
            float distanciaFinal = Vector2.Distance(transform.position, player.position);
            if (distanciaFinal <= enemyData.attackRange)
            {
                VirusInvadersPlayerController playerController = player.GetComponent<VirusInvadersPlayerController>();
                if (playerController != null)
                {
                    Vector2 knockbackDirection = (player.position - transform.position).normalized;
                    playerController.RecibirDaño(knockbackDirection, enemyData.attackDamage);
                }
            }
        }
    }
    
    void LookAtPlayer()
    {
        if (player == null || enemyData == null) return;
        
        float targetScale = enemyData.scale;
        
        if (player.position.x < transform.position.x)
        {
            transform.localScale = new Vector3(-targetScale, targetScale, 1);
        }
        else
        {
            transform.localScale = new Vector3(targetScale, targetScale, 1);
        }
    }
    
    public void TakeDamage(float amount)
    {
        if (isDead) return;
        
        currentHealth -= amount;
        StartCoroutine(DamageEffect());
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    IEnumerator DamageEffect()
    {
        EnemyState previousState = currentState;
        currentState = EnemyState.Hit;
        ChangeAnimation(spritesHit, false, "hit");
        
        yield return new WaitForSeconds(0.3f);
        
        if (!isDead)
        {
            currentState = previousState;
        }
    }
    
    void Die()
    {
        isDead = true;
        currentState = EnemyState.Dying;
        col.enabled = false;
        rb.simulated = false;
        
        ChangeAnimation(spritesDeath, false, "death");
        
        // Notificar al GameManager con la posición del enemigo para números flotantes
        if (VirusInvadersGameManager.Instance != null)
        {
            VirusInvadersGameManager.Instance.EnemyDefeated(enemyData, transform.position);
        }
        
        StartCoroutine(DeathAnimation());
    }
    
    IEnumerator DeathAnimation()
    {
        yield return new WaitForSeconds(1f);
        
        // Fade out
        float time = 0f;
        Color initialColor = spriteRenderer.color;
        
        while (time < 0.5f)
        {
            time += Time.deltaTime;
            Color color = initialColor;
            color.a = Mathf.Lerp(1f, 0f, time / 0.5f);
            spriteRenderer.color = color;
            yield return null;
        }
        
        Destroy(gameObject);
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // *** REMOVIDO: No hacer daño aquí, solo durante la animación de ataque ***
            // El daño se controla completamente a través del sistema de ataque con animación
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (enemyData == null) return;
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, enemyData.detectionRange);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, enemyData.attackRange);
    }
    
    // Método público para actualizar datos del enemigo (para cambios de dificultad)
    public void UpdateEnemyData(VirusInvadersEnemyData newData)
    {
        enemyData = newData;
        
        // Actualizar propiedades visuales
        spriteRenderer.color = enemyData.enemyColor;
        transform.localScale = Vector3.one * enemyData.scale;
        
        // Actualizar tamaño del collider
        if (col != null)
        {
            if (enemyData.scaleColliderWithSize)
            {
                col.radius = enemyData.colliderRadius * enemyData.scale * enemyData.colliderSizeMultiplier;
            }
            else
            {
                col.radius = enemyData.colliderRadius * enemyData.colliderSizeMultiplier;
            }
        }
        
        // Actualizar componente de movimiento con modificadores de dificultad
        if (movementComponent != null)
        {
            float difficultySpeedMultiplier = 1f;
            if (VirusInvadersGameManager.Instance != null && VirusInvadersGameManager.Instance.difficultyLevels.Length > 0)
            {
                var currentDifficulty = VirusInvadersGameManager.Instance.difficultyLevels[VirusInvadersGameManager.Instance.currentDifficultyLevel];
                difficultySpeedMultiplier = currentDifficulty.enemySpeedMultiplier;
            }
            
            movementComponent.SetMovementSpeed(enemyData.moveSpeed * difficultySpeedMultiplier);
            movementComponent.SetMovementParameters(enemyData.movementParameters);
        }
        
        // Recargar sprites si el tipo de virus cambió
        LoadSprites();
        StartSafeAnimation();
    }
    
    void ResetEnemy()
    {
        if (enemyData == null)
        {
            return;
        }
        
        // *** 1. DETENER TODAS LAS CORRUTINAS ***
        StopAllCoroutines();
        
        // *** 2. RESETEAR ESTADO ***
        isDead = false;
        currentState = EnemyState.Idle;
        isGamePaused = false; // Gestionado por GameManager
        lastAttackTime = 0f;
        
        // *** 3. RESETEAR SALUD ***
        float difficultyHealthMultiplier = 1f;
        if (VirusInvadersGameManager.Instance != null && VirusInvadersGameManager.Instance.difficultyLevels.Length > 0)
        {
            var currentDifficulty = VirusInvadersGameManager.Instance.difficultyLevels[VirusInvadersGameManager.Instance.currentDifficultyLevel];
            difficultyHealthMultiplier = currentDifficulty.enemyHealthMultiplier;
        }
        currentHealth = enemyData.health * difficultyHealthMultiplier;
        
        // *** 4. RESETEAR COMPONENTES FÍSICOS ***
        if (rb != null)
        {
            rb.simulated = true;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        
        if (col != null)
        {
            col.enabled = true;
        }
        
        // *** 5. RESETEAR COMPONENTES VISUALES ***
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
            spriteRenderer.color = enemyData.enemyColor;
        }
        
        // *** 6. RESETEAR TRANSFORM ***
        transform.localScale = Vector3.one * enemyData.scale;
        transform.rotation = Quaternion.identity;
        
        // *** 7. RESETEAR ANIMACIONES ***
        currentFrame = 0;
        frameTime = 0f;
        currentAnimation = null;
        animationLoop = true;
        currentAnimationState = "";
        
        // *** 8. RECARGAR Y REINICIAR ANIMACIONES ***
        LoadSprites();
        StartSafeAnimation();
        
        // *** 9. RE-BUSCAR JUGADOR ***
        player = null;
        FindPlayer();
        
        // *** 10. RESETEAR COMPONENTE DE MOVIMIENTO ***
        if (movementComponent != null)
        {
            float difficultySpeedMultiplier = 1f;
            if (VirusInvadersGameManager.Instance != null && VirusInvadersGameManager.Instance.difficultyLevels.Length > 0)
            {
                var currentDifficulty = VirusInvadersGameManager.Instance.difficultyLevels[VirusInvadersGameManager.Instance.currentDifficultyLevel];
                difficultySpeedMultiplier = currentDifficulty.enemySpeedMultiplier;
            }
            
            movementComponent.SetMovementSpeed(enemyData.moveSpeed * difficultySpeedMultiplier);
            movementComponent.SetMovementParameters(enemyData.movementParameters);
        }
        
        // *** 11. ASEGURAR TAMAÑO CORRECTO DEL COLLIDER ***
        if (col != null)
        {
            if (enemyData.scaleColliderWithSize)
            {
                col.radius = enemyData.colliderRadius * enemyData.scale * enemyData.colliderSizeMultiplier;
            }
            else
            {
                col.radius = enemyData.colliderRadius * enemyData.colliderSizeMultiplier;
            }
        }
    }
}
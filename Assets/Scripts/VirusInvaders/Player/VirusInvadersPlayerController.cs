using UnityEngine;

[System.Serializable]
public class VirusInvadersPlayerController : MonoBehaviour
{
    [Header("VirusInvaders - Movement Configuration")]
    public float velocidadMovimiento = 5f;
    
    [Header("VirusInvaders - Physics Configuration")]
    public bool usarRigidbody = true;
    public float gravedad = -9.8f;
    
    [Header("VirusInvaders - Visual Configuration")]
    public float escalaJugador = 0.3f;
    
    [Header("VirusInvaders - Shooting Configuration")]
    public bool habilitarDisparo = true;
    
    [Header("VirusInvaders - Health System")]
    public float vidaMaxima = 100f;
    public bool crearBarraDeVida = true;
    public bool usarBarraUI = true; // true = UI Canvas, false = World Space
    public Vector3 posicionBarraRelativa = new Vector3(0, 0.25f, 0);
    
    // Referencias privadas
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private VirusInvadersPlayerShooter shooter;
    private VirusInvadersHealthBar barraVida;
    private float vidaActual;
    private float inputHorizontal;
    private float velocidadVertical;
    private bool estaMuerto = false;
    private bool isGamePaused = false;
    
    // Eventos
    public System.Action OnPlayerDeath;
    public System.Action<float> OnHealthChanged;
    
    void Start()
    {

        
        ConfigurarComponentes();
        ConfigurarFisicas();
        ConfigurarDisparo();
        ConfigurarSistemaVida();
        
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
        
        if (paused)
        {
            // Detener movimiento cuando esté pausado
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }
        }
    }
    
    void ConfigurarComponentes()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        
        transform.localScale = new Vector3(escalaJugador, escalaJugador, 1);
        
        if (usarRigidbody)
        {
            rb = GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody2D>();
            }
            
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.gravityScale = 1f;
            rb.linearDamping = 0f;
            rb.angularDamping = 0.05f;
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }
    
    void ConfigurarFisicas()
    {
        // Configuración de física sin detección de suelo
    }
    
    void ConfigurarDisparo()
    {
        if (habilitarDisparo)
        {
            shooter = GetComponent<VirusInvadersPlayerShooter>();
            if (shooter == null)
            {
                shooter = gameObject.AddComponent<VirusInvadersPlayerShooter>();
            }
        }
    }
    
    void ConfigurarSistemaVida()
    {
        vidaActual = vidaMaxima;
        
        if (crearBarraDeVida)
        {
            CrearBarraDeVida();
        }
    }
    
    void CrearBarraDeVida()
    {
        if (usarBarraUI)
        {
            CrearBarraUI();
        }
        else
        {
            CrearBarraWorldSpace();
        }
    }
    
    void CrearBarraUI()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("VirusInvaders: No se encontró Canvas. Creando uno.");
            GameObject canvasGO = new GameObject("VirusInvaders_Canvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }
        
        GameObject barraGO = new GameObject("VirusInvaders_PlayerHealthBar_UI");
        barraGO.transform.SetParent(canvas.transform, false);
        
        RectTransform rt = barraGO.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.05f, 0.05f);
        rt.anchorMax = new Vector2(0.35f, 0.1f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        
        UnityEngine.UI.Image imagen = barraGO.AddComponent<UnityEngine.UI.Image>();
        
        Sprite barSprite = Resources.Load<Sprite>("VirusInvaders/Sprites/blood_red_bar");
        if (barSprite != null)
        {
            imagen.sprite = barSprite;
        }
        else
        {
            Debug.LogWarning("VirusInvaders: No se encontró blood_red_bar.png en Resources/VirusInvaders/Sprites/!");
        }
        
        barraVida = barraGO.AddComponent<VirusInvadersHealthBar>();
        barraVida.useImageComponent = true;
        barraVida.maxHealth = vidaMaxima;
        barraVida.currentHealth = vidaActual;
    }
    
    void CrearBarraWorldSpace()
    {
        Vector3 posicionBarra = transform.position + posicionBarraRelativa;
        GameObject barraGO = new GameObject("VirusInvaders_PlayerHealthBar_World");
        barraGO.transform.position = posicionBarra;
        barraGO.transform.SetParent(transform);
        
        SpriteRenderer sr = barraGO.AddComponent<SpriteRenderer>();
        
        Sprite barSprite = Resources.Load<Sprite>("VirusInvaders/Sprites/blood_red_bar");
        if (barSprite != null)
        {
            sr.sprite = barSprite;
        }
        else
        {
            Debug.LogWarning("VirusInvaders: No se encontró blood_red_bar.png en Resources/VirusInvaders/Sprites/!");
        }
        
        sr.sortingLayerName = "UI";
        sr.sortingOrder = 100;
        
        barraGO.transform.localScale = new Vector3(0.15f, 0.03f, 1f);
        
        barraVida = barraGO.AddComponent<VirusInvadersHealthBar>();
        barraVida.useImageComponent = false;
        barraVida.maxHealth = vidaMaxima;
        barraVida.currentHealth = vidaActual;
    }

    void Update()
    {
        if (estaMuerto || isGamePaused) return;
        
        inputHorizontal = Input.GetAxisRaw("Horizontal");
        
        // Sprite flipping
        if (spriteRenderer != null)
        {
            if (inputHorizontal > 0.1f)
            {
                spriteRenderer.flipX = false;
            }
            else if (inputHorizontal < -0.1f)
            {
                spriteRenderer.flipX = true;
            }
        }
        
        transform.localScale = new Vector3(escalaJugador, escalaJugador, 1);
        
        if (!usarRigidbody)
        {
            MoverSinRigidbody();
        }
        
        // Controles de debug de salud
        if (Input.GetKeyDown(KeyCode.H))
        {
    
            RecibirDaño(10f);
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
    
            Curar(15f);
        }
    }
    
    void FixedUpdate()
    {
        if (usarRigidbody && rb != null && !estaMuerto && !isGamePaused)
        {
            Mover();
        }
    }
    
    void Mover()
    {
        rb.linearVelocity = new Vector2(inputHorizontal * velocidadMovimiento, rb.linearVelocity.y);
    }
    
    void MoverSinRigidbody()
    {
        Vector3 movimiento = new Vector3(inputHorizontal * velocidadMovimiento * Time.deltaTime, 0, 0);
        transform.Translate(movimiento);
    }
    
    // === SISTEMA DE SALUD ===
    
    public void RecibirDaño(float daño)
    {
                if (estaMuerto)
        {
            return;
        }
        
        float vidaAnterior = vidaActual;
        vidaActual = Mathf.Clamp(vidaActual - daño, 0f, vidaMaxima);
        

        
        if (barraVida != null)
        {
            barraVida.SetHealth(vidaActual);
        }
        
        OnHealthChanged?.Invoke(vidaActual / vidaMaxima);
        
        if (vidaActual <= 0f && !estaMuerto)
        {

            Morir();
        }
    }
    
    public void Curar(float cantidadCuracion)
    {
        if (estaMuerto) return;
        
        float vidaAnterior = vidaActual;
        vidaActual = Mathf.Clamp(vidaActual + cantidadCuracion, 0f, vidaMaxima);
        
        if (barraVida != null)
        {
            barraVida.SetHealth(vidaActual);
        }
        
        OnHealthChanged?.Invoke(vidaActual / vidaMaxima);
    }
    
    public void EstablecerVidaMaxima(float nuevaVidaMaxima)
    {
        vidaMaxima = Mathf.Max(1f, nuevaVidaMaxima);
        vidaActual = Mathf.Clamp(vidaActual, 0f, vidaMaxima);
        
        if (barraVida != null)
        {
            barraVida.SetMaxHealth(vidaMaxima);
        }
    }
    
    void Morir()
    {
        if (estaMuerto) 
        {
            return;
        }
        
        estaMuerto = true;
        
        OnPlayerDeath?.Invoke();
        
        // Desactivar controles y física
        enabled = false;
        if (rb != null)
        {
            rb.simulated = false;
            rb.linearVelocity = Vector2.zero;
        }
    }
    
    // Métodos públicos de consulta
    public float GetVidaActual() => vidaActual;
    public float GetVidaMaxima() => vidaMaxima;
    public float GetPorcentajeVida() => vidaActual / vidaMaxima;
    public bool EstaVivo() => !estaMuerto;
    public bool TieneVidaCompleta() => Mathf.Approximately(vidaActual, vidaMaxima);
    
    // Método para reiniciar jugador al estado inicial
    public void ResetPlayer()
    {
        // Reiniciar posición a la posición inicial de la escena
        Vector3 initialPosition = new Vector3(-0.005f, -3.6153f, 0f);
        transform.position = initialPosition;
        
        // Reiniciar salud
        vidaActual = vidaMaxima;
        estaMuerto = false;
        OnHealthChanged?.Invoke(vidaActual / vidaMaxima);
        
        // Reiniciar física
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        velocidadVertical = 0f;
        
        // Reiniciar entrada
        inputHorizontal = 0f;
        
        // Actualizar barra de vida si existe
        if (barraVida != null)
        {
            barraVida.SetHealth(vidaActual);
        }
        
        // Reactivar componentes del jugador
        enabled = true;
        if (rb != null)
        {
            rb.simulated = true;
        }
        
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
        
        // Habilitar disparo del jugador
        if (shooter != null)
        {
            shooter.enabled = true;
        }
        
        // NOTA: isGamePaused es controlado por GameManager, no aquí
        

    }
    
    // Sobrecarga con knockback - método de compatibilidad
    public void RecibirDaño(Vector2 direccionKnockback, float fuerzaKnockback)
    {
        RecibirDaño(10f); // Cantidad de daño estándar
        
        if (rb != null)
        {
            rb.AddForce(direccionKnockback.normalized * fuerzaKnockback, ForceMode2D.Impulse);
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (crearBarraDeVida && !usarBarraUI)
        {
            Gizmos.color = Color.green;
            Vector3 posicionBarra = transform.position + posicionBarraRelativa;
            Gizmos.DrawWireCube(posicionBarra, new Vector3(1f, 0.1f, 0f));
        }
    }
} 
using UnityEngine;
using System.Collections;

[System.Serializable]
public class VirusInvadersMedKit : MonoBehaviour
{
    [Header("VirusInvaders - Configuración del MedKit")]
    public float velocidadCaida = 2f;
    public float cantidadCuracion = 30f;
    public float tiempoVida = 15f;
    
    [Header("VirusInvaders - Efectos Visuales")]
    public bool parpadeaAlCaer = true;
    public float velocidadParpadeo = 3f;
    public bool rotarAlCaer = true;
    public float velocidadRotacion = 50f;
    
    // Referencias privadas
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private CircleCollider2D col;
    private bool fueRecogido = false;
    private float tiempoParpadeo = 0f;
    private bool esVisible = true;
    
    // Valores originales para animación
    private Vector3 escalaOriginal;
    private Color colorOriginal;
    
    void Start()
    {
        ConfigurarComponentes();
        InicializarBotiquin();
        IniciarAnimaciones();
        ConfigurarDestruccionAutomatica();
    }
    
    void ConfigurarComponentes()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.down * velocidadCaida;
        
        col = GetComponent<CircleCollider2D>();
        if (col == null)
        {
            col = gameObject.AddComponent<CircleCollider2D>();
        }
        col.radius = 0.3f;
        col.isTrigger = true;
        
        try
        {
            gameObject.tag = "MedKit";
        }
        catch (UnityException)
        {
            // Si el tag MedKit no existe, usar el predeterminado
            gameObject.tag = "Untagged";
            Debug.LogWarning("VirusInvaders: MedKit tag not found. Please create 'MedKit' tag in Project Settings > Tags and Layers");
        }
        gameObject.layer = LayerMask.NameToLayer("Default");
        
        transform.localScale = Vector3.one * 0.15f; // Tamaño pequeño de píldora
        
        spriteRenderer.sortingLayerName = "Characters";
        spriteRenderer.sortingOrder = 15;
    }
    
    void InicializarBotiquin()
    {
        if (cantidadCuracion <= 0f)
        {
            cantidadCuracion = 30f;
        }
        
        Sprite medSprite = Resources.Load<Sprite>("VirusInvaders/Sprites/Medicine");
        if (medSprite != null)
        {
            spriteRenderer.sprite = medSprite;
        }
        else
        {
            Debug.LogWarning("VirusInvaders: No se encontró Medicine.png en Resources/VirusInvaders/Sprites/!");
            CrearSpriteDefault();
        }
        
        escalaOriginal = transform.localScale;
        colorOriginal = spriteRenderer.color;
    }
    
    void CrearSpriteDefault()
    {
        // Crear sprite básico de cruz roja como respaldo
        Texture2D textura = new Texture2D(32, 32);
        Color[] pixeles = new Color[32 * 32];
        
        for (int x = 0; x < 32; x++)
        {
            for (int y = 0; y < 32; y++)
            {
                if (x >= 12 && x <= 19)
                {
                    pixeles[y * 32 + x] = Color.red;
                }
                else if (y >= 12 && y <= 19)
                {
                    pixeles[y * 32 + x] = Color.red;
                }
                else
                {
                    pixeles[y * 32 + x] = Color.clear;
                }
            }
        }
        
        textura.SetPixels(pixeles);
        textura.Apply();
        
        Sprite spriteDefault = Sprite.Create(textura, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
        spriteRenderer.sprite = spriteDefault;
    }
    
    void IniciarAnimaciones()
    {
        if (parpadeaAlCaer || rotarAlCaer)
        {
            StartCoroutine(EfectosVisuales());
        }
    }
    
    void ConfigurarDestruccionAutomatica()
    {
        Destroy(gameObject, tiempoVida);
    }
    
    void Update()
    {
        if (fueRecogido) return;
        
        if (rb != null)
        {
            rb.linearVelocity = Vector2.down * velocidadCaida;
        }
    }
    
    IEnumerator EfectosVisuales()
    {
        while (!fueRecogido)
        {
            if (parpadeaAlCaer)
            {
                tiempoParpadeo += Time.deltaTime * velocidadParpadeo;
                
                esVisible = Mathf.Sin(tiempoParpadeo) > 0f;
                Color nuevoColor = colorOriginal;
                nuevoColor.a = esVisible ? 1f : 0.7f;
                spriteRenderer.color = nuevoColor;
                
                // Efecto de brillo
                float brillo = (Mathf.Sin(tiempoParpadeo * 2f) + 1f) * 0.1f;
                nuevoColor.r = Mathf.Clamp01(colorOriginal.r + brillo);
                nuevoColor.g = Mathf.Clamp01(colorOriginal.g + brillo);
                nuevoColor.b = Mathf.Clamp01(colorOriginal.b + brillo);
                spriteRenderer.color = nuevoColor;
            }
            
            if (rotarAlCaer)
            {
                transform.Rotate(0, 0, velocidadRotacion * Time.deltaTime);
            }
            
            yield return null;
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (fueRecogido) return;
        
        VirusInvadersPlayerController jugador = other.GetComponent<VirusInvadersPlayerController>();
        if (jugador != null)
        {
            if (VirusInvadersAudioManager.Instance != null)
            {
                VirusInvadersAudioManager.Instance.ReproducirMedKit();
            }
            
            float curacionReal = cantidadCuracion;
            jugador.Curar(curacionReal);
            
            StartCoroutine(AnimacionRecogida());
        }
    }
    
    IEnumerator AnimacionRecogida()
    {
        fueRecogido = true;
        col.enabled = false;
        rb.simulated = false;
        
        // Efecto de recogida - escalar hacia arriba y desvanecer
        float duracion = 0.5f;
        float tiempo = 0f;
        
        Vector3 escalaFinal = escalaOriginal * 1.5f;
        Color colorFinal = colorOriginal;
        colorFinal.a = 0f;
        
        while (tiempo < duracion)
        {
            float progreso = tiempo / duracion;
            
            transform.localScale = Vector3.Lerp(escalaOriginal, escalaFinal, progreso);
            spriteRenderer.color = Color.Lerp(colorOriginal, colorFinal, progreso);
            
            tiempo += Time.deltaTime;
            yield return null;
        }
        
        Destroy(gameObject);
    }
    
    void OnBecameInvisible()
    {
        if (gameObject != null)
        {
            StartCoroutine(DestruirPorSalirDePantalla());
        }
    }
    
    IEnumerator DestruirPorSalirDePantalla()
    {
        // Esperar para confirmar que realmente está fuera de pantalla
        yield return new WaitForSeconds(2f);
        
        if (gameObject != null)
        {
            Camera camara = Camera.main;
            if (camara != null)
            {
                Vector3 posicionPantalla = camara.WorldToViewportPoint(transform.position);
                
                if (posicionPantalla.x < -0.1f || posicionPantalla.x > 1.1f || 
                    posicionPantalla.y < -0.1f || posicionPantalla.y > 1.1f)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
    
    // Métodos públicos de configuración
    public void SetCantidadCuracion(float cantidad)
    {
        cantidadCuracion = Mathf.Max(cantidad, 1f);
    }
    
    public void SetVelocidadCaida(float velocidad)
    {
        velocidadCaida = Mathf.Max(velocidad, 0.1f);
        if (rb != null)
        {
            rb.linearVelocity = Vector2.down * velocidadCaida;
        }
    }
    
    public void SetTiempoVida(float tiempo)
    {
        tiempoVida = Mathf.Max(tiempo, 1f);
    }
    
    // Método factory estático
    public static GameObject CrearBotiquin(Vector3 posicion, float curacion = 30f)
    {
        GameObject botiquinGO = new GameObject("VirusInvadersMedKit");
        botiquinGO.transform.position = posicion;
        
        VirusInvadersMedKit botiquin = botiquinGO.AddComponent<VirusInvadersMedKit>();
        botiquin.cantidadCuracion = curacion;
        
        return botiquinGO;
    }
}
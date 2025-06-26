using UnityEngine;

public class VirusInvadersBullet : MonoBehaviour
{
    [Header("VirusInvaders - Syringe Configuration")]
    public float velocidad = 15f;
    public float daño = 50f;
    public float tiempoVida = 5f;
    public float radioDañoColision = 0.4f; // Radio de colisión más grande para facilitar el impacto
    
    [Header("VirusInvaders - Impact Effects")]
    public bool createExplosionOnHit = true;
    public float explosionScale = 0.2f; // Reducido significativamente de 0.5f a 0.2f
    
    private Vector2 direccion = Vector2.up; // Siempre hacia arriba
    private bool configurada = false;
    
    void Start()
    {
        // La jeringuilla debe moverse hacia arriba inmediatamente
        ConfigurarJeringuilla();
        
        // Registrar disparo con GameManager
        if (VirusInvadersGameManager.Instance != null)
        {
            VirusInvadersGameManager.Instance.RegisterShot();
        }
        
        // Auto-destruir después del tiempo de vida
        Destroy(gameObject, tiempoVida);
    }
    
    void ConfigurarJeringuilla()
    {
        // Asegurar que tenemos Rigidbody2D
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        
        // Configurar física
        rb.gravityScale = 0f;
        rb.linearVelocity = direccion * velocidad;
        rb.angularVelocity = 0f;
        
        // Asegurar que tenemos collider
        CircleCollider2D col = GetComponent<CircleCollider2D>();
        if (col == null)
        {
            col = gameObject.AddComponent<CircleCollider2D>();
            col.radius = radioDañoColision; // Usar variable configurable
            col.isTrigger = true;
        }
        else
        {
            col.radius = radioDañoColision; // Actualizar si ya existe
        }
        
        // Configurar Tag y Layer
        gameObject.tag = "Bullet";
        gameObject.layer = LayerMask.NameToLayer("Default");
        
        // Rotar para apuntar hacia arriba (jeringuilla vertical)
        transform.rotation = Quaternion.AngleAxis(180f, Vector3.forward);
        
        configurada = true;
    }
    
    public void ConfigurarSprite(Sprite sprite)
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            sr = gameObject.AddComponent<SpriteRenderer>();
        }
        
        if (sprite != null)
        {
            sr.sprite = sprite;
        }
        else
        {
            // Crear sprite cian temporal
            Texture2D texture = new Texture2D(4, 12);
            Color[] pixels = new Color[4 * 12];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.cyan;
            }
            texture.SetPixels(pixels);
            texture.Apply();
            
            sr.sprite = Sprite.Create(texture, new Rect(0, 0, 4, 12), new Vector2(0.5f, 0.5f));
        }
        
        // Configurar ordenamiento
        sr.sortingLayerName = "Default";
        sr.sortingOrder = 15;
    }
    
    void Update()
    {
        // Asegurar que la jeringuilla siga moviéndose hacia arriba
        if (configurada)
        {
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null && rb.linearVelocity.y < velocidad * 0.5f)
            {
                rb.linearVelocity = direccion * velocidad;
            }
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {

        bool hitSomething = false;
        
        // Verificar si golpeó un enemigo
        if (other.CompareTag("Enemy"))
        {
            hitSomething = true;
            
            // Registrar impacto con GameManager
            if (VirusInvadersGameManager.Instance != null)
            {
                VirusInvadersGameManager.Instance.RegisterHit();
            }
            
            // Primero intentar el sistema nuevo EnemyController
            VirusInvadersEnemyController enemyController = other.GetComponent<VirusInvadersEnemyController>();
            if (enemyController != null)
            {
                enemyController.TakeDamage(daño);
            }
            else
            {
                // Recurrir al sistema de enemigo antiguo para compatibilidad
                VirusInvadersCoronavirusEnemy enemigo = other.GetComponent<VirusInvadersCoronavirusEnemy>();
                if (enemigo != null)
                {
                    enemigo.RecibirDaño(daño);
                }
            }
        }
        
        // Verificar paredes (destruir al contacto con bordes)
        if (other.gameObject.layer == LayerMask.NameToLayer("Environment"))
        {
            hitSomething = true;
        }
        
        // Crear efecto de explosión en punto de impacto (solo una vez)
        if (hitSomething && createExplosionOnHit)
        {
            float scale = other.CompareTag("Enemy") ? explosionScale : explosionScale * 0.7f;
            VirusInvadersBoomEffect.CreateExplosion(transform.position, scale);
        }
        
        // Destruir bala después del impacto
        if (hitSomething)
        {
            Destroy(gameObject);
        }
    }
    
    void OnBecameInvisible()
    {
        // Destruir cuando esté fuera de pantalla
        Destroy(gameObject);
    }
}

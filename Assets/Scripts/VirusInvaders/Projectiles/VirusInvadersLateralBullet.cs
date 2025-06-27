using UnityEngine;

public class VirusInvadersLateralBullet : MonoBehaviour
{
    [Header("VirusInvaders - Lateral Bullet Configuration")]
    public float velocidad = 15f;
    public float daño = 50f;
    public float tiempoVida = 5f;
    public float radioDañoColision = 0.4f;
    
    [Header("VirusInvaders - Impact Effects")]
    public bool createExplosionOnHit = true;
    public float explosionScale = 0.2f;
    
    private Vector2 direccion = Vector2.up;
    private bool configurada = false;
    private bool estaClavada = false;
    
    public void ConfigurarBala(Vector2 direccionMovimiento, float vel, float damage, Sprite sprite)
    {
        direccion = direccionMovimiento.normalized;
        velocidad = vel;
        daño = damage;
        
        ConfigurarComponentes();
        ConfigurarSprite(sprite);
        
        // Registrar disparo con GameManager
        if (VirusInvadersGameManager.Instance != null)
        {
            VirusInvadersGameManager.Instance.RegisterShot();
        }
        
        // Auto-destroy after lifetime
        Destroy(gameObject, tiempoVida);
    }
    
    void ConfigurarComponentes()
    {
        // Configurar Rigidbody2D
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        
        rb.gravityScale = 0f;
        rb.linearVelocity = direccion * velocidad;
        rb.angularVelocity = 0f;
        
        // Configurar Collider
        CircleCollider2D col = GetComponent<CircleCollider2D>();
        if (col == null)
        {
            col = gameObject.AddComponent<CircleCollider2D>();
            col.radius = radioDañoColision;
            col.isTrigger = true;
        }
        else
        {
            col.radius = radioDañoColision;
        }
        
        // Configurar Tag y Layer
        gameObject.tag = "Bullet";
        gameObject.layer = LayerMask.NameToLayer("Default");
        
        // Rotar hacia la dirección de movimiento
        if (direccion != Vector2.zero)
        {
            float angle = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle + 90f, Vector3.forward);
        }
        
        configurada = true;
    }
    
    void ConfigurarSprite(Sprite sprite)
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
            // Crear sprite temporal
            Texture2D texture = new Texture2D(4, 12);
            Color[] pixels = new Color[4 * 12];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.yellow;
            }
            texture.SetPixels(pixels);
            texture.Apply();
            
            sr.sprite = Sprite.Create(texture, new Rect(0, 0, 4, 12), new Vector2(0.5f, 0.5f));
        }
        
        sr.sortingLayerName = "Default";
        sr.sortingOrder = 15;
    }
    
    void Update()
    {
        if (configurada && !estaClavada)
        {
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // Mantener velocidad constante
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
            
            // Reproducir sonido de impacto usando AudioManager
            if (VirusInvadersAudioManager.Instance != null)
            {
                VirusInvadersAudioManager.Instance.ReproducirSonidoImpacto();
            }
            
            // Registrar impacto con GameManager
            if (VirusInvadersGameManager.Instance != null)
            {
                VirusInvadersGameManager.Instance.RegisterHit();
            }
            
            // Crear efecto de explosión al golpear enemigo
            if (createExplosionOnHit)
            {
                VirusInvadersBoomEffect.CreateExplosion(transform.position, explosionScale);
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
            
            // Destruir bala después del impacto
            Destroy(gameObject);
            return;
        }
        
        // Verificar paredes - múltiples opciones de detección
        bool esPared = false;
        
        // Opción 1: Por Layer "Environment"
        if (other.gameObject.layer == LayerMask.NameToLayer("Environment"))
        {
            esPared = true;
        }
        
        // Opción 2: Por Tag "Wall" (si las paredes usan este tag)
        if (other.CompareTag("Wall"))
        {
            esPared = true;
        }
        
        // Opción 3: Por nombre del GameObject (para máxima compatibilidad)
        string nombreObjeto = other.gameObject.name.ToLower();
        if (nombreObjeto.Contains("wall") || nombreObjeto.Contains("pared") || 
            nombreObjeto.Contains("border") || nombreObjeto.Contains("limite") ||
            nombreObjeto.Contains("barrier") || nombreObjeto.Contains("barra"))
        {
            esPared = true;
        }
        
        if (esPared)
        {
            ClavarBalaEnPared();
            return;
        }
    }
    
    void ClavarBalaEnPared()
    {
        if (estaClavada) return;
        
        estaClavada = true;
        
        // Detener completamente el movimiento
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
        
        // Cambiar el color para indicar que está clavada
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = Color.gray;
        }
    }
    
    void OnBecameInvisible()
    {
        if (!estaClavada)
        {
            Destroy(gameObject);
        }
    }
} 
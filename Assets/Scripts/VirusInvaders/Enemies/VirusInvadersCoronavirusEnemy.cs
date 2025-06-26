using UnityEngine;
using System.Collections;

[System.Serializable]
public class VirusInvadersCoronavirusEnemy : MonoBehaviour
{
    [Header("VirusInvaders - Configuración del Enemigo")]
    public float velocidadMovimiento = 2f;
    public float velocidadDescenso = 1f;
    public float vida = 100f;
    public float distanciaDeteccion = 1.5f;
    public float distanciaAtaque = 1.5f;
    public float dañoAtaque = 25f;
    public float tiempoEntreAtaques = 2f;
    
    [Header("VirusInvaders - Configuración Visual")]
    public float velocidadAnimacion = 0.15f;
    public Color colorEnemigo = Color.white;
    public string sortingLayerName = "Default";
    public int sortingOrder = 10;
    
    [Header("VirusInvaders - Configuración de Animación")]
    public string tipoCoronavirus = "classic"; // classic, green, blue-rim-light, red-rim-light
    
    [Header("VirusInvaders - Animación de Muerte")]
    public bool animacionMuerteEspecial = true;
    public float fuerzaMovimientoMuerte = 8f;
    public bool rotarAlMorir = true;
    [Range(0f, 1f)]
    public float probabilidadAnimacionEpica = 0.3f; // 30% de probabilidad por defecto
    
    [Header("VirusInvaders - Timing del Juego")]
    public float delayAntesDeMovimiento = 30f;
    
    [Header("VirusInvaders - Reaparición")]
    public bool permitirReaparicion = true;
    public float tiempoReaparicion = 120f; // 2 minutos
    
    // Referencias de componentes
    private SpriteRenderer spriteRenderer;
    private Transform jugador;
    private Rigidbody2D rb;
    private CircleCollider2D col;
    private bool estaMuerto = false;
    private float vidaActual;
    private float tiempoUltimoAtaque = 0f;
    
    // Temporización del juego
    private float tiempoInicioJuego;
    private bool puedeMoverse = false;
    
    // Sistema de animación
    private Sprite[] spritesIdle;
    private Sprite[] spritesPulse;
    private Sprite[] spritesAttack;
    private Sprite[] spritesHit;
    private Sprite[] spritesDeath;
    private Sprite spriteDefault;
    
    // Estado de animación
    private int frameActual = 0;
    private float tiempoFrame = 0f;
    private Sprite[] animacionActual;
    private bool animacionLoop = true;
    private string estadoAnimacionActual = "";
    
    // Comportamiento del enemigo
    private enum EstadoEnemigo { Idle, Persiguiendo, Atacando, Golpeado, Muriendo }
    private EstadoEnemigo estadoActual = EstadoEnemigo.Idle;
    private bool isGamePaused = false;
    
    // Variables privadas para reaparición
    private Vector3 posicionInicial;
    private Vector3 escalaInicial;
    private bool enProcesoreseaseaaparicion = false;
    
    void Start()
    {
        tiempoInicioJuego = Time.time;
        vidaActual = vida;
        
        ConfigurarComponentes();
        BuscarJugador();
        
        CargarSprites();
        IniciarAnimacionSegura();
        
        // Suscribirse a eventos de pausa y reset
        VirusInvadersGameManager.OnGamePaused += OnGamePaused;
        VirusInvadersGameManager.OnGameReset += ResetEnemy;
        
        // Guardar posición y escala inicial para reaparición
        posicionInicial = transform.position;
        escalaInicial = transform.localScale;
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
    
    void ConfigurarComponentes()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        
        // Configurar orden de renderizado para aparecer al frente
        spriteRenderer.sortingLayerName = sortingLayerName;
        spriteRenderer.sortingOrder = sortingOrder;
        spriteRenderer.color = colorEnemigo;
        
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.linearDamping = 2f;
        rb.angularDamping = 5f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.freezeRotation = true;
        
        col = GetComponent<CircleCollider2D>();
        if (col == null)
        {
            col = gameObject.AddComponent<CircleCollider2D>();
        }
        col.radius = 1.1f;
        col.isTrigger = true;
        
        // Ajustar posición Z para evitar problemas de profundidad
        Vector3 pos = transform.position;
        pos.z = -1f;
        transform.position = pos;
        
        gameObject.tag = "Enemy";
        gameObject.layer = LayerMask.NameToLayer("Default");
        
        CrearSpriteDefault();
    }
    
    void CrearSpriteDefault()
    {
        Texture2D texture = new Texture2D(64, 64);
        Color[] pixels = new Color[64 * 64];
        Vector2 center = new Vector2(32, 32);
        
        for (int x = 0; x < 64; x++)
        {
            for (int y = 0; y < 64; y++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                if (distance <= 30)
                {
                    float alpha = 1f - (distance / 30f);
                    pixels[y * 64 + x] = new Color(colorEnemigo.r, colorEnemigo.g, colorEnemigo.b, alpha * 0.8f);
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
    
    void CargarSprites()
    {
        string[] rutasBase = new string[]
        {
            $"VirusInvaders/Coronavirus/virus_spriteanimation/{tipoCoronavirus}/",
            $"Spine/Coronavirus/virus_spriteanimation/{tipoCoronavirus}/",
            $"Coronavirus/virus_spriteanimation/{tipoCoronavirus}/",
            $"virus_spriteanimation/{tipoCoronavirus}/"
        };
        
        foreach (string rutaBase in rutasBase)
        {
            spritesIdle = CargarAnimacion(rutaBase, "idle1");
            if (spritesIdle != null && spritesIdle.Length > 0)
            {
                spritesPulse = CargarAnimacion(rutaBase, "pulse") ?? spritesIdle;
                spritesAttack = CargarAnimacion(rutaBase, "attack") ?? spritesIdle;
                spritesHit = CargarAnimacion(rutaBase, "hit") ?? spritesIdle;
                spritesDeath = CargarAnimacion(rutaBase, "death") ?? spritesIdle;
                

                
                return;
            }
        }
        
        
        spritesIdle = new Sprite[] { spriteDefault };
        spritesPulse = spritesIdle;
        spritesAttack = spritesIdle;
        spritesHit = spritesIdle;
        spritesDeath = spritesIdle;
    }
    
    Sprite[] CargarAnimacion(string rutaBase, string animacion)
    {
        System.Collections.Generic.List<Sprite> sprites = new System.Collections.Generic.List<Sprite>();
        
        string prefijo = rutaBase + $"coronavirus-{tipoCoronavirus}-{animacion}_";
        
        for (int i = 0; i < 30; i++)
        {
            Sprite sprite = Resources.Load<Sprite>(prefijo + i.ToString("00"));
            if (sprite == null)
            {
                sprite = Resources.Load<Sprite>(prefijo + i.ToString());
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
    
    void IniciarAnimacionSegura()
    {
        if (spritesIdle != null && spritesIdle.Length > 0)
        {
            CambiarAnimacion(spritesIdle, true, "idle");
        }
        else
        {
            spriteRenderer.sprite = spriteDefault;
        }
    }
    
    void BuscarJugador()
    {
        if (jugador == null)
        {
            GameObject jugadorObj = GameObject.FindGameObjectWithTag("Player");
            if (jugadorObj != null)
            {
                jugador = jugadorObj.transform;
            }
            else
            {
                VirusInvadersPlayerController playerController = FindFirstObjectByType<VirusInvadersPlayerController>();
                if (playerController != null)
                {
                    jugador = playerController.transform;
                }
            }
        }
    }
    
    void Update()
    {
        if (estaMuerto || jugador == null || isGamePaused) return;
        
        VerificarDelayMovimiento();
        ActualizarAnimacion();
        ActualizarComportamiento();
        MirarHaciaJugador();
    }
    
    void VerificarDelayMovimiento()
    {
        if (!puedeMoverse && Time.time - tiempoInicioJuego >= delayAntesDeMovimiento)
        {
            puedeMoverse = true;
        }
    }
    
    void ActualizarAnimacion()
    {
        if (animacionActual == null || animacionActual.Length == 0) 
        {
            spriteRenderer.sprite = spriteDefault;
            return;
        }
        
        tiempoFrame += Time.deltaTime;
        
        if (tiempoFrame >= velocidadAnimacion)
        {
            frameActual++;
            
            if (frameActual >= animacionActual.Length)
            {
                if (animacionLoop)
                {
                    frameActual = 0;
                }
                else
                {
                    frameActual = animacionActual.Length - 1;
                }
            }
            
            if (frameActual >= 0 && frameActual < animacionActual.Length && animacionActual[frameActual] != null)
            {
                spriteRenderer.sprite = animacionActual[frameActual];
            }
            else
            {
                spriteRenderer.sprite = spriteDefault;
            }
            
            tiempoFrame = 0f;
        }
    }
    
    void CambiarAnimacion(Sprite[] nuevaAnimacion, bool loop, string nombre)
    {
        if (estadoAnimacionActual == nombre && animacionActual == nuevaAnimacion) return;
        
        if (nuevaAnimacion != null && nuevaAnimacion.Length > 0)
        {
            animacionActual = nuevaAnimacion;
            animacionLoop = loop;
            frameActual = 0;
            tiempoFrame = 0f;
            estadoAnimacionActual = nombre;
            
            if (animacionActual[0] != null)
            {
                spriteRenderer.sprite = animacionActual[0];
            }
            else
            {
                spriteRenderer.sprite = spriteDefault;
            }
        }
    }
    
    void ActualizarComportamiento()
    {
        if (estadoActual == EstadoEnemigo.Golpeado || estadoActual == EstadoEnemigo.Muriendo) return;
        
        if (!puedeMoverse)
        {
            if (estadoActual != EstadoEnemigo.Idle)
            {
                estadoActual = EstadoEnemigo.Idle;
                CambiarAnimacion(spritesIdle, true, "idle");
            }
            
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }
            return;
        }
        
        float distancia = Vector2.Distance(transform.position, jugador.position);
        
        // Determinar nuevo estado basado en distancia (como el componente moderno)
        EstadoEnemigo nuevoEstado = estadoActual;
        
        if (distancia <= distanciaAtaque)
        {
            nuevoEstado = EstadoEnemigo.Atacando;
        }
        else if (distancia <= distanciaDeteccion)
        {
            nuevoEstado = EstadoEnemigo.Persiguiendo;
        }
        else
        {
            nuevoEstado = EstadoEnemigo.Idle;
        }

        // Solo cambiar estado si es diferente (como el componente moderno)
        if (nuevoEstado != estadoActual)
        {
            
            estadoActual = nuevoEstado;
            
            // Cambiar animación solo cuando cambia el estado
            switch (estadoActual)
            {
                case EstadoEnemigo.Idle:
                    CambiarAnimacion(spritesIdle, true, "idle");
                    break;
                case EstadoEnemigo.Persiguiendo:
                CambiarAnimacion(spritesPulse, true, "pulse");
                    break;
                case EstadoEnemigo.Atacando:
                    CambiarAnimacion(spritesAttack, false, "attack");
                    break;
            }
        }

        // Ejecutar comportamiento según estado actual
        switch (estadoActual)
        {
            case EstadoEnemigo.Idle:
                // Descenso automático cuando está inactivo y puede moverse
                if (rb != null)
                {
                    rb.linearVelocity = Vector2.down * velocidadDescenso;
                }
                break;

            case EstadoEnemigo.Persiguiendo:
                // Perseguir al jugador
            Vector2 direccion = (jugador.position - transform.position).normalized;
            if (rb != null)
            {
                rb.linearVelocity = direccion * velocidadMovimiento;
            }
                break;

            case EstadoEnemigo.Atacando:
                // Detenerse y atacar
            if (rb != null)
            {
                    rb.linearVelocity = Vector2.zero;
                }
                
                // Verificar si es tiempo de atacar
                if (Time.time - tiempoUltimoAtaque >= tiempoEntreAtaques)
                {
                    StartCoroutine(EjecutarAtaque());
                    tiempoUltimoAtaque = Time.time;
            }
                break;
        }
    }
    
    IEnumerator EjecutarAtaque()
    {
        tiempoUltimoAtaque = Time.time;
        
        // *** PASO 1: Dar tiempo para que la animación se establezca correctamente ***
        yield return new WaitForEndOfFrame(); // Esperar al final del frame actual
        yield return new WaitForSeconds(velocidadAnimacion * 0.5f); // Esperar medio frame extra
        
        // *** PASO 2: Esperar el tiempo dramático antes del daño ***
        // Esperamos 2-3 frames adicionales para que el jugador vea venir el ataque
        float tiempoAntesDelDaño = 2f * velocidadAnimacion; // 2 frames = 0.3s
        
        yield return new WaitForSeconds(tiempoAntesDelDaño);
        
        // *** VERIFICAR QUE EL JUGADOR SIGUE EN RANGO DE ATAQUE ***
        if (jugador != null && !estaMuerto)
        {
            float distanciaFinal = Vector2.Distance(transform.position, jugador.position);
            if (distanciaFinal <= distanciaAtaque)
        {
            VirusInvadersPlayerController playerController = jugador.GetComponent<VirusInvadersPlayerController>();
            if (playerController != null)
            {
                Vector2 direccionKnockback = (jugador.position - transform.position).normalized;
                    float damageAmount = 5f; // Usando daño fijo por ahora, podría usar dañoAtaque
                    playerController.RecibirDaño(direccionKnockback, damageAmount);
            }
        }
        }
        
        // *** IMPORTANTE: Tras el ataque, el estado puede cambiar en ActualizarComportamiento() ***
    }
    
    void MirarHaciaJugador()
    {
        if (jugador != null && spriteRenderer != null)
        {
            Vector3 direccion = jugador.position - transform.position;
            spriteRenderer.flipX = direccion.x < 0;
        }
    }
    
    public void RecibirDaño(float cantidad)
    {
        if (estaMuerto) return;
        
        vidaActual -= cantidad;
        CambiarAnimacion(spritesHit, false, "hit");
        StartCoroutine(EfectoDaño());
        
        if (vidaActual <= 0)
        {
            Morir();
        }
    }
    
    IEnumerator EfectoDaño()
    {
        yield return new WaitForSeconds(0.2f);
        
        if (!estaMuerto && estadoActual != EstadoEnemigo.Muriendo)
        {
            estadoActual = EstadoEnemigo.Idle;
            CambiarAnimacion(spritesIdle, true, "idle");
        }
    }
    
    void Morir()
    {
        if (estaMuerto) return;
        
        estaMuerto = true;
        estadoActual = EstadoEnemigo.Muriendo;
        
        // *** INTEGRACIÓN CON SISTEMA DE PUNTUACIÓN CORREGIDA ***
        if (VirusInvadersGameManager.Instance != null)
        {
            // Crear un EnemyData temporal con la configuración del enemigo legacy
            VirusInvadersEnemyData tempEnemyData = ScriptableObject.CreateInstance<VirusInvadersEnemyData>();
            tempEnemyData.pointsValue = 15; // Puntos base para enemigos legacy
            tempEnemyData.enemyName = $"Legacy_{tipoCoronavirus}_Enemy";
            tempEnemyData.pointsMultiplier = 1.0f;
            tempEnemyData.awardsCombo = true;
            
            // *** CLAVE: Pasar la posición del enemigo para efectos visuales ***
            VirusInvadersGameManager.Instance.EnemyDefeated(tempEnemyData, transform.position);
            
    
        }
        
        if (permitirReaparicion && !enProcesoreseaseaaparicion)
        {
            // Iniciar proceso de reaparición en lugar de destruir
            StartCoroutine(ProcesoReaparicion());
        }
        else if (animacionMuerteEspecial)
        {
            // Decidir aleatoriamente si hacer la animación épica
            bool hacerAnimacionEpica = Random.Range(0f, 1f) <= probabilidadAnimacionEpica;
            
            if (hacerAnimacionEpica)
            {
                // Animación épica completa (deambulación + expansión)
                StartCoroutine(AnimacionMuerte());
            }
            else
            {
                // Solo deambulación básica, sin expansión épica
                StartCoroutine(AnimacionMuerteBasica());
            }
        }
        else
        {
            // Si no hay animación especial, destruir inmediatamente
            Destroy(gameObject);
        }
    }
    
    IEnumerator AnimacionMuerte()
    {
        CambiarAnimacion(spritesDeath, false, "death");
        
        // Fase 1: Animación de globo desinflándose (1.5 segundos)
        yield return StartCoroutine(AnimacionGloboDesinflando());
        
        // Fase 2: Expansión de virus que mancha toda la pantalla (1 segundo)
        yield return StartCoroutine(AnimacionExpansionManchaVirus());
        
        Destroy(gameObject);
    }
    
    IEnumerator AnimacionMuerteBasica()
    {
        CambiarAnimacion(spritesDeath, false, "death");
        
        // Solo deambulación básica, sin expansión épica
        yield return StartCoroutine(AnimacionGloboDesinflando());
        
        // Desvanecimiento simple y rápido
        yield return StartCoroutine(AnimacionFadeOutBasico());
        
        Destroy(gameObject);
    }
    
    IEnumerator AnimacionGloboDesinflando()
    {
        float duracion = 1.5f;
        float tiempo = 0f;
        
        // Variables iniciales
        Vector3 escalaInicial = transform.localScale;
        Color colorInicial = spriteRenderer.color;
        
        // Variables para el movimiento errático
        float fuerzaMovimiento = fuerzaMovimientoMuerte;
        float frecuenciaMovimiento = 15f;
        float reduccionFuerza = 0.85f;
        
        // Variables para el cambio de escala
        float amplitudEscala = 0.3f;
        float frecuenciaEscala = 20f;
        
        while (tiempo < duracion)
        {
            float progreso = tiempo / duracion;
            
            // === MOVIMIENTO ERRÁTICO (como globo desinflándose) ===
            Vector2 direccionAleatoria = new Vector2(
                Mathf.Sin(Time.time * frecuenciaMovimiento + Random.Range(0f, Mathf.PI * 2f)),
                Mathf.Cos(Time.time * frecuenciaMovimiento * 0.7f + Random.Range(0f, Mathf.PI * 2f))
            ).normalized;
            
            // La fuerza disminuye con el tiempo
            float fuerzaActual = fuerzaMovimiento * (1f - progreso * 0.8f);
            
            // Aplicar movimiento errático
            if (rb != null)
            {
                rb.linearVelocity = direccionAleatoria * fuerzaActual;
            }
            
            // === ESCALA OSCILANTE (efecto de desinflado) ===
            float factorEscala = 1f + Mathf.Sin(Time.time * frecuenciaEscala) * amplitudEscala * (1f - progreso);
            // Reducir tamaño general gradualmente
            float reduccionGeneral = Mathf.Lerp(1f, 0.3f, progreso);
            
            Vector3 nuevaEscala = escalaInicial * factorEscala * reduccionGeneral;
            transform.localScale = nuevaEscala;
            
            // === CAMBIO DE COLOR (rojizo como si estuviera "explotando") ===
            Color colorMuerte = Color.Lerp(colorInicial, new Color(1f, 0.3f, 0.3f, 0.8f), progreso);
            spriteRenderer.color = colorMuerte;
            
            // === ROTACIÓN ERRÁTICA (opcional) ===
            if (rotarAlMorir)
            {
                float rotacion = Mathf.Sin(Time.time * frecuenciaMovimiento * 1.5f) * 180f * progreso;
                transform.rotation = Quaternion.Euler(0, 0, rotacion);
            }
            
            // Reducir frecuencias gradualmente para efecto de "pérdida de energía"
            frecuenciaMovimiento *= reduccionFuerza;
            frecuenciaEscala *= reduccionFuerza;
            
            tiempo += Time.deltaTime;
            yield return null;
        }
        
        // Detener movimiento al final
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false; // Ahora sí desactivamos el Rigidbody2D
        }
    }
    
    IEnumerator AnimacionExpansionManchaVirus()
    {
        float duracion = 1f;
        float tiempo = 0f;
        Color colorInicial = spriteRenderer.color;
        Vector3 escalaInicial = transform.localScale;
        
        // Escala objetivo para cubrir toda la pantalla (expansión masiva)
        float escalaMaxima = 25f; // Escala enorme para cubrir pantalla
        
        while (tiempo < duracion)
        {
            float progreso = tiempo / duracion;
            float escalaPop = 1f;
            
            // Efecto "pop" en los últimos 30% de la animación
            if (progreso > 0.7f)
            {
                float factorPop = (progreso - 0.7f) / 0.3f;
                escalaPop = 1f + Mathf.Sin(factorPop * Mathf.PI) * 0.5f;
            }
            
            // Expansión exponencial (comienza lento, se acelera)
            float progresoExpansion = Mathf.Pow(progreso, 0.3f);
            float escalaActual = Mathf.Lerp(escalaInicial.x, escalaMaxima, progresoExpansion);
            
            // Aplicar escala con efecto pop
            Vector3 escalaFinal = Vector3.one * escalaActual * escalaPop;
            transform.localScale = escalaFinal;
            
            // Desvanecer mientras se expande
            float alpha = Mathf.Lerp(1f, 0f, Mathf.Pow(progreso, 1.5f));
            Color color = colorInicial;
            color.a = alpha;
            spriteRenderer.color = color;
            
            // Rotación opcional para efecto más dramático
            transform.Rotate(0, 0, 30f * Time.deltaTime * (1f - progreso));
            
            tiempo += Time.deltaTime;
            yield return null;
        }
        
        // Asegurar que esté completamente invisible al final
        spriteRenderer.color = new Color(colorInicial.r, colorInicial.g, colorInicial.b, 0f);
    }
    
    IEnumerator AnimacionFadeOutBasico()
    {
        float duracion = 0.5f;
        float tiempo = 0f;
        Color colorInicial = spriteRenderer.color;
        
        while (tiempo < duracion)
        {
            float progreso = tiempo / duracion;
            
            // Desvanecimiento simple sin efectos adicionales
            float alpha = Mathf.Lerp(1f, 0f, progreso);
            Color color = colorInicial;
            color.a = alpha;
            spriteRenderer.color = color;
            
            tiempo += Time.deltaTime;
            yield return null;
        }
        
        // Asegurar que quede completamente invisible
        spriteRenderer.color = new Color(colorInicial.r, colorInicial.g, colorInicial.b, 0f);
    }
    
    IEnumerator ProcesoReaparicion()
    {
        enProcesoreseaseaaparicion = true;
        
        // Cambiar a animación de muerte antes de ejecutarla
        CambiarAnimacion(spritesDeath, false, "death");
        
        // Ejecutar animación de muerte COMPLETA si está habilitada
        if (animacionMuerteEspecial)
        {
            bool hacerAnimacionEpica = Random.Range(0f, 1f) <= probabilidadAnimacionEpica;
            
            if (hacerAnimacionEpica)
            {
                // Animación completa: globo desinflando + expansión
                yield return StartCoroutine(AnimacionGloboDesinflando());
                yield return StartCoroutine(AnimacionExpansionManchaVirus());
            }
            else
            {
                // Solo deambulación básica + desvanecimiento
                yield return StartCoroutine(AnimacionGloboDesinflando());
                yield return StartCoroutine(AnimacionFadeOutBasico());
            }
        }
        else
        {
            // Solo hacer invisible inmediatamente
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0f);
        }
        
        // Desactivar componentes después de la animación
        col.enabled = false;
        rb.simulated = false;
        
        // Esperar tiempo de reaparición
        yield return new WaitForSeconds(tiempoReaparicion);
        
        // Reaparecer
        Reaparecer();
    }
    
    void Reaparecer()
    {
        // Resetear estado
        estaMuerto = false;
        enProcesoreseaseaaparicion = false;
        estadoActual = EstadoEnemigo.Idle;
        
        // Resetear vida
        vidaActual = vida;
        
        // Volver a posición inicial
        transform.position = posicionInicial;
        
        // Reactivar componentes
        col.enabled = true;
        rb.simulated = true;
        
        // Hacer visible con efecto de aparición
        StartCoroutine(EfectoAparicion());
        
        // Resetear timer de movimiento
        tiempoInicioJuego = Time.time;
        puedeMoverse = false;
        
        // Volver a animación idle
        IniciarAnimacionSegura();
    }
    
    IEnumerator EfectoAparicion()
    {
        float duracion = 1f;
        float tiempo = 0f;
        Color colorOriginal = colorEnemigo;
        
        while (tiempo < duracion)
        {
            float progreso = tiempo / duracion;
            
            // Aparecer gradualmente
            float alpha = Mathf.Lerp(0f, 1f, progreso);
            spriteRenderer.color = new Color(colorOriginal.r, colorOriginal.g, colorOriginal.b, alpha);
            
            // Efecto de escala desde pequeño hasta la escala original
            float factorEscala = Mathf.Lerp(0.1f, 1f, Mathf.Pow(progreso, 0.5f));
            transform.localScale = escalaInicial * factorEscala;
            
            tiempo += Time.deltaTime;
            yield return null;
        }
        
        // Asegurar valores finales con escala original
        spriteRenderer.color = colorOriginal;
        transform.localScale = escalaInicial;
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (estaMuerto) return;
        

        
        if (other.CompareTag("Bullet"))
        {

            RecibirDaño(25f);
            
            // Destruir la bala
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("Player"))
        {
            // *** REMOVIDO: No hacer daño aquí, solo durante la animación de ataque ***
            // El daño se controla completamente a través del sistema de ataque con animación
        }
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, distanciaAtaque);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, distanciaDeteccion);
        
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * 2f);
    }
    
    // Métodos públicos para control y depuración
    public void CambiarTipoCoronavirus(string nuevoTipo)
    {
        tipoCoronavirus = nuevoTipo;
        CargarSprites();
        IniciarAnimacionSegura();
    }
    
    public bool PuedeMoverse()
    {
        return puedeMoverse;
    }
    
    public void ForzarInicioMovimiento()
    {
        puedeMoverse = true;
        tiempoInicioJuego = Time.time - delayAntesDeMovimiento;
    }
    
    public float TiempoHastaMovimiento()
    {
        return Mathf.Max(0f, delayAntesDeMovimiento - (Time.time - tiempoInicioJuego));
    }
    
    // Método público para forzar reaparición (útil para testing)
    public void ForzarReaparicion()
    {
        if (estaMuerto && permitirReaparicion)
        {
            StopAllCoroutines();
            Reaparecer();
        }
    }
    
    // Método para desactivar reaparición (si quieres que algunos enemigos no reaparezcan)
    public void DesactivarReaparicion()
    {
        permitirReaparicion = false;
    }
    
    // *** MÉTODO DE RESET COMPLETO PARA REINICIO DE JUEGO ***
    void ResetEnemy()
    {

        
        // *** 1. DETENER TODAS LAS CORRUTINAS ACTIVAS ***
        StopAllCoroutines();
        
        // *** 2. RESETEAR ESTADO GENERAL ***
        estaMuerto = false;
        enProcesoreseaseaaparicion = false;
        estadoActual = EstadoEnemigo.Idle;
        isGamePaused = false; // Will be managed by GameManager
        
        // *** 3. RESETEAR SISTEMA DE VIDA ***
        vidaActual = vida;
        
        // *** 4. RESETEAR TIMING Y MOVIMIENTO ***
        tiempoInicioJuego = Time.time;
        tiempoUltimoAtaque = 0f;
        puedeMoverse = false; // Debe esperar el delay inicial
        
        // *** 5. RESETEAR POSICIÓN Y ESCALA ***
        transform.position = posicionInicial;
        transform.localScale = escalaInicial;
        transform.rotation = Quaternion.identity; // Resetear rotación también
        
        // *** 6. RESETEAR COMPONENTES FÍSICOS ***
        if (rb != null)
        {
            rb.simulated = true;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Dynamic;
        }
        
        if (col != null)
        {
            col.enabled = true;
            col.isTrigger = true;
        }
        
        // *** 7. RESETEAR SISTEMA VISUAL ***
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
            spriteRenderer.color = colorEnemigo;
            
            // Resetear orden para asegurar renderizado correcto
            spriteRenderer.sortingLayerName = sortingLayerName;
            spriteRenderer.sortingOrder = sortingOrder;
        }
        
        // *** 8. RESETEAR ANIMACIONES ***
        frameActual = 0;
        tiempoFrame = 0f;
        animacionActual = null;
        animacionLoop = true;
        estadoAnimacionActual = "";
        
        // *** 9. RECARGAR SPRITES Y REINICIAR ANIMACIÓN ***
        CargarSprites();
        IniciarAnimacionSegura();
        
        // *** 10. RE-BUSCAR JUGADOR (en caso de que haya cambiado) ***
        jugador = null; // Force re-find
        BuscarJugador();
        
        // *** 11. RESETEAR TAG Y LAYER POR SEGURIDAD ***
        gameObject.tag = "Enemy";
        gameObject.layer = LayerMask.NameToLayer("Default");
        
        // *** 12. ASEGURAR Z-DEPTH CORRECTO ***
        Vector3 pos = transform.position;
        pos.z = -1f;
        transform.position = pos;
        

    }
}
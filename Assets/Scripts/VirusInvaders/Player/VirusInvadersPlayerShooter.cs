using UnityEngine;

public class VirusInvadersPlayerShooter : MonoBehaviour
{
    [Header("VirusInvaders - Shooting Configuration")]
    public GameObject prefabBala;
    public Transform puntoDisparo;
    public float cadenciaDisparo = 0.3f;
    public KeyCode teclaDisparo = KeyCode.X;
    
    [Header("VirusInvaders - Lateral Shooting")]
    public KeyCode teclaDisparoLateral = KeyCode.Z;
    public Transform puntoDisparoIzquierdo;
    public Transform puntoDisparoDerecho;
    
    [Header("VirusInvaders - Bullet Configuration")]
    public Sprite texturaBala;
    public float velocidadBala = 15f;
    public float dañoBala = 50f;
    
    // Referencias privadas
    private float tiempoUltimoDisparo = 0f;
    private float tiempoUltimoDisparoLateral = 0f;
    private bool isGamePaused = false;
    
    void Start()
    {
        ConfigurarComponentes();
        CargarTexturaBala();
        
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
    }
    
    void ConfigurarComponentes()
    {
        // Crear punto de disparo si no existe
        if (puntoDisparo == null)
        {
            GameObject puntoDisparoObj = new GameObject("VirusInvaders_ShootPoint");
            puntoDisparoObj.transform.SetParent(transform);
            puntoDisparoObj.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            puntoDisparo = puntoDisparoObj.transform;
        }
        
        // Crear puntos de disparo lateral si no existen
        if (puntoDisparoIzquierdo == null)
        {
            GameObject puntoIzqObj = new GameObject("VirusInvaders_LeftShootPoint");
            puntoIzqObj.transform.SetParent(transform);
            puntoIzqObj.transform.localPosition = new Vector3(-0.3f, 0.2f, 0f);
            puntoDisparoIzquierdo = puntoIzqObj.transform;
        }
        
        if (puntoDisparoDerecho == null)
        {
            GameObject puntoDerObj = new GameObject("VirusInvaders_RightShootPoint");
            puntoDerObj.transform.SetParent(transform);
            puntoDerObj.transform.localPosition = new Vector3(0.3f, 0.2f, 0f);
            puntoDisparoDerecho = puntoDerObj.transform;
        }
        
        // Crear prefab de bala si no existe
        if (prefabBala == null)
        {
            CrearPrefabBalaSimple();
        }
    }
    
    void CargarTexturaBala()
    {
        if (texturaBala == null)
        {
            texturaBala = Resources.Load<Sprite>("VirusInvaders/Sprites/bullet");
        }
    }
    
    void CrearPrefabBalaSimple()
    {
        // Crear prefab básico solo con componentes esenciales
        GameObject balaObj = new GameObject("VirusInvadersBullet");
        
        // Agregar solo el script de bala
        VirusInvadersBullet scriptBala = balaObj.AddComponent<VirusInvadersBullet>();
        scriptBala.velocidad = velocidadBala;
        scriptBala.daño = dañoBala;
        
        prefabBala = balaObj;
        balaObj.SetActive(false);
    }
    
    void Update()
    {
        // Solo procesar entrada si este es el Player y el juego no está pausado
        if (CompareTag("Player") && !isGamePaused)
        {
            // Disparo normal hacia arriba
            if (Input.GetKeyDown(teclaDisparo))
            {
            if (Time.time - tiempoUltimoDisparo >= cadenciaDisparo)
            {
                Disparar();
                tiempoUltimoDisparo = Time.time;
                }
            }
            
            // Disparo lateral
            if (Input.GetKeyDown(teclaDisparoLateral))
            {
                if (Time.time - tiempoUltimoDisparoLateral >= cadenciaDisparo)
                {
                    DispararLateral();
                    tiempoUltimoDisparoLateral = Time.time;
                }
            }
        }
    }
    
    public void Disparar()
    {
        if (prefabBala == null || puntoDisparo == null) 
        {
            return;
        }
        
        // Instanciar la jeringuilla
        GameObject nuevaJeringuilla = Instantiate(prefabBala, puntoDisparo.position, Quaternion.identity);
        nuevaJeringuilla.SetActive(true);
        
        // Configurar la jeringuilla
        VirusInvadersBullet bullet = nuevaJeringuilla.GetComponent<VirusInvadersBullet>();
        if (bullet != null)
        {
            bullet.velocidad = velocidadBala;
            bullet.daño = dañoBala;
            bullet.ConfigurarSprite(texturaBala);
        }
    }
    
    public void DispararLateral()
    {
        if (prefabBala == null || puntoDisparoIzquierdo == null || puntoDisparoDerecho == null) 
        {
            return;
        }
        
        // Crear bala izquierda
        GameObject balaIzquierda = Instantiate(prefabBala, puntoDisparoIzquierdo.position, Quaternion.identity);
        balaIzquierda.SetActive(true);
        
        // Crear bala derecha
        GameObject balaDerecha = Instantiate(prefabBala, puntoDisparoDerecho.position, Quaternion.identity);
        balaDerecha.SetActive(true);
        
        // Configurar bala izquierda (hacia arriba-izquierda)
        VirusInvadersLateralBullet bulletIzq = balaIzquierda.AddComponent<VirusInvadersLateralBullet>();
        bulletIzq.ConfigurarBala(new Vector2(-3f, 1f).normalized, velocidadBala, dañoBala, texturaBala);
        
        // Configurar bala derecha (hacia arriba-derecha)
        VirusInvadersLateralBullet bulletDer = balaDerecha.AddComponent<VirusInvadersLateralBullet>();
        bulletDer.ConfigurarBala(new Vector2(3f, 1f).normalized, velocidadBala, dañoBala, texturaBala);
        
        // Remover el componente VirusInvadersBullet para evitar conflictos
        VirusInvadersBullet bulletComponentIzq = balaIzquierda.GetComponent<VirusInvadersBullet>();
        if (bulletComponentIzq != null) DestroyImmediate(bulletComponentIzq);
        
        VirusInvadersBullet bulletComponentDer = balaDerecha.GetComponent<VirusInvadersBullet>();
        if (bulletComponentDer != null) DestroyImmediate(bulletComponentDer);
    }
    

}

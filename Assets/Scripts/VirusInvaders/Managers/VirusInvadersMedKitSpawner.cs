using UnityEngine;
using System.Collections;

[System.Serializable]
public class VirusInvadersMedKitSpawner : MonoBehaviour
{
    [Header("VirusInvaders - Configuración del Spawner de MedKit")]
    public bool activarSpawner = true;
    public float tiempoEntreSpawns = 60f; // Cada 1 minuto
    public float variacionTiempo = 10f; // Variación de ±10 segundos
    
    [Header("VirusInvaders - Área de Aparición")]
    public float areaSpawnAncho = 15f;
    public float alturaSpawn = 8f;
    public Vector3 centroSpawn = Vector3.zero;
    
    [Header("VirusInvaders - Propiedades del MedKit")]
    public float cantidadCuracionBase = 30f;
    public float variacionCuracion = 10f; // Variación de ±10 HP
    public float velocidadCaidaBase = 2f;
    public float variacionVelocidad = 0.5f;
    
    [Header("VirusInvaders - Probabilidad")]
    public float probabilidadSpawn = 75f; // 75% de probabilidad
    
    [Header("VirusInvaders - Requisitos de Salud del Jugador")]
    public bool soloSiJugadorHerido = true;
    public float umbralVidaBaja = 50f; // Solo aparece si salud < 50%
    
    // Referencias privadas
    private VirusInvadersPlayerController playerController;
    private Camera camaraJuego;
    private bool spawnerActivo = false;
    private float siguienteSpawn = 0f;
    
    void Start()
    {
        ConfigurarComponentes();
        ConfigurarAreaSpawn();
        
        if (activarSpawner)
        {
            ActivarSpawner();
        }
    }
    
    void ConfigurarComponentes()
    {
        if (playerController == null)
        {
            playerController = FindFirstObjectByType<VirusInvadersPlayerController>();
            if (playerController == null)
            {
                Debug.LogWarning("VirusInvaders: No se encontró PlayerController para el MedKit Spawner");
                return;
            }
        }
        
        if (camaraJuego == null)
        {
            camaraJuego = FindFirstObjectByType<Camera>();
        }
        
        siguienteSpawn = Time.time + GetTiempoSpawnAleatorio();
    }
    
    void ConfigurarAreaSpawn()
    {
        Camera camara = Camera.main;
        if (camara != null)
        {
            float alturaCamara = 2f * camara.orthographicSize;
            float anchoCamara = alturaCamara * camara.aspect;
            
            areaSpawnAncho = anchoCamara * 0.8f; // 80% del ancho de pantalla
            alturaSpawn = camara.transform.position.y + (alturaCamara * 0.6f);
        }
        else
        {
            areaSpawnAncho = 15f;
            alturaSpawn = 8f;
        }
        
        if (camaraJuego != null)
        {
            ConfigurarAreaSpawnPorCamara();
        }
    }
    
    void ConfigurarAreaSpawnPorCamara()
    {
        float alturaVision = camaraJuego.orthographicSize * 2f;
        float anchoVision = alturaVision * camaraJuego.aspect;
        
        areaSpawnAncho = anchoVision * 0.8f; // 80% del ancho visible
        alturaSpawn = camaraJuego.transform.position.y + alturaVision * 0.6f;
        centroSpawn = new Vector3(camaraJuego.transform.position.x, alturaSpawn, 0f);
    }
    
    void Update()
    {
        if (!spawnerActivo || !activarSpawner) return;
        
        if (Time.time >= siguienteSpawn)
        {
            IntentarSpawn();
            siguienteSpawn = Time.time + GetTiempoSpawnAleatorio();
        }
    }
    
    void IniciarSpawner()
    {
        spawnerActivo = true;
    }
    
    void DetenerSpawner()
    {
        spawnerActivo = false;
    }
    
    void IntentarSpawn()
    {
        float random = Random.Range(0f, 100f);
        if (random > probabilidadSpawn)
        {
            return;
        }
        
        if (!VerificarCondicionesJugador())
        {
            return;
        }
        
        SpawnearBotiquin();
    }
    
    bool VerificarCondicionesJugador()
    {
        if (playerController == null) return true;
        
        if (!soloSiJugadorHerido) return true;
        
        float vidaActual = playerController.GetVidaActual();
        float vidaMaxima = playerController.GetVidaMaxima();
        float porcentajeVida = (vidaActual / vidaMaxima) * 100f;
        
        bool necesitaCuracion = porcentajeVida < umbralVidaBaja;
        
        return necesitaCuracion;
    }
    
    void SpawnearBotiquin()
    {
        Vector3 posicionSpawn = CalcularPosicionSpawn();
        
        float curacion = cantidadCuracionBase + Random.Range(-variacionCuracion, variacionCuracion);
        float velocidad = velocidadCaidaBase + Random.Range(-variacionVelocidad, variacionVelocidad);
        
        GameObject medKit = VirusInvadersMedKit.CrearBotiquin(posicionSpawn, curacion);
        
        if (medKit != null)
        {
            VirusInvadersMedKit medKitScript = medKit.GetComponent<VirusInvadersMedKit>();
            if (medKitScript != null)
            {
                medKitScript.SetVelocidadCaida(velocidad);
                medKitScript.SetCantidadCuracion(curacion);
            }
        }
    }
    
    Vector3 CalcularPosicionSpawn()
    {
        float rangoX = areaSpawnAncho * 0.5f;
        float posX = centroSpawn.x + Random.Range(-rangoX, rangoX);
        
        float posY = alturaSpawn;
        
        return new Vector3(posX, posY, 0f);
    }
    
    float GetTiempoSpawnAleatorio()
    {
        return tiempoEntreSpawns + Random.Range(-variacionTiempo, variacionTiempo);
    }
    
    // Métodos públicos de control
    public void ActivarSpawner()
    {
        if (!spawnerActivo)
        {
            spawnerActivo = true;
            IniciarSpawner();
        }
    }
    
    public void DesactivarSpawner()
    {
        spawnerActivo = false;
    }
    
    public void ForzarSpawn()
    {
        if (activarSpawner)
        {
            SpawnearBotiquin();
        }
    }
    
    public void ConfigurarTiempoSpawn(float nuevoTiempo, float nuevaVariacion)
    {
        tiempoEntreSpawns = Mathf.Max(nuevoTiempo, 1f);
        variacionTiempo = Mathf.Max(nuevaVariacion, 0f);
    }
    
    public void ConfigurarCuracion(float nuevaCantidad, float nuevaVariacion)
    {
        cantidadCuracionBase = Mathf.Max(nuevaCantidad, 5f);
        variacionCuracion = Mathf.Max(nuevaVariacion, 0f);
    }
    
    public void ConfigurarProbabilidad(float nuevaProbabilidad)
    {
        probabilidadSpawn = Mathf.Clamp(nuevaProbabilidad, 0f, 100f);
    }
    
    public void NotificarBotiquinRecogido()
    {
    }
    
    public float GetTiempoHastaSiguienteSpawn()
    {
        return Mathf.Max(0f, siguienteSpawn - Time.time);
    }
    
    // Visualización en editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(centroSpawn, new Vector3(areaSpawnAncho, 1f, 0f));
        
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(centroSpawn, 0.2f);
        
        if (Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            Vector3 infoPos = centroSpawn + Vector3.up * 1.5f;
            if (spawnerActivo)
            {
                Gizmos.DrawSphere(infoPos, 0.1f);
            }
        }
    }
} 
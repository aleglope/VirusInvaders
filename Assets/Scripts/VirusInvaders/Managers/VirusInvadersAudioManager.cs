using UnityEngine;
using System.Collections;

public class VirusInvadersAudioManager : MonoBehaviour
{
    [Header("VirusInvaders - Audio Sources")]
    [SerializeField] private AudioSource sfxAudioSource;
    [SerializeField] private AudioSource musicAudioSource;
    
    [Header("VirusInvaders - Sound Effects")]
    [SerializeField] private AudioClip sonidoDisparo;
    [SerializeField] private AudioClip sonidoImpacto;
    [SerializeField] private AudioClip sonidoExplosion;
    [SerializeField] private AudioClip sonidoMedKit;
    [SerializeField] private AudioClip sonidoEnemyHit;
    [SerializeField] private AudioClip sonidoEnemyDeath;
    [SerializeField] private AudioClip sonidoPowerUp;
    [SerializeField] private AudioClip sonidoLevelUp;
    
    [Header("VirusInvaders - Level Music")]
    [SerializeField] private AudioClip[] musicaPorNivel = new AudioClip[3]; // action1, action2, action3
    [SerializeField] private AudioClip musicaGameOver;
    [SerializeField] private AudioClip musicaMenu;
    
    [Header("VirusInvaders - Music Transition")]
    [SerializeField] private float transitionDuration = 2f;
    [SerializeField] private bool enableMusicTransitions = true;
    
    [Header("VirusInvaders - Volume Controls")]
    [Range(0f, 1f)] public float volumenSFX = 0.7f;
    [Range(0f, 1f)] public float volumenMusica = 0.3f;
    [Range(0f, 1f)] public float volumenMaster = 1f;
    
    // Variables privadas
    private int currentMusicLevel = -1;
    private Coroutine musicTransitionCoroutine;
    
    // Singleton pattern
    public static VirusInvadersAudioManager Instance { get; private set; }
    
    void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            ConfigurarAudio();
            SuscribirseAEventos();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void OnDestroy()
    {
        DesuscribirseDeEventos();
    }
    
    void SuscribirseAEventos()
    {
        // Suscribirse al evento de cambio de dificultad para cambiar música
        VirusInvadersGameManager.OnDifficultyChanged += OnLevelChanged;
        VirusInvadersGameManager.OnGameOver += OnGameOver;
        VirusInvadersGameManager.OnGameReset += OnGameReset;
    }
    
    void DesuscribirseDeEventos()
    {
        VirusInvadersGameManager.OnDifficultyChanged -= OnLevelChanged;
        VirusInvadersGameManager.OnGameOver -= OnGameOver;
        VirusInvadersGameManager.OnGameReset -= OnGameReset;
    }
    
    void ConfigurarAudio()
    {
        // Crear AudioSource para SFX si no existe
        if (sfxAudioSource == null)
        {
            GameObject sfxObject = new GameObject("SFX_AudioSource");
            sfxObject.transform.SetParent(transform);
            sfxAudioSource = sfxObject.AddComponent<AudioSource>();
            
            sfxAudioSource.playOnAwake = false;
            sfxAudioSource.spatialBlend = 0f; // 2D audio
            sfxAudioSource.volume = volumenSFX * volumenMaster;
        }
        
        // Crear AudioSource para música si no existe
        if (musicAudioSource == null)
        {
            GameObject musicObject = new GameObject("Music_AudioSource");
            musicObject.transform.SetParent(transform);
            musicAudioSource = musicObject.AddComponent<AudioSource>();
            
            musicAudioSource.playOnAwake = false;
            musicAudioSource.spatialBlend = 0f; // 2D audio
            musicAudioSource.volume = volumenMusica * volumenMaster;
            musicAudioSource.loop = true;
        }
        
        // Cargar sonidos desde Resources
        CargarSonidos();
    }
    
    void CargarSonidos()
    {
        // Efectos de sonido
        if (sonidoDisparo == null)
            sonidoDisparo = Resources.Load<AudioClip>("VirusInvaders/Audio/SFX/Shoot");
            
        if (sonidoImpacto == null)
        {
            sonidoImpacto = Resources.Load<AudioClip>("VirusInvaders/Audio/SFX/Impact");
            Debug.Log(sonidoImpacto != null ? "Impacto cargado" : "Error cargando impacto");
        }
        
        // Cargar sonido de enemy hit (cuando enemigo nos ataca)
        if (sonidoEnemyHit == null)
        {
            sonidoEnemyHit = Resources.Load<AudioClip>("VirusInvaders/Audio/SFX/enemyhit");
            Debug.Log(sonidoEnemyHit != null ? "Enemy hit cargado" : "Enemy hit no encontrado");
        }
        
        // Cargar sonido de muerte de enemigo
        if (sonidoEnemyDeath == null)
        {
            sonidoEnemyDeath = Resources.Load<AudioClip>("VirusInvaders/Audio/SFX/enemydeath");
            Debug.Log(sonidoEnemyDeath != null ? "Enemy death cargado" : "Enemy death no encontrado");
        }
        
        // Cargar sonido de MedKit
        if (sonidoMedKit == null)
        {
            sonidoMedKit = Resources.Load<AudioClip>("VirusInvaders/Audio/SFX/MedKit");
            Debug.Log(sonidoMedKit != null ? "MedKit cargado" : "MedKit no encontrado");
        }
        
        // Música por niveles
        for (int i = 0; i < musicaPorNivel.Length; i++)
        {
            if (musicaPorNivel[i] == null)
            {
                string musicFileName = $"Action{i + 1}";
                musicaPorNivel[i] = Resources.Load<AudioClip>($"VirusInvaders/Audio/Music/{musicFileName}");
                
                if (musicaPorNivel[i] != null)
                {
                    Debug.Log($"VirusInvadersAudioManager: Música nivel {i + 1} '{musicFileName}' cargada correctamente");
                }
                else
                {
                    Debug.LogWarning($"VirusInvadersAudioManager: No se pudo cargar la música '{musicFileName}' para el nivel {i + 1}");
                }
            }
        }
        
        // Música de game over
        if (musicaGameOver == null)
        {
            musicaGameOver = Resources.Load<AudioClip>("VirusInvaders/Audio/Music/gameover");
            Debug.Log(musicaGameOver != null ? "Música gameover cargada" : "Música gameover no encontrada");
        }
    }
    
    // === EVENTOS DEL JUEGO ===
    void OnLevelChanged(int newLevel)
    {
        Debug.Log($"VirusInvadersAudioManager: Cambiando a música del nivel {newLevel + 1}");
        CambiarMusicaNivel(newLevel);
        
        // Reproducir sonido de level up
        ReproducirSFX(sonidoLevelUp);
    }
    
    void OnGameOver(int finalScore)
    {
        Debug.Log("VirusInvadersAudioManager: Game Over - Reproduciendo música de game over");
        ReproducirMusicaGameOver();
    }
    
    void OnGameReset()
    {
        Debug.Log("VirusInvadersAudioManager: Game Reset - Volviendo a música del nivel 1");
        CambiarMusicaNivel(0); // Volver al nivel 1
    }
    
    // === MÉTODOS DE MÚSICA ===
    public void CambiarMusicaNivel(int nivel)
    {
        // Asegurar que el nivel esté en rango
        nivel = Mathf.Clamp(nivel, 0, musicaPorNivel.Length - 1);
        
        if (currentMusicLevel == nivel) return; // Ya estamos reproduciendo esta música
        
        currentMusicLevel = nivel;
        AudioClip nuevaMusica = musicaPorNivel[nivel];
        
        if (nuevaMusica != null)
        {
            if (enableMusicTransitions && musicAudioSource.isPlaying)
            {
                // Transición suave
                if (musicTransitionCoroutine != null)
                {
                    StopCoroutine(musicTransitionCoroutine);
                }
                musicTransitionCoroutine = StartCoroutine(TransicionMusica(nuevaMusica));
            }
            else
            {
                // Cambio directo
                musicAudioSource.clip = nuevaMusica;
                musicAudioSource.Play();
            }
        }
        else
        {
            Debug.LogWarning($"VirusInvadersAudioManager: No hay música asignada para el nivel {nivel + 1}");
        }
    }
    
    IEnumerator TransicionMusica(AudioClip nuevaMusica)
    {
        float volumenOriginal = musicAudioSource.volume;
        
        // Fade out de la música actual
        float tiempoFadeOut = transitionDuration / 2f;
        float elapsed = 0f;
        
        while (elapsed < tiempoFadeOut)
        {
            elapsed += Time.deltaTime;
            musicAudioSource.volume = Mathf.Lerp(volumenOriginal, 0f, elapsed / tiempoFadeOut);
            yield return null;
        }
        
        // Cambiar música
        musicAudioSource.clip = nuevaMusica;
        musicAudioSource.Play();
        
        // Fade in de la nueva música
        elapsed = 0f;
        while (elapsed < tiempoFadeOut)
        {
            elapsed += Time.deltaTime;
            musicAudioSource.volume = Mathf.Lerp(0f, volumenOriginal, elapsed / tiempoFadeOut);
            yield return null;
        }
        
        musicAudioSource.volume = volumenOriginal;
        musicTransitionCoroutine = null;
    }
    
    public void IniciarMusicaJuego()
    {
        CambiarMusicaNivel(0); // Empezar con la música del nivel 1
    }
    
    public void ReproducirMusicaGameOver()
    {
        if (musicTransitionCoroutine != null)
        {
            StopCoroutine(musicTransitionCoroutine);
            musicTransitionCoroutine = null;
        }
        
        if (musicaGameOver != null && musicAudioSource != null)
        {
            musicAudioSource.Stop();
            musicAudioSource.loop = false; // Game over no debe hacer loop
            musicAudioSource.clip = musicaGameOver;
            musicAudioSource.Play();
        }
    }
    
    public void PararMusica()
    {
        if (musicAudioSource != null)
        {
            if (musicTransitionCoroutine != null)
            {
                StopCoroutine(musicTransitionCoroutine);
                musicTransitionCoroutine = null;
            }
            musicAudioSource.Stop();
        }
        currentMusicLevel = -1;
    }
    
    // === MÉTODOS DE EFECTOS DE SONIDO ===
    public void ReproducirSonidoDisparo()
    {
        ReproducirSFX(sonidoDisparo);
    }
    
    public void ReproducirSonidoImpacto()
    {
        ReproducirSFX(sonidoImpacto);
    }
    
    public void ReproducirEnemyHit()
    {
        ReproducirSFX(sonidoEnemyHit);
    }
    
    public void ReproducirExplosion()
    {
        ReproducirSFX(sonidoExplosion);
    }
    
    public void ReproducirMuerteEnemigo()
    {
        ReproducirSFX(sonidoEnemyDeath);
    }
    
    public void ReproducirMedKit()
    {
        ReproducirSFX(sonidoMedKit);
    }
    
    public void ReproducirPowerUp()
    {
        ReproducirSFX(sonidoPowerUp);
    }
    
    public void ReproducirLevelUp()
    {
        ReproducirSFX(sonidoLevelUp);
    }
    
    // Método genérico para reproducir SFX
    private void ReproducirSFX(AudioClip clip)
    {
        if (clip == null)
        {
            return;
        }
        
        if (sfxAudioSource == null)
        {
            Debug.LogError("VirusInvadersAudioManager: sfxAudioSource es null");
            return;
        }
        
        sfxAudioSource.PlayOneShot(clip, volumenSFX * volumenMaster);
    }
    
    // === MÉTODOS DE CONTROL DE VOLUMEN ===
    public void CambiarVolumenSFX(float nuevoVolumen)
    {
        volumenSFX = Mathf.Clamp01(nuevoVolumen);
        ActualizarVolumenes();
    }
    
    public void CambiarVolumenMusica(float nuevoVolumen)
    {
        volumenMusica = Mathf.Clamp01(nuevoVolumen);
        ActualizarVolumenes();
    }
    
    public void CambiarVolumenMaster(float nuevoVolumen)
    {
        volumenMaster = Mathf.Clamp01(nuevoVolumen);
        ActualizarVolumenes();
    }
    
    void ActualizarVolumenes()
    {
        if (sfxAudioSource != null)
            sfxAudioSource.volume = volumenSFX * volumenMaster;
            
        if (musicAudioSource != null)
            musicAudioSource.volume = volumenMusica * volumenMaster;
    }
    
    // === MÉTODOS DE CONTROL GENERAL ===
    public void MutearTodo(bool muted)
    {
        if (sfxAudioSource != null)
            sfxAudioSource.mute = muted;
            
        if (musicAudioSource != null)
            musicAudioSource.mute = muted;
    }
    
    public void PausarTodoElAudio()
    {
        if (sfxAudioSource != null)
            sfxAudioSource.Pause();
            
        if (musicAudioSource != null)
            musicAudioSource.Pause();
    }
    
    public void ReanudarTodoElAudio()
    {
        if (sfxAudioSource != null)
            sfxAudioSource.UnPause();
            
        if (musicAudioSource != null)
            musicAudioSource.UnPause();
    }
    
    // === GETTERS ===
    public int GetCurrentMusicLevel()
    {
        return currentMusicLevel;
    }
    
    public bool IsMusicPlaying()
    {
        return musicAudioSource != null && musicAudioSource.isPlaying;
    }
} 
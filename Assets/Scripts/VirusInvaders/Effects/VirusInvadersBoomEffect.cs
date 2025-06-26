using UnityEngine;
using System.Collections;

[System.Serializable]
public class VirusInvadersBoomEffect : MonoBehaviour
{
    [Header("VirusInvaders - Configuración del Efecto de Explosión")]
    public float animationSpeed = 0.03f;
    public float scaleMultiplier = 0.3f;
    public int maxFrames = 30;
    
    private SpriteRenderer spriteRenderer;
    private Sprite[] boomSprites;
    private int currentFrame = 0;
    private bool isPlaying = false;
    
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        
        spriteRenderer.sortingLayerName = "Characters";
        spriteRenderer.sortingOrder = 25;
        
        LoadBoomSprites();
    }
    
    void LoadBoomSprites()
    {
        System.Collections.Generic.List<Sprite> sprites = new System.Collections.Generic.List<Sprite>();
        
        Object[] allSprites = Resources.LoadAll("VirusInvaders/Sprites/boom1", typeof(Sprite));
        
        if (allSprites != null && allSprites.Length > 0)
        {
            foreach (Object obj in allSprites)
            {
                if (obj is Sprite sprite)
                {
                    sprites.Add(sprite);
                }
            }
            
            // Ordenar por nombre para asegurar secuencia correcta
            sprites.Sort((a, b) => {
                string aNum = a.name.Replace("boom1_", "");
                string bNum = b.name.Replace("boom1_", "");
                
                if (int.TryParse(aNum, out int aInt) && int.TryParse(bNum, out int bInt))
                {
                    return aInt.CompareTo(bInt);
                }
                return a.name.CompareTo(b.name);
            });
            
            int framesToUse = Mathf.Min(sprites.Count, maxFrames);
            boomSprites = new Sprite[framesToUse];
            
            for (int i = 0; i < framesToUse; i++)
            {
                boomSprites[i] = sprites[i];
            }
        }
        else
        {
            Debug.LogWarning("VirusInvaders: No se encontraron sprites de boom1! Creando efecto simple.");
            CreateSimpleExplosion();
        }
    }
    
    void CreateSimpleExplosion()
    {
        Texture2D texture = new Texture2D(16, 16);
        Color[] pixels = new Color[16 * 16];
        Vector2 center = new Vector2(8, 8);
        
        for (int x = 0; x < 16; x++)
        {
            for (int y = 0; y < 16; y++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                
                if (distance <= 3)
                {
                    pixels[y * 16 + x] = new Color(1f, 1f, 0.2f, 0.9f);
                }
                else if (distance <= 5)
                {
                    pixels[y * 16 + x] = new Color(1f, 0.6f, 0f, 0.7f);
                }
                else if (distance <= 8)
                {
                    pixels[y * 16 + x] = new Color(1f, 0.2f, 0f, 0.4f);
                }
                else
                {
                    pixels[y * 16 + x] = Color.clear;
                }
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        Sprite simpleSprite = Sprite.Create(texture, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f));
        boomSprites = new Sprite[] { simpleSprite };
    }
    
    public void PlayExplosion()
    {
        if (boomSprites == null || boomSprites.Length == 0) 
        {
            Debug.LogError("VirusInvaders: No hay sprites de explosión disponibles!");
            return;
        }
        
        isPlaying = true;
        currentFrame = 0;
        
        spriteRenderer.sprite = boomSprites[0];
        transform.localScale = Vector3.one * scaleMultiplier;
        spriteRenderer.color = Color.white;
        
        StartCoroutine(AnimateExplosion());
    }
    
    IEnumerator AnimateExplosion()
    {
        float frameTimer = 0f;
        
        while (isPlaying && currentFrame < boomSprites.Length)
        {
            frameTimer += Time.deltaTime;
            
            if (frameTimer >= animationSpeed)
            {
                currentFrame++;
                
                if (currentFrame >= boomSprites.Length)
                {
                    isPlaying = false;
                    Destroy(gameObject);
                    yield break;
                }
                
                spriteRenderer.sprite = boomSprites[currentFrame];
                frameTimer = 0f;
            }
            
            yield return null;
        }
    }
    
    // Método factory estático - explosión simple para impactos de bala
    public static void CreateExplosion(Vector3 position, float scale = 1f)
    {
        GameObject explosionGO = new GameObject("VirusInvadersBoomEffect");
        explosionGO.transform.position = position;
        
        VirusInvadersBoomEffect explosion = explosionGO.AddComponent<VirusInvadersBoomEffect>();
        explosion.scaleMultiplier = scale;
        
        explosion.PlayExplosion();
    }
} 
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Floor : MonoBehaviour
{
    [Header("Floor Settings")]
    public Color floorColor = new Color(0.18f, 0.72f, 0.36f, 1f); // Beautiful green color

    private static Sprite defaultSolidSprite;

    void Awake()
    {
        SetupFloor();
    }

    void OnValidate()
    {
        // Update visuals in the editor when colors/settings change
        SetupFloor();
    }

    private void SetupFloor()
    {
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        if (col != null)
        {
            col.size = new Vector2(1f, 1f);
            col.isTrigger = false; // Ensure it is not a trigger so the player can stand on it
        }

        if (sr != null)
        {
            sr.color = floorColor;
            
            // Only assign the dynamic fallback sprite during play mode to avoid SendMessage warnings in OnValidate
            if (Application.isPlaying && sr.sprite == null)
            {
                if (defaultSolidSprite == null)
                {
                    defaultSolidSprite = CreateSolidWhiteSprite();
                }
                sr.sprite = defaultSolidSprite;
            }
        }
    }

    private Sprite CreateSolidWhiteSprite()
    {
        Texture2D tex = new Texture2D(16, 16);
        tex.filterMode = FilterMode.Point;
        
        Color[] pixels = new Color[16 * 16];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.white;
        }
        
        tex.SetPixels(pixels);
        tex.Apply();
        
        return Sprite.Create(tex, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16f);
    }
}

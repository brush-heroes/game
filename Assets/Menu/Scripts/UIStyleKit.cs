using UnityEngine;
using UnityEngine.UI;

public static class UIStyleKit
{
    // ── Palette ────────────────────────────────────────────────────────────────
    public static readonly Color PageBg        = new Color(0.94f, 0.95f, 0.97f, 1.00f);
    public static readonly Color HeroPanelBg   = new Color(0.10f, 0.12f, 0.22f, 1.00f);
    public static readonly Color CardBg        = Color.white;
    public static readonly Color CardShadow    = new Color(0.00f, 0.00f, 0.00f, 0.14f);
    public static readonly Color Accent        = new Color(1.00f, 0.34f, 0.13f, 1.00f);
    public static readonly Color AccentDim     = new Color(1.00f, 0.50f, 0.30f, 1.00f);
    public static readonly Color TextOnDark    = new Color(0.96f, 0.97f, 1.00f, 1.00f);
    public static readonly Color TextSecDark   = new Color(0.60f, 0.67f, 0.82f, 1.00f);
    public static readonly Color TextOnLight   = new Color(0.11f, 0.14f, 0.22f, 1.00f);
    public static readonly Color TextSecLight  = new Color(0.42f, 0.46f, 0.58f, 1.00f);
    public static readonly Color NavBg         = Color.white;
    public static readonly Color NavActive     = Accent;
    public static readonly Color NavInactive   = new Color(0.60f, 0.63f, 0.72f, 1.00f);
    public static readonly Color GoldStreak    = new Color(1.00f, 0.74f, 0.08f, 1.00f);
    public static readonly Color BlueStars     = new Color(0.20f, 0.55f, 0.98f, 1.00f);
    public static readonly Color NavShadow     = new Color(0.00f, 0.00f, 0.00f, 0.06f);
    public static readonly Color CalToday      = Accent;
    public static readonly Color CalDotGreat   = new Color(0.20f, 0.78f, 0.36f, 1.00f);
    public static readonly Color CalDotOk      = new Color(1.00f, 0.80f, 0.15f, 1.00f);
    public static readonly Color CalDotMissed  = new Color(0.90f, 0.25f, 0.20f, 1.00f);

    // ── Rounded rect sprite (white 9-slice, 128×128 texture for quality) ──────
    // Returns a reusable sprite — caller should cache it, not call per-frame.
    public static Sprite RoundedSprite(int radius)
    {
        const int SIZE = 128;
        var tex = new Texture2D(SIZE, SIZE, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode   = TextureWrapMode.Clamp;
        float r  = radius;
        var  px  = new Color[SIZE * SIZE];
        for (int y = 0; y < SIZE; y++)
        for (int x = 0; x < SIZE; x++)
        {
            float d  = CornerDist(x, y, SIZE, SIZE, r);
            float al = d < r ? 1f : d < r + 1.5f ? 1f - (d - r) / 1.5f : 0f;
            px[y * SIZE + x] = new Color(1f, 1f, 1f, al);
        }
        tex.SetPixels(px);
        tex.Apply(false);
        return Sprite.Create(tex, new Rect(0, 0, SIZE, SIZE),
            new Vector2(0.5f, 0.5f), 1f, 0, SpriteMeshType.FullRect,
            new Vector4(r, r, r, r));
    }

    static float CornerDist(float x, float y, int w, int h, float r)
    {
        float nx = x < r ? r : x > w - 1 - r ? w - 1 - r : x;
        float ny = y < r ? r : y > h - 1 - r ? h - 1 - r : y;
        if (nx == x && ny == y) return 0f;
        float dx = x - nx, dy = y - ny;
        return Mathf.Sqrt(dx * dx + dy * dy);
    }

    // ── Apply helpers ──────────────────────────────────────────────────────────
    public static void ApplyRounded(Image img, Color color, int radius)
    {
        img.sprite = RoundedSprite(radius);
        img.type   = Image.Type.Sliced;
        img.color  = color;
    }

    public static void ApplyCard(Image img)         => ApplyRounded(img, CardBg,    32);
    public static void ApplyAccentButton(Image img) => ApplyRounded(img, Accent,    20);
    public static void ApplyBadge(Image img, Color c) => ApplyRounded(img, c,       18);

    // Simple circle (for calendar Today highlight)
    public static Sprite CircleSprite()
    {
        const int S = 64;
        var tex = new Texture2D(S, S, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        float cx = S * 0.5f, r = S * 0.5f - 1f;
        var px = new Color[S * S];
        for (int y = 0; y < S; y++)
        for (int x = 0; x < S; x++)
        {
            float d  = Mathf.Sqrt((x - cx) * (x - cx) + (y - cx) * (y - cx));
            float al = d < r ? 1f : d < r + 1.5f ? 1f - (d - r) / 1.5f : 0f;
            px[y * S + x] = new Color(1f, 1f, 1f, al);
        }
        tex.SetPixels(px);
        tex.Apply(false);
        return Sprite.Create(tex, new Rect(0, 0, S, S), new Vector2(0.5f, 0.5f));
    }
}

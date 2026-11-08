using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Score oben rechts, Wave-Ansagen und Game Over in der Bildmitte.
// Wird vom WaveManager automatisch erzeugt (kein Setup in der Szene nötig).
public class GameHUD : MonoBehaviour
{
    public static GameHUD Instance { get; private set; }

    private TextMeshProUGUI scoreText;

    // Herzen
    private RectTransform heartRow;
    private Image[] hearts = new Image[0];
    private float[] heartPunch = new float[0];
    private int shownHP = -1;
    private PlayerController player;
    private static Sprite heartSprite;
    private static readonly Color HeartFull = new Color(0.95f, 0.2f, 0.3f, 1f);
    private static readonly Color HeartEmpty = new Color(0.25f, 0.25f, 0.3f, 0.7f);
    private const float HeartSize = 64f;
    private const float HeartSpacing = 12f;
    private TextMeshProUGUI bannerText;
    private TextMeshProUGUI subText;
    private CanvasGroup bannerGroup;
    private Coroutine bannerRoutine;

    private int startHighScore;
    private int shownScore = -1;
    private float scorePunch;
    private bool gameOver;

    private static readonly Color Gold = new Color(1f, 0.85f, 0.2f);
    private static readonly Color BossRed = new Color(1f, 0.3f, 0.3f);

    public static GameHUD Ensure()
    {
        if (Instance != null) return Instance;

        GameObject go = new GameObject("Game HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
        Canvas canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        return go.AddComponent<GameHUD>(); // Awake setzt Instance
    }

    void Awake()
    {
        Instance = this;
        startHighScore = PlayerPrefs.GetInt("HighScore", 0);

        scoreText = CreateText("Score", transform, new Vector2(0.5f, 1), new Vector2(0, -20), new Vector2(600, 110), 38, TextAlignmentOptions.Top); // oben mittig, HP sitzt oben rechts

        // Banner in der Mitte (mit CanvasGroup zum Ein-/Ausblenden)
        GameObject banner = new GameObject("Banner", typeof(RectTransform), typeof(CanvasGroup));
        RectTransform br = banner.GetComponent<RectTransform>();
        br.SetParent(transform, false);
        br.anchorMin = br.anchorMax = new Vector2(0.5f, 0.62f);
        br.sizeDelta = new Vector2(1400, 300);
        bannerGroup = banner.GetComponent<CanvasGroup>();
        bannerGroup.alpha = 0f;
        bannerGroup.blocksRaycasts = false;

        bannerText = CreateText("Title", banner.transform, new Vector2(0.5f, 0.5f), new Vector2(0, 30), new Vector2(1400, 160), 120, TextAlignmentOptions.Center);
        bannerText.fontStyle = FontStyles.Bold;
        subText = CreateText("Sub", banner.transform, new Vector2(0.5f, 0.5f), new Vector2(0, -70), new Vector2(1400, 80), 52, TextAlignmentOptions.Center);
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Update()
    {
        UpdateHearts();
        if (gameOver) return;

        int score = ScoreManager.instance != null ? ScoreManager.instance.currentScore : 0;
        if (score != shownScore)
        {
            if (shownScore >= 0 && score > shownScore) scorePunch = 1f; // kleiner "Pop" bei Punkten
            shownScore = score;
            int best = Mathf.Max(startHighScore, score);
            scoreText.text = $"SCORE  {score}   <size=70%><color=#BBBBBB>BEST  {best}</color></size>";
        }

        scorePunch = Mathf.MoveTowards(scorePunch, 0f, Time.unscaledDeltaTime * 5f);
        scoreText.rectTransform.localScale = Vector3.one * (1f + 0.25f * scorePunch);
    }

    // --- Herzen (oben rechts) ---
    private void UpdateHearts()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) player = p.GetComponent<PlayerController>();
        }

        int maxHP = player != null ? Mathf.CeilToInt(player.MaxHP) : hearts.Length;
        int hp = player != null && !player.IsDead ? Mathf.Clamp(Mathf.CeilToInt(player.HP), 0, maxHP) : 0;

        if (maxHP > 0 && maxHP != hearts.Length) BuildHearts(maxHP);

        if (hp != shownHP)
        {
            // verlorene Herzen kurz "aufploppen" lassen
            if (shownHP > hp)
                for (int i = hp; i < shownHP && i < hearts.Length; i++) heartPunch[i] = 1f;
            shownHP = hp;
            for (int i = 0; i < hearts.Length; i++) hearts[i].color = i < hp ? HeartFull : HeartEmpty;
        }

        for (int i = 0; i < hearts.Length; i++)
        {
            heartPunch[i] = Mathf.MoveTowards(heartPunch[i], 0f, Time.unscaledDeltaTime * 4f);
            float scale = 1f + 0.5f * heartPunch[i];
            // letztes Herz pulsiert
            if (hp == 1 && i == 0) scale *= 1f + 0.12f * Mathf.Sin(Time.unscaledTime * 8f);
            hearts[i].rectTransform.localScale = Vector3.one * scale;
        }
    }

    private void BuildHearts(int count)
    {
        if (heartRow == null)
        {
            GameObject row = new GameObject("Hearts", typeof(RectTransform));
            heartRow = row.GetComponent<RectTransform>();
            heartRow.SetParent(transform, false);
            heartRow.anchorMin = heartRow.anchorMax = heartRow.pivot = new Vector2(1, 1);
            heartRow.anchoredPosition = new Vector2(-40, -30);
        }
        foreach (Transform child in heartRow) Destroy(child.gameObject);

        hearts = new Image[count];
        heartPunch = new float[count];
        heartRow.sizeDelta = new Vector2(count * HeartSize + (count - 1) * HeartSpacing, HeartSize);
        for (int i = 0; i < count; i++)
        {
            GameObject h = new GameObject("Heart " + (i + 1), typeof(RectTransform), typeof(Image));
            RectTransform rt = h.GetComponent<RectTransform>();
            rt.SetParent(heartRow, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(HeartSize, HeartSize);
            rt.anchoredPosition = new Vector2(HeartSize / 2f + i * (HeartSize + HeartSpacing), 0);
            Image img = h.GetComponent<Image>();
            img.sprite = GetHeartSprite();
            img.raycastTarget = false;
            hearts[i] = img;
        }
        shownHP = -1; // Farben neu setzen
    }

    // Herz-Sprite per Code erzeugt (Herzkurve (x²+y²-1)³ - x²y³ <= 0), damit kein Asset nötig ist
    private static Sprite GetHeartSprite()
    {
        if (heartSprite != null) return heartSprite;
        const int n = 64, ss = 4; // 4x4 Supersampling für weiche Kanten
        Texture2D tex = new Texture2D(n, n, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;
        Color32[] px = new Color32[n * n];
        for (int y = 0; y < n; y++)
        for (int x = 0; x < n; x++)
        {
            int inside = 0;
            for (int sy = 0; sy < ss; sy++)
            for (int sx = 0; sx < ss; sx++)
            {
                float u = ((x + (sx + 0.5f) / ss) / n) * 2.6f - 1.3f;
                float v = ((y + (sy + 0.5f) / ss) / n) * 2.6f - 1.2f;
                float a = u * u + v * v - 1f;
                if (a * a * a - u * u * v * v * v <= 0f) inside++;
            }
            byte alpha = (byte)(255 * inside / (ss * ss));
            px[y * n + x] = new Color32(255, 255, 255, alpha);
        }
        tex.SetPixels32(px);
        tex.Apply();
        heartSprite = Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), 100f);
        return heartSprite;
    }

    // --- Öffentliche Aufrufe ---
    public void ShowWave(int wave, bool boss)
    {
        if (boss) ShowBanner("BOSS WAVE", $"Wave {wave}", BossRed, 1.4f);
        else ShowBanner($"WAVE {wave}", "", Color.white, 1.1f);
    }

    public void ShowGameOver(int finalScore)
    {
        gameOver = true;
        bool newHighScore = finalScore > startHighScore && finalScore > 0;
        if (newHighScore) ShowBanner("NEW HIGHSCORE!", $"Score: {finalScore}", Gold, 3f);
        else ShowBanner("GAME OVER", $"Score: {finalScore}", Color.white, 3f);
    }

    public void ShowBanner(string title, string sub, Color color, float hold)
    {
        if (bannerRoutine != null) StopCoroutine(bannerRoutine);
        bannerRoutine = StartCoroutine(BannerRoutine(title, sub, color, hold));
    }

    private IEnumerator BannerRoutine(string title, string sub, Color color, float hold)
    {
        bannerText.text = title;
        bannerText.color = color;
        subText.text = sub;

        yield return Fade(0f, 1f, 0.2f, true);
        yield return new WaitForSecondsRealtime(hold);
        yield return Fade(1f, 0f, 0.4f, false);
        bannerRoutine = null;
    }

    private IEnumerator Fade(float from, float to, float time, bool zoomIn)
    {
        RectTransform rt = (RectTransform)bannerGroup.transform;
        for (float t = 0f; t < time; t += Time.unscaledDeltaTime)
        {
            float k = t / time;
            bannerGroup.alpha = Mathf.Lerp(from, to, k);
            if (zoomIn) rt.localScale = Vector3.one * Mathf.Lerp(1.3f, 1f, k);
            yield return null;
        }
        bannerGroup.alpha = to;
        rt.localScale = Vector3.one;
    }

    private static TextMeshProUGUI CreateText(string name, Transform parent, Vector2 anchor, Vector2 pos, Vector2 size, float fontSize, TextAlignmentOptions alignment)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = rt.anchorMax = anchor;
        rt.pivot = anchor;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        TextMeshProUGUI text = go.GetComponent<TextMeshProUGUI>();
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.raycastTarget = false;
        text.color = Color.white;
        return text;
    }
}

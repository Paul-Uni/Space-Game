using System.Collections;
using UnityEngine;

// Gemeinsame Basis für alle Gegner (basher, lasek, Bosse).
// Kümmert sich um: Bild betreten/verlassen, zum Spieler drehen, Schaden, Tod, Wave-Zählung, Hit-Flash.
// Unterklassen liefern nur noch ihr eigenes Angriffsmuster (Attack) und Extras.
public abstract class Enemy : MonoBehaviour, IDamageable, Boss_HP_Changer
{
    [Header("Stats")]
    public float HP = 1f;
    public float Damage = 2f;

    [Header("Audio")]
    public AudioClip EXPLOSION;
    public AudioSource aS;
    public AudioClip hitSound;

    [Header("Feedback")]
    public Color hitFlashColor = new Color(1f, 0.35f, 0.35f, 1f);
    public float deathShake = 0.12f;

    protected Camera cam;
    protected Rigidbody2D rb;
    protected GameObject player;
    protected WaveManager waveManager;
    protected bool dead;
    protected bool needToFacePlayer;

    private bool wasVisible;
    private Coroutine attackRoutine;
    private SpriteRenderer[] renderers;
    private Color[] baseColors;
    private Coroutine flashRoutine;

    private const float LeaveMargin = 0.05f;
    private const float DespawnDistance = 25f;
    private const float EnterSpeed = 2f;

    public bool IsDead => dead || HP <= 0f;

    // --- Das, was jede Gegnerart selbst festlegt ---
    protected abstract bool IsAttacking { get; }
    protected abstract IEnumerator Attack();
    protected virtual void OnLeftScreen() { }

    // --- Unity ---
    protected virtual void Awake()
    {
        cam = Camera.main;
        rb = GetComponent<Rigidbody2D>();
        aS = GetComponent<AudioSource>();

        renderers = GetComponentsInChildren<SpriteRenderer>();
        baseColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++) baseColors[i] = renderers[i].color;
    }

    protected virtual void Start()
    {
        GameObject wm = GameObject.FindWithTag("Wave");
        if (wm != null) waveManager = wm.GetComponent<WaveManager>();
        player = GameObject.FindWithTag("Player");
        EnterScreen();
    }

    protected virtual void FixedUpdate()
    {
        UpdateVisibility();
        FacePlayer();
        OnFixedUpdate();
        if (needToFacePlayer) rb.linearVelocity = Vector2.zero;
    }

    protected virtual void OnFixedUpdate() { }

    protected virtual void Update()
    {
        if (HP <= 0f && !dead) Die();
    }

    // --- Schaden & Tod ---
    // true = dieser Treffer hat den Gegner getötet
    public bool TakeDamage(float amount)
    {
        if (IsDead) return false;
        HP -= amount;
        Flash();
        return HP <= 0f;
    }

    public void SetMaxHP(float MaxHp)
    {
        HP = MaxHp;
    }

    protected void Die()
    {
        dead = true;
        if (waveManager != null) waveManager.EnemyDestroyed();
        PlaySoundAtPosition(EXPLOSION, transform.position);
        CameraShake.Shake(deathShake, 0.15f);
        Destroy(gameObject);
    }

    // Entfernen ohne Explosion (zu weit weg, Enemy Wall). Zählt für die Wave trotzdem.
    public void Despawn()
    {
        if (dead) return;
        dead = true;
        if (waveManager != null) waveManager.EnemyDestroyed();
        Destroy(gameObject);
    }

    private void Flash()
    {
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        for (int i = 0; i < renderers.Length; i++)
            if (renderers[i] != null) renderers[i].color = baseColors[i] * hitFlashColor;
        yield return new WaitForSeconds(0.08f);
        for (int i = 0; i < renderers.Length; i++)
            if (renderers[i] != null) renderers[i].color = baseColors[i];
        flashRoutine = null;
    }

    // --- Bildschirm betreten / verlassen ---
    // Prüft über die Main Camera, ob der Gegner im Bild ist.
    // (Renderer.isVisible / OnBecameInvisible zählen im Editor auch die Scene-View mit.)
    private bool IsInView(float margin)
    {
        Vector3 v = cam.WorldToViewportPoint(transform.position);
        return v.x > -margin && v.x < 1f + margin && v.y > -margin && v.y < 1f + margin;
    }

    private void UpdateVisibility()
    {
        // Rein zählt ab Bildrand, raus erst etwas außerhalb -> kein Flackern genau am Rand
        bool isVisible = wasVisible ? IsInView(LeaveMargin) : IsInView(0f);
        if (!wasVisible && isVisible && !IsAttacking)
        {
            attackRoutine = StartCoroutine(Attack());
        }
        else if (wasVisible && !isVisible)
        {
            LeftScreen();
        }
        wasVisible = isVisible;
    }

    private void LeftScreen()
    {
        if (dead) return;
        // Laufenden Angriff abbrechen, sonst greift er von außerhalb des Bildes an
        if (attackRoutine != null) { StopCoroutine(attackRoutine); attackRoutine = null; }
        needToFacePlayer = false;
        rb.linearVelocity = Vector2.zero;
        OnLeftScreen();
        EnterScreen();
    }

    public void EnterScreen()
    {
        if (dead) return;
        rb.freezeRotation = false;

        // Direkt Richtung Bildmitte fliegen
        Vector2 dir = ((Vector2)cam.transform.position - rb.position).normalized;
        rb.rotation = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        rb.linearVelocity = dir * EnterSpeed;

        // Zu weit weg -> entfernen
        if (player != null && Vector3.Distance(transform.position, player.transform.position) > DespawnDistance)
            Despawn();
    }

    // --- Helfer ---
    protected void FacePlayer()
    {
        if (!needToFacePlayer || player == null) return;
        Vector2 directionToPlayer = player.transform.position - transform.position;
        float angle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
        // -90, weil der Sprite nach "oben" schaut
        transform.rotation = Quaternion.Euler(0, 0, angle - 90);
    }

    protected static void PlaySoundAtPosition(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (clip == null) return;
        GameObject soundObject = new GameObject("TempAudio");
        soundObject.transform.position = position;
        AudioSource audioSource = soundObject.AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.Play();
        Destroy(soundObject, clip.length);
    }
}

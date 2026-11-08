using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;

public class PlayerController : MonoBehaviour, IDamageable
{
    public GameObject Dmg_text;

    //Decleration
    Rigidbody2D rb;
    public Camera cam;
    private SpriteRenderer sr;

    //HP
    public float HP = 4f;
    public float MaxHP;
    public float invulnerabilityTime = 1f; // nach einem Treffer kurz unverwundbar (blinkt)
    [Tooltip("An: jeder Treffer kostet genau 1 HP (= 1 Herz). Aus: Gegner-Damage zählt (z.B. für halbe Herzen).")]
    public bool oneHeartPerHit = true;
    private float invulnerableUntil;
    public bool IsDead => dead;
    //death
    private bool dead;
    public AudioClip death_Sound;
    public ParticleSystem explosionPrefab;


    //Movement
    public float speed = 10.0f;
    public float rotationSpeed = 5000f;
    private bool isMoving;
    //Dash
    public float dashForce = 5f;
    public float dashCd = 2;
    public float nextDashIn;

    //Shoothing
    //Laser
    public GameObject laser;
    public float fireRate = 0.5f;
    private float nextShotIn;

    //Energyball
    public GameObject ball;
    public float cd = 0.5f;
    private float nextShotIn1;

    //Ball Ring
    public GameObject ballPrefab;
    public float ballLifetime = 5f;
    public float ballCooldown = 10f;
    private float nextBallRingIn;
    private List<GameObject> activeBalls = new List<GameObject>();
    private List<Vector3> activeBallsRelativePositions = new List<Vector3>();

    //God
    private bool isGodMode = false;
    private int defaultLayer;
    private int godModeLayer;

    //Cd Managment
    public float alpha1Cooldown;
    public float alpha2Cooldown;
    public float alpha3Cooldown;

    void Start()
    {
        MaxHP = HP;
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        nextShotIn = 0f;
        nextShotIn1 = 0f;
        nextBallRingIn = 0f;

        GameObject damageText = Instantiate(Dmg_text, gameObject.transform.position, Quaternion.identity);
        damageText.GetComponent<Canvas>().worldCamera = Camera.main;
        Destroy(damageText);

        defaultLayer = gameObject.layer;
        godModeLayer = LayerMask.NameToLayer("GodMode");

        ballPrefab = ball;
    }

    void Update()
    {
        //HP Managment
        CheckIfDead();
        if (dead) return; // tot -> keine Eingaben mehr (Schiff ist 1s unsichtbar, konnte aber noch schießen/dashen)

        //Rotation
        // In Update statt FixedUpdate: FixedUpdate läuft nur 50x/s, dadurch drehte das Schiff
        // in festen 5°-Stufen (250°/s * 0.02s) und man konnte zwischen zwei Stufen nicht zielen.
        // Hier dreht es pro Frame -> so fein wie die Framerate (144 FPS ~ 1.7° pro Frame).
        float turn = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) turn += 1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) turn -= 1f;
        if (turn != 0f) transform.Rotate(0f, 0f, turn * rotationSpeed * Time.deltaTime);

        //Shooting
        if (Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0))
        {
            if (nextShotIn <= Time.time)
            {
                Instantiate(laser, transform.position, transform.rotation);
                nextShotIn = Time.time + fireRate;
            }
        }

        if (Input.GetKey(KeyCode.Alpha1) || Input.GetMouseButton(1))
        {
            if (nextShotIn1 <= Time.time)
            {
                //Straight
                Instantiate(ball, transform.position, transform.rotation);
                //Left
                Instantiate(ball, transform.position, transform.rotation * Quaternion.Euler(0, 0, -10));
                //Right
                Instantiate(ball, transform.position, transform.rotation * Quaternion.Euler(0, 0, 10));
                nextShotIn1 = Time.time + cd;
            }
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (nextDashIn <= Time.time)
            {
                Vector2 dashDirection = transform.up;
                rb.AddForce(dashDirection * dashForce, ForceMode2D.Impulse);
                nextDashIn = Time.time + dashCd;
            }
        }

        if (Debug.isDebugBuild && Input.GetKeyDown(KeyCode.G)) // God-Mode nur im Editor/Development-Build
        {
            isGodMode = !isGodMode;
            gameObject.layer = isGodMode ? godModeLayer : defaultLayer;
            Physics2D.IgnoreLayerCollision(godModeLayer, defaultLayer, isGodMode);
        }

        if (Input.GetMouseButtonDown(2) || Input.GetKeyDown(KeyCode.Alpha3))
        {
            if (nextBallRingIn <= Time.time)
            {
                CreateBallRing();
                nextBallRingIn = Time.time + ballCooldown;
            }
        }
        UpdateBallRing();

        alpha1Cooldown = nextShotIn1 - Time.time;
        alpha2Cooldown = nextDashIn - Time.time;
        alpha3Cooldown = nextBallRingIn - Time.time;

        if (alpha1Cooldown < 0) alpha1Cooldown = 0;
        if (alpha2Cooldown < 0) alpha2Cooldown = 0;
        if (alpha3Cooldown < 0) alpha3Cooldown = 0;
    }

    void FixedUpdate()
    {
        if (dead) return;
        //Movement
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            Vector2 forwardMovement = new Vector2(transform.up.x, transform.up.y) * speed;
            rb.AddForce(forwardMovement);
        }
    }

    private void CheckIfDead()
    {
        if (HP <= 0f && !dead && !isGodMode)
        {
            dead = true;
            
            sr.GetComponent<Renderer>().enabled = false;
            Destroy(gameObject, 2.5f); // muss länger leben als LoadMainMenuAfterDelay, sonst stirbt die Coroutine mit
            PlaySoundAtPosition(death_Sound, transform.position);
            Instantiate(explosionPrefab, gameObject.transform.position, Quaternion.identity);
            CameraShake.Shake(0.45f, 0.6f);

            // Game Over / New Highscore anzeigen (Score vor dem Zurücksetzen auslesen)
            int finalScore = ScoreManager.instance != null ? ScoreManager.instance.currentScore : 0;
            if (GameHUD.Instance != null) GameHUD.Instance.ShowGameOver(finalScore);

            // Instanzen existieren nur, wenn das Spiel über das MainMenu gestartet wurde
            if (MusicManager.instance != null) StartCoroutine(MusicManager.instance.FadeOut(1f));
            if (ScoreManager.instance != null) ScoreManager.instance.currentScore = 0;

            StartCoroutine(LoadMainMenuAfterDelay(2f));
        }
    }

    // Schaden nehmen. false = ignoriert (tot, God-Mode oder noch unverwundbar)
    public bool TakeDamage(float amount)
    {
        if (dead || isGodMode || Time.time < invulnerableUntil) return false;

        HP -= oneHeartPerHit ? 1f : amount;
        CameraShake.Shake(0.25f, 0.25f);
        if (HP > 0f)
        {
            invulnerableUntil = Time.time + invulnerabilityTime;
            StartCoroutine(Blink());
        }
        return true;
    }

    IEnumerator Blink()
    {
        while (Time.time < invulnerableUntil && !dead)
        {
            sr.enabled = !sr.enabled;
            yield return new WaitForSeconds(0.08f);
        }
        if (!dead) sr.enabled = true;
    }

    void PlaySoundAtPosition(AudioClip clip, Vector3 position, float volume = 1f)
    {
        // Create a new GameObject at the specified position
        GameObject soundObject = new GameObject("TempAudio");
        soundObject.transform.position = position;

        // Add an AudioSource component and configure it
        AudioSource audioSource = soundObject.AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.volume = volume;

        // Play the sound
        audioSource.Play();
        // Destroy the GameObject once the sound has finished playing
        Destroy(soundObject, clip.length);
    }

    IEnumerator LoadMainMenuAfterDelay(float delay)
    {
        // Wait for the specified delay
        yield return new WaitForSeconds(delay);

        // Load the main menu
        SceneManager.LoadScene("MainMenu");
    }
    void CreateBallRing()
    {
        for (int i = 0; i < 360; i += 30) // Adjust the increment to control the number of balls
        {
            // Calculate ball position
            Vector3 ballPosition = transform.position + new Vector3(Mathf.Cos(i * Mathf.Deg2Rad), Mathf.Sin(i * Mathf.Deg2Rad), 0);

            // Create ball
            GameObject ball = Instantiate(ballPrefab, ballPosition, Quaternion.identity);
            ball.transform.SetParent(transform);

            // Add ball and its relative position to lists
            activeBalls.Add(ball);
            activeBallsRelativePositions.Add(ball.transform.position - transform.position);

            // Destroy ball after specified lifetime
            Destroy(ball, ballLifetime);
        }

        // Remove destroyed balls from list after specified lifetime
        StartCoroutine(RemoveBallsAfterDelay(ballLifetime));
    }

    void UpdateBallRing()
    {
        for (int i = 0; i < activeBalls.Count; i++)
        {
            if (activeBalls[i] != null)
            {
                // Apply rotation to relative position
                float rotationSpeed = -30f; // Adjust this to control the speed of rotation, negative for clockwise
                activeBallsRelativePositions[i] = Quaternion.Euler(0, 0, rotationSpeed * Time.deltaTime) * activeBallsRelativePositions[i];

                // Update ball position
                activeBalls[i].transform.position = transform.position + activeBallsRelativePositions[i];
            }
        }
    }

    IEnumerator RemoveBallsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Clear lists
        activeBalls.Clear();
        activeBallsRelativePositions.Clear();
    }
}
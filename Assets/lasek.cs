using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lasek : MonoBehaviour, Boss_HP_Changer
{
    Camera cam;
    private GameObject waveManager;

    //Audio
    public AudioClip EXPLOSION;
    public AudioSource aS;
    public AudioClip hitSound;

    //Decleration
    private Rigidbody2D rb;
    private GameObject player;
    private SpriteRenderer sr;

    //position
    Vector2 stayhere;

    //Attacking
    public GameObject laser;
    private bool wasVisible = false;
    private bool needToFacePlayer;
    public float Damage = 2f;

    public float firerate = 1f;
    private float nextShotIn;
    private bool isFireing;
    //hit check

    //HP
    public float HP = 1f;
    private bool dead;

    private void Awake()
    {
        cam = Camera.main;
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        aS = GetComponent<AudioSource>();
    }

    // Start is called before the first frame update
    void Start()
    {
        waveManager = GameObject.FindWithTag("Wave");
        player = GameObject.FindWithTag("Player");
        EnterScreen();
        nextShotIn = 0;
    }

    void FixedUpdate()
    {
        OnBecomeVisable();
        FacePlayer();
        if (needToFacePlayer) { rb.velocity = Vector2.zero;}
    }

    private void Update()
    {
        Death();

        if (isFireing && nextShotIn < Time.time)
        {
            GameObject Laser = Instantiate(laser, transform.position, transform.rotation);
            Laser.GetComponent<EnemyChaser>().Damage = Damage;
            nextShotIn = Time.time + firerate;
            rb.position = stayhere;
        }
    }

    private void OnBecameInvisible()
    {
        rb.freezeRotation = true;
        rb.velocity = Vector2.zero;

        EnterScreen(); 
    }

    IEnumerator Fire()
    {
        yield return new WaitForSeconds(1f);

        //Position & Movement
        stayhere = transform.position;
        rb.velocity = Vector2.zero;

        yield return new WaitForSeconds(0.25f);
        //Fireing
        needToFacePlayer = true;
        yield return new WaitForSeconds(0.25f);
        isFireing = true;
    }

    private void FacePlayer()
    {
        if (needToFacePlayer)
        {
            Vector2 directionToPlayer = player.transform.position - transform.position;

            // Calculate the angle towards the player
            float angle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;

            // Apply the rotation (subtracting 90 degrees to align with the sprite's forward direction)
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 90));
        }  
    }

    private void OnBecomeVisable()
    {
        bool isVisible = GetComponent<Renderer>().isVisible;
        // If the object was not visible last frame, but is visible this frame
        if (!wasVisible && isVisible && !isFireing)
        {   // Your custom function to handle when the object becomes visible
            StartCoroutine(Fire());
        }
        wasVisible = isVisible;
    }

    public void EnterScreen()
    { 
        rb.freezeRotation = false;
        // Get the position of the object in the viewport space (values between 0 and 1)
        Vector3 viewportPosition = cam.WorldToViewportPoint(rb.transform.position);

        if (viewportPosition.x < 0)
        {
            // Object is off the screen to the left
            rb.rotation = 270;
            rb.velocity = new Vector2(2,0);
        }
        else if (viewportPosition.x > 0.9)
        {
            // Object is off the screen to the right
            rb.rotation = 90;
            rb.velocity = new Vector2(-2, 0);
        }

        if (viewportPosition.y < 0)
        {
            // Object is off the screen at the bottom
            rb.rotation = 0;
            rb.velocity = new Vector2(0, 2);
        }
        else if (viewportPosition.y > 0.9)
        {
            // Object is off the screen at the top
            rb.rotation = 180;
            rb.velocity = new Vector2(0, -2);
        }
    }

    void Death()
    {
        if (HP <= 0f && !dead)
        {
            waveManager.GetComponent<WaveManager>().EnemyDestroyed();
            dead = true;
            PlaySoundAtPosition(EXPLOSION, transform.position);
            Destroy(gameObject);
        }
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
    public void SetMaxHP(float MaxHp)
    {
        HP = MaxHp;
    }
}


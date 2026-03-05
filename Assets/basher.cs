using System.Collections;
using UnityEngine;

public class basher : MonoBehaviour, Boss_HP_Changer
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
    

    //Attacking
    private float chargeSpeed;
    public float chargeVelocity = 5;
    private bool wasVisible = false;
    private bool needToFacePlayer;
    private bool isCharging;
    public float Damage = 2f;
    //hii check
    private bool hit;

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
    }

    void FixedUpdate()
    {
        OnBecomeVisable();
        FacePlayer();
        Stuckcheck();
        if (needToFacePlayer) { rb.velocity = Vector2.zero;}
    }

    private void Update()
    {
        Death();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null && !hit)
            {
                hit = true;
                GetComponent<Collider2D>().isTrigger = true;
                if (player.HP > Damage) aS.PlayOneShot(hitSound);
                player.HP -= Damage;
            }
        }
    }

    private void OnBecameInvisible()
    {
        rb.freezeRotation = true;
        rb.velocity = Vector2.zero;
        isCharging = false;
        if (hit) { GetComponent<Collider2D>().isTrigger = false; }
        hit = false;

        EnterScreen(); 
    }

    IEnumerator DashCharge()
    {
        yield return new WaitForSeconds(0.5f);
        rb.velocity = Vector2.zero;

        chargeSpeed = Random.Range(2f, 4f);
        needToFacePlayer = true;

        yield return new WaitForSeconds(chargeSpeed);
        needToFacePlayer = false;
        rb.velocity = transform.up * chargeVelocity;
        isCharging = true;
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
        if (!wasVisible && isVisible && !isCharging)
        {   // Your custom function to handle when the object becomes visible
            StartCoroutine(DashCharge());
        }
        wasVisible = isVisible;
    }

    public void EnterScreen()
    { 
        rb.freezeRotation = false;
        // Get the position of the object in the viewport space (values between 0 and 1)
        Vector3 viewportPosition = cam.WorldToViewportPoint(rb.transform.position);

        if (viewportPosition.x < 1)
        {
            // Object is off the screen to the left
            rb.rotation = 270;
            rb.velocity = new Vector2(2,0);
        }
        else if (viewportPosition.x > 1)
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
        else if (viewportPosition.y > 1)
        {
            // Object is off the screen at the top
            rb.rotation = 180;
            rb.velocity = new Vector2(0, -2);
        }

        if (Vector3.Distance(gameObject.transform.position, player.transform.position) > 25)
        {
            Destroy(gameObject);
            waveManager.GetComponent<WaveManager>().EnemyDestroyed();
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

    void Stuckcheck()
    {
        if (isCharging && rb.velocity == Vector2.zero) 
        {
            rb.rotation -= 180f;
            rb.velocity = new Vector2(5f,5f);
        }
    }

    public void SetMaxHP(float MaxHp)
    {
        HP = MaxHp;
    }
}

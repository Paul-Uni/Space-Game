using System.Collections;
using UnityEngine;

public class EnemyChaser : MonoBehaviour
{
    public float Damage = 1f;

    //Projectile
    public Sprite Explosion;
    public Sprite Explosion2;
    public float laser_speed = 50;
    private Rigidbody2D rb;
    private SpriteRenderer sprite;
    bool hit = false;

    //Sound Handeling
    public AudioClip hitSound; // Drag your audio clip in the inspector
    private AudioSource aS;

    //Explosion
    public GameObject explosionPrefab;

    void Start()
    {
        aS = GetComponent<AudioSource>();

        sprite = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        Vector2 forwardMovement = new Vector2(transform.up.x, transform.up.y) * laser_speed;
        rb.velocity = forwardMovement;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        rb.velocity = Vector2.zero;
        sprite.sprite = Explosion;
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
        Destroy(gameObject, 0.2f);
        StartCoroutine(ExplosionAnim());
    }

    IEnumerator ExplosionAnim()
    {
        sprite.sprite = Explosion;

        // Wait for 2 seconds.
        yield return new WaitForSeconds(0.1f);

        sprite.sprite = Explosion2;
    }

    void OnBecameInvisible()
    {
        Destroy(gameObject,1f);
    }
}

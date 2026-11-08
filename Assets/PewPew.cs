using System.Collections;
using UnityEngine;

// Normaler Laser (Leertaste / Linksklick)
public class PewPew : MonoBehaviour
{
    public float Damage = 1f;

    //Dmg handeling
    public GameObject Dmg_text;

    //Projectile
    public Sprite Explosion;
    public Sprite Explosion2;
    public float laser_speed = 50;
    private Rigidbody2D rb;
    private SpriteRenderer sprite;
    bool hit = false;

    //Sound Handeling
    public AudioClip hitSound;
    private AudioSource audioSource;

    //Explosion
    public GameObject explosionPrefab;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        Destroy(gameObject, 8f); // Fallback, falls OnBecameInvisible nie kommt (Scene-View im Editor)

        sprite = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = (Vector2)transform.up * laser_speed;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        rb.linearVelocity = Vector2.zero;
        if (collision.gameObject.CompareTag("Enemy"))
        {
            sprite.sprite = Explosion;
            if (!hit) hit = ProjectileUtils.HitEnemy(collision, Damage, audioSource, hitSound, explosionPrefab, Dmg_text);
        }
        Destroy(gameObject, 0.2f);
        StartCoroutine(ExplosionAnim());
    }

    IEnumerator ExplosionAnim()
    {
        sprite.sprite = Explosion;
        yield return new WaitForSeconds(0.1f);
        sprite.sprite = Explosion2;
    }

    void OnBecameInvisible()
    {
        Destroy(gameObject, 6f);
    }
}

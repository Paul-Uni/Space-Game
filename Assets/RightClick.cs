using System.Collections;
using UnityEngine;

// Energiekugel (Taste 1 / Rechtsklick, auch für den Ballring)
public class RightClick : MonoBehaviour
{
    public GameObject Dmg_text;

    public float Damage = 2f;

    public Sprite Explosion;
    public Sprite Explosion2;

    public float laser_speed = 50;

    public AudioClip hitSound;
    private Rigidbody2D rb;
    private SpriteRenderer sprite;
    private AudioSource audioSource;

    bool hit = false;

    //Explosion
    public GameObject explosionPrefab;

    void Start()
    {
        StartCoroutine(KillMyself());
        audioSource = GetComponent<AudioSource>();
        sprite = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = (Vector2)transform.up * laser_speed;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag("Enemy")) return;

        sprite.sprite = Explosion;
        rb.linearVelocity = Vector2.zero;
        if (!hit) hit = ProjectileUtils.HitEnemy(collision, Damage, audioSource, hitSound, explosionPrefab, Dmg_text);

        Destroy(gameObject, 0.2f);
        StartCoroutine(ExplosionAnim());
    }

    IEnumerator ExplosionAnim()
    {
        sprite.sprite = Explosion;
        yield return new WaitForSeconds(0.1f);
        sprite.sprite = Explosion2;
    }

    IEnumerator KillMyself()
    {
        yield return new WaitForSeconds(5f);
        Destroy(gameObject);
    }

    void OnBecameInvisible()
    {
        Destroy(gameObject, 1f);
    }
}

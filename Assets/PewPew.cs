using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

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
    public AudioClip hitSound; // Drag your audio clip in the inspector
    private AudioSource audioSource;

    //Explosion
    public GameObject explosionPrefab;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

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
        if (collision.gameObject.CompareTag("Enemy"))
        {
            sprite.sprite = Explosion;

            //StopMovement
            collision.attachedRigidbody.velocity = Vector2.zero;

            basher basherScript = collision.gameObject.GetComponent<basher>();
            lasek lasekScript = collision.gameObject.GetComponent<lasek>();

            if (basherScript != null && !hit)
            {
                hit = true;
                if (basherScript.HP > Damage) audioSource.PlayOneShot(hitSound);
                basherScript.HP -= Damage;
                if (basherScript.HP <= 0)
                {
                    Instantiate(explosionPrefab, collision.transform.position, Quaternion.identity);
                    ScoreManager.instance.AddScore(1);
                }
                //Dmg text
                GameObject damageText = Instantiate(Dmg_text, collision.transform.position, Quaternion.identity);
                damageText.GetComponent<Canvas>().worldCamera = Camera.main;
                Destroy(damageText,0.19f);
            }

            if (lasekScript != null && !hit)
            {
                hit = true;
                if (lasekScript.HP > Damage) audioSource.PlayOneShot(hitSound);
                lasekScript.HP -= Damage;
                if (lasekScript.HP <= 0)
                {
                    Instantiate(explosionPrefab, collision.transform.position, Quaternion.identity);
                    ScoreManager.instance.AddScore(1);
                }
                //Dmg text
                GameObject damageText = Instantiate(Dmg_text, collision.transform.position, Quaternion.identity);
                damageText.GetComponent<Canvas>().worldCamera = Camera.main;
                Destroy(damageText, 0.19f);
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
        Destroy(gameObject,6f);
    }
}

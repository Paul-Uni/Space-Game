using System.Collections;
using UnityEngine;

// Rammer: fliegt ins Bild, zielt 2-4 s auf den Spieler und rammt dann geradeaus durch.
public class basher : Enemy
{
    [Header("Charge")]
    public float chargeVelocity = 5;

    private float chargeSpeed;
    private bool isCharging;
    private bool hit; // pro Rammangriff nur einmal Schaden

    protected override bool IsAttacking => isCharging;

    protected override IEnumerator Attack()
    {
        yield return new WaitForSeconds(0.5f);
        rb.linearVelocity = Vector2.zero;

        chargeSpeed = Random.Range(2f, 4f);
        needToFacePlayer = true;

        yield return new WaitForSeconds(chargeSpeed);
        needToFacePlayer = false;
        rb.linearVelocity = transform.up * chargeVelocity;
        isCharging = true;
    }

    protected override void OnLeftScreen()
    {
        isCharging = false;
        if (hit) GetComponent<Collider2D>().isTrigger = false;
        hit = false;
    }

    protected override void OnFixedUpdate()
    {
        // z.B. von einem Schuss gestoppt -> in Blickrichtung weiter rammen
        if (isCharging && rb.linearVelocity == Vector2.zero)
            rb.linearVelocity = transform.up * chargeVelocity;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player") || hit) return;

        PlayerController target = collision.gameObject.GetComponent<PlayerController>();
        if (target == null) return;

        hit = true;
        GetComponent<Collider2D>().isTrigger = true; // durch den Spieler durchfliegen
        if (target.TakeDamage(Damage) && target.HP > 0) aS.PlayOneShot(hitSound);
    }
}

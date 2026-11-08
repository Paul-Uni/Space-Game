using System.Collections;
using UnityEngine;

// Schütze: fliegt ins Bild, bleibt stehen, dreht sich zum Spieler und feuert regelmäßig.
public class lasek : Enemy
{
    [Header("Laser")]
    public GameObject laser;
    public float firerate = 1f;

    private float nextShotIn;
    private bool isFireing;
    private Vector2 stayhere;

    protected override bool IsAttacking => isFireing;

    protected override IEnumerator Attack()
    {
        yield return new WaitForSeconds(1f);

        // Position merken & anhalten
        stayhere = transform.position;
        rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(0.25f);
        needToFacePlayer = true;
        yield return new WaitForSeconds(0.25f);
        isFireing = true;
    }

    protected override void OnLeftScreen()
    {
        isFireing = false;
    }

    protected override void Update()
    {
        base.Update();
        if (dead || !isFireing || nextShotIn >= Time.time) return;

        GameObject shot = Instantiate(laser, transform.position, transform.rotation);
        shot.GetComponent<EnemyChaser>().Damage = Damage;
        nextShotIn = Time.time + firerate;
        rb.position = stayhere; // falls er weggeschubst wurde
    }
}

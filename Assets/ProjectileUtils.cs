using TMPro;
using UnityEngine;

// Gemeinsame Treffer-Logik für die Spieler-Projektile (PewPew, RightClick).
public static class ProjectileUtils
{
    // Gibt true zurück, wenn ein noch lebender Gegner getroffen wurde.
    public static bool HitEnemy(Collider2D collision, float damage, AudioSource audioSource, AudioClip hitSound,
                                GameObject explosionPrefab, GameObject dmgTextPrefab)
    {
        Enemy enemy = collision.GetComponentInParent<Enemy>();
        if (enemy == null || enemy.IsDead) return false; // schon tot -> kein doppelter Score

        // Gegner anhalten
        if (collision.attachedRigidbody != null) collision.attachedRigidbody.linearVelocity = Vector2.zero;

        if (enemy.HP > damage && audioSource != null && hitSound != null) audioSource.PlayOneShot(hitSound);

        bool killed = enemy.TakeDamage(damage);
        if (killed)
        {
            if (explosionPrefab != null) Object.Instantiate(explosionPrefab, collision.transform.position, Quaternion.identity);
            if (ScoreManager.instance != null) ScoreManager.instance.AddScore(1);
        }

        // Schadenszahl
        if (dmgTextPrefab != null)
        {
            GameObject damageText = Object.Instantiate(dmgTextPrefab, collision.transform.position, Quaternion.identity);
            Canvas canvas = damageText.GetComponent<Canvas>();
            if (canvas != null) canvas.worldCamera = Camera.main;
            TMP_Text text = damageText.GetComponentInChildren<TMP_Text>();
            if (text != null) text.text = damage.ToString();
            Object.Destroy(damageText, 0.19f);
        }
        return true;
    }
}

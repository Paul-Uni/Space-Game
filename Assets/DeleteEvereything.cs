using UnityEngine;

// "Enemy Wall" weit außerhalb des Bildes: entfernt verirrte Gegner.
public class DeleteEvereything : MonoBehaviour
{
    public WaveManager waveManager;

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Enemy")) return;

        Enemy enemy = collision.gameObject.GetComponentInParent<Enemy>();
        if (enemy != null)
        {
            enemy.Despawn(); // zählt genau einmal für die Wave
        }
        else
        {
            Destroy(collision.gameObject);
            if (waveManager != null) waveManager.EnemyDestroyed();
        }
    }
}

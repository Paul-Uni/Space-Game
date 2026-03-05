using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class DeleteEvereything : MonoBehaviour
{
    public WaveManager waveManager;
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Destroy(collision.gameObject);
            waveManager.GetComponent<WaveManager>().EnemyDestroyed();
        }
    }
}

using System.Collections;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    //Enemys zeitlich spawnen mit log
    //Liste von spawn point umsortieren und durch loopen

    public GameObject[] enemyPrefabs; // Array of enemy prefabs
    public GameObject[] Bosses; // Array of enemy prefabs
    public Transform[] spawnPoints; // Where enemies can spawn from
    private int currentWave = 0;
    private int enemiesLeft;
    public int enemyMultiplyer = 2;
    private int enemysSpawned;
    private int x; //For Enemy Spawning Function
    private bool spawned;

    void Start()
    {

    }

    void Update()
    {
        if (enemiesLeft <= 0 && !spawned)
        {
            StartCoroutine(Wait());
        }
    }

    public void EnemyDestroyed()
    {
        // Decrement the number of enemies left in the wave
        enemiesLeft--;
        if (enemysSpawned < x && currentWave % 5 != 0)
        {
            if (currentWave < 5)
            {
                Instantiate(enemyPrefabs[Random.Range(0, 1)], spawnPoints[Random.Range(0, spawnPoints.Length)]);
                enemysSpawned++;
                enemiesLeft++;
            }
            else
            {
                Instantiate(enemyPrefabs[Random.Range(0, enemyPrefabs.Length)], spawnPoints[Random.Range(0, spawnPoints.Length)]);
                enemysSpawned++;
                enemiesLeft++;
            }
        }
        if (enemiesLeft == 0) { spawned = false; };
    }

    void SpawnEnemys()
    {
        spawned = true;
        SchuffleSpawnPoints();
        x = AmountOfEnemys();

        if (currentWave % 5 == 0) //Boss
        {
            int BossAmount = currentWave / 5;
            for (int j = 0; j < BossAmount; j++)
            {
                if (j < 2)
                {
                    GameObject Boss = Instantiate(Bosses[Random.Range(0, Bosses.Length)], spawnPoints[j]);
                    Boss.GetComponent <Boss_HP_Changer>().SetMaxHP(currentWave);
                    enemiesLeft++;
                }
                else break;
            }
        }
        else
        {
            if (currentWave < 5)
            {
                for (int j = 0;j < x; j++)
                {
                    Instantiate(enemyPrefabs[Random.Range(0, 2)], spawnPoints[j]);
                    enemiesLeft++;
                }
            }
            else
            {
                for (int j = 0; j < x; j++)
                {
                    if (j <= 4)
                    {
                        Instantiate(enemyPrefabs[Random.Range(0, enemyPrefabs.Length)], spawnPoints[j]);
                        enemysSpawned++;
                        enemiesLeft++;
                    }
                    else break;
                }
            }
        }
        
    }

    int AmountOfEnemys()
    {
        return (int)Mathf.Ceil(Mathf.Log(currentWave+2,5)*enemyMultiplyer);
    }

    void SchuffleSpawnPoints()
    {
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            Transform x = spawnPoints[i];
            int RandomVar = Random.Range(0, spawnPoints.Length);
            spawnPoints[i] = spawnPoints[RandomVar];
            spawnPoints[RandomVar] = x;
        }
    }

    private IEnumerator Wait()
    {
        spawned = true;
        yield return new WaitForSeconds(2f);
        Debug.Log($"Momentane Wave:" + currentWave);
        enemiesLeft = 0;
        enemysSpawned = 0;
        currentWave++;
        SpawnEnemys();
    }
}

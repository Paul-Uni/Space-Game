using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    // Singleton pattern to access ScoreManager from anywhere
    public static ScoreManager instance;

    public int currentScore;

    void Awake()
    {
        // Der ScoreManager hängt am Ship-Prefab. Vorher hat DontDestroyOnLoad dadurch das
        // SPIELERSCHIFF in die nächste Szene mitgenommen, und ein zweiter ScoreManager
        // hätte per Destroy(gameObject) das Schiff gelöscht. Der Highscore liegt eh in den PlayerPrefs.
        instance = this;
    }

    void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    public void AddScore(int amount)
    {
        currentScore += amount;
        CheckHighScore();
    }

    void CheckHighScore()
    {
        int highScore = PlayerPrefs.GetInt("HighScore", 0);

        if (currentScore > highScore)
        {
            PlayerPrefs.SetInt("HighScore", currentScore);
            PlayerPrefs.Save(); // ohne Save geht der Highscore z.B. in WebGL-Builds verloren
        }
    }
}

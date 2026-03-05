using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    // Singleton pattern to access ScoreManager from anywhere
    public static ScoreManager instance;

    public int currentScore;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
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
        }
    }
}

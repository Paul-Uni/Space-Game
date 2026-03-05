using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public TextMeshProUGUI highScoreText;  // Assign this in the Inspector.
     
    void Start()
    {   
        // Get the high score from PlayerPrefs and display it.
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        highScoreText.text = "High Score: " + highScore;
    }

    public void StartGame()
    {
        // Load the game scene when the Start button is clicked.
        SceneManager.LoadScene("GameScene");
    }
}

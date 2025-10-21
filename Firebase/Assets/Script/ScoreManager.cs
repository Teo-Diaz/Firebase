using TMPro;
using UnityEngine;
using UnityEngine.UI; 

public class ScoreManager : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highScoreText; 
    private int score;
    private int highScore; 


    void Start()
    {
        score = 0;
        highScore = PlayerPrefs.GetInt("HightScore", 0); 
        UpdateScoreText();
    }

    public void IncreaseScore(int amount)
    {
        score += amount;
        UpdateScoreText();
    }

    void UpdateScoreText()
    {
        if (score > highScore) 
        {
            highScore = score;
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
        }
        scoreText.text = "Score: " + score.ToString();
        highScoreText.text = "High Score: " + highScore.ToString();
    }

    public void OnPlayerLose()
    {
        if (AuthManager.Instance != null)
        {
            AuthManager.Instance.SaveAndSendScore(highScore);
        }
        else
        {
            Debug.LogError("AuthManager no encontrado.");
        }
    }

    public int GetScore()
    {
        return score;
    }
}

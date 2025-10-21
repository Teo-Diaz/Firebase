using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; 
using TMPro; 

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public bool isGameOver = false;
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverHighScoreText;
    public GameObject authManagerPrefab;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (FindObjectOfType<AuthManager>() == null)
        {
            // Instanciar el AuthManager si no existe
            Instantiate(authManagerPrefab);
            Debug.Log("AuthManager instanciado.");
        }
        else
        {
            Debug.Log("AuthManager ya existe en la escena.");
        }
    }

    public void GameOver()
    {
        if (!isGameOver)
        {
            isGameOver = true;
            Debug.Log("Game Over!");
            Time.timeScale = 0; 
            gameOverPanel.SetActive(true); 

            ScoreManager scoreManager = FindObjectOfType<ScoreManager>();
            if(scoreManager != null)
            {
                int finalScore = scoreManager.GetScore();
                int highScore = PlayerPrefs.GetInt("HighScore", 0);

                if (finalScore > highScore)
                {
                    highScore = finalScore;
                    PlayerPrefs.SetInt("HighScore", highScore);
                    PlayerPrefs.Save();
                }
                gameOverHighScoreText.text = "High Score: " + highScore.ToString();
                scoreManager.OnPlayerLose();
            }
            else
            {
                Debug.LogError("ScoreManager no encontrado.");
            }
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); 
    }

    public void Quit()
    {
        Application.Quit(); 
    }

    public void Home()
    {
        SceneManager.LoadScene("LoginScene"); 
    }
}
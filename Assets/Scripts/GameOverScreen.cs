using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverScreen : MonoBehaviour
{
    [Header("Внешние объекты")]
    [SerializeField] private ScoreSystem _scoreSystem;
    
    [Header("Внутренние объекты")]
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private TextMeshProUGUI _highscoreText;

    void Awake()
    {
        gameObject.SetActive(false);
    }
    
    public void Show()
    {
        GlobalScript.Instance.SetHighscore(_scoreSystem.score);

        _scoreText.text = "Счет: " + _scoreSystem.score;
        _highscoreText.text = "Рекорд: " + GlobalScript.Instance.Highscore;

        gameObject.SetActive(true);
    }
    
    public void OnRestartClicked()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnToMenuClicked()
    {
        SceneManager.LoadScene("Menu");
    }
}

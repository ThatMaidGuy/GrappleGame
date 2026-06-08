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
        _scoreText.text = "Счет: " + _scoreSystem.score;
        _highscoreText.text = "Рекорд: " + _scoreSystem.score;
        gameObject.SetActive(true);
    }
    
    public void OnRestartClicked()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}

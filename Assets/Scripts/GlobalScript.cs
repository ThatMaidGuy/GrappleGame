using UnityEngine;

public class GlobalScript : MonoBehaviour
{
    public static GlobalScript Instance { get; private set; }

    public int Highscore { get; private set; }

    private const string HighscoreKey = "SaveHighscore";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        // Проверяем, существует ли уже экземпляр менеджера
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Уничтожаем дубликат, если мы вернулись на сцену, где он создается
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        LoadHighscore();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetHighscore(int score)
    {
        if (Highscore < score)
        {
            Highscore = score;
            SaveHighscore();
        }
    }

    // Метод для принудительного сохранения
    private void SaveHighscore()
    {
        PlayerPrefs.SetInt(HighscoreKey, Highscore);
        PlayerPrefs.Save(); // Записывает данные на диск сразу (в Unity 6 это происходит и автоматически, но так надежнее)
    }

    // Метод для загрузки
    private void LoadHighscore()
    {
        // Если ключ существует, берем значение. Если нет — вернет 0 по умолчанию
        if (PlayerPrefs.HasKey(HighscoreKey))
        {
            Highscore = PlayerPrefs.GetInt(HighscoreKey);
        }
        else
        {
            Highscore = 0;
        }
    }

    // Полезный метод, если игроку захочется сбросить прогресс
    public void ResetHighscore()
    {
        Highscore = 0;
        PlayerPrefs.DeleteKey(HighscoreKey);
        PlayerPrefs.Save();
    }
}

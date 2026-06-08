using UnityEngine;

public class MenuMusic : MonoBehaviour
{
    public static MenuMusic Instance { get; private set; }

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
    }
}

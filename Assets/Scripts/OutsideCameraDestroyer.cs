using UnityEngine;

public class OutsideCameraDestroyer : MonoBehaviour
{
    private Camera _mainCamera;
    
    [Tooltip("Запасное расстояние выше верхней границы камеры, прежде чем объект удалится")]
    [SerializeField] private float _bufferDistance = 0.25f;

    private void Start()
    {
        // Находим главную камеру один раз при старте
        _mainCamera = Camera.main;

        if (_mainCamera == null)
        {
            Debug.LogError($"[{gameObject.name}] OutsideCameraDestroyer: Главная камера не найдена! Убедитесь, что у камеры стоит тег 'MainCamera'.");
            enabled = false; // Отключаем скрипт, чтобы не спамить ошибками в Update
        }
    }

    private void Update()
    {
        // Вычисляем верхнюю границу видимости камеры по оси Y
        // ортографический размер (orthographicSize) — это половина высоты экрана в юнитах
        float cameraTopY = _mainCamera.transform.position.y + _mainCamera.orthographicSize;

        // Если позиция объекта выше, чем верхняя граница камеры + запас
        if (transform.position.y > cameraTopY + _bufferDistance)
        {
            Destroy(gameObject);
        }
    }
}
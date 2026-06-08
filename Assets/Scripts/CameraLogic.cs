using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CameraLogic : MonoBehaviour
{
    [Header("Ссылки на объекты")]
    public Transform Target;
    public LevelGenerator lg;
    [Tooltip("Объект, в котором находятся пилы (контейнер/родитель пил)")]
    public Transform SawsContainer; // Сюда нужно перетащить объект с пилами в инспекторе

    [Header("Настройки смещения камеры")]
    public float IdleDownSpeed = 0.15f; // Скорость опускания камеры, когда игрок стоит
    [SerializeField] private float _lerpSpeed = 0.03f;    // Коэффициент для lerp (0.01 - медленно, 0.1 - быстро)
    
    [Header("Логика пил")]
    [SerializeField] private float _midPointOffset = 0f;  // Относительный Y, где центр экрана
    [SerializeField] private float _sawsYOffset = 1.0f;    // На сколько пилы выше верхней границы камеры

    private Camera _camera;
    private float _cameraHalfHeight;
    private float _lastCameraRawY;
    private float _triggerPosition = -2;
    private float _currentShift = 0f;
    private Rigidbody2D _targetRb;
    
    // Флаг для отслеживания момента активации пил (может быть полезен другим скриптам)
    public bool IsSawsActivated { get; private set; } = false;

    void Start()
    {
        _camera = GetComponent<Camera>();
        if (_camera != null)
        {
            // Рассчитываем половину высоты камеры в мировых координатах
            _cameraHalfHeight = _camera.orthographicSize;
        }

        if (Target != null)
        {
            _targetRb = Target.GetComponent<Rigidbody2D>();
        }
        
        _lastCameraRawY = transform.position.y;

        // В начале игры скрываем пилы или ставим их очень высоко
        /*
        if (SawsContainer != null)
        {
            // Можно либо деактивировать, либо просто поставить далеко вверх.
            // Поставим далеко вверх, чтобы они не мешали при появлении.
            SetSawsPositionAboveCamera();
        }
        */
    }

    void Update()
    {
        if (Target == null || SawsContainer == null || _camera == null) return;

        // 1. Проверка триггера генерации чанков (твоя исходная логика)
        if (transform.position.y <= _triggerPosition)
        {
            _triggerPosition -= 2;
            lg.GenerateRandomPattern();
        }

        // 2. Логика медленного опускания, если игрок стоит на месте
        // 2. Логика медленного опускания, если КАМЕРА не двигается вниз
        // Сравниваем текущую виртуальную позицию с позицией в прошлом кадре.
        // Используем небольшой порог (0.0001f), чтобы избежать погрешностей float.
        bool isCameraStatic = math.abs(transform.position.y - _lastCameraRawY) < 0.01f;

        // Сохраняем текущее значение для следующего кадра (делать это нужно ДО расчета нового _currentShift)
        _lastCameraRawY = transform.position.y;

        if (isCameraStatic)
        {
            // Если камера заблокирована (игрок не падает ниже), плавно смещаем виртуальную цель вниз
            _currentShift -= IdleDownSpeed * Time.deltaTime;
        }
        else
        {
            // Как только камера пошла вниз, плавно возвращаем смещение к нулю
            _currentShift = (float)math.lerp(_currentShift, 0f, Time.deltaTime * 5f);
        }

        // 3. Расчет целевой позиции камеры (твоя исходная логика с math.min)
        float desiredY = Target.position.y + _currentShift;
        float targetY = (float)math.lerp(transform.position.y, desiredY, Time.deltaTime * 60 * _lerpSpeed);
        float newY = (float)math.min(transform.position.y, targetY); // Камера идет только вниз

        transform.position = new Vector3(
            transform.position.x,
            newY,
            transform.position.z
        );

        // 4. Логика пил
        if (!IsSawsActivated)
        {
            // Проверка: дошел ли игрок до середины камеры
            if (Target.position.y < transform.position.y + _midPointOffset)
            {
                // Игрок опустился ниже середины. Активируем пилы!
                IsSawsActivated = true;
                Debug.Log("Игрок прошел середину, пилы выезжают!");
            }
        }

        if (IsSawsActivated)
        {
            // Если пилы активированы, они ВСЕГДА двигаются вместе с камерой, находясь над ней.
            SetSawsPositionAboveCamera();
        }
    }

    /// <summary>
    /// Устанавливает позицию контейнера пил точно над верхней границей камеры.
    /// </summary>
    private void SetSawsPositionAboveCamera()
    {
        // Рассчитываем Y-координату верхней границы камеры: Центр + Половина высоты + Смещение
        float sawTargetY = transform.position.y + _cameraHalfHeight + _sawsYOffset;

        // Обновляем позицию пил, сохраняя их X (или можно сделать его тоже центром камеры)
        SawsContainer.position = Vector3.Lerp(SawsContainer.position, new Vector3(
            transform.position.x, // Или можно оставить SawsContainer.position.x, если они смещены горизонтально
            sawTargetY,
            SawsContainer.position.z
        ), Time.deltaTime * 60 * 0.2f);
        
        /*
        SawsContainer.position = new Vector3(
            transform.position.x, // Или можно оставить SawsContainer.position.x, если они смещены горизонтально
            sawTargetY,
            SawsContainer.position.z
        );
        */
    }
}
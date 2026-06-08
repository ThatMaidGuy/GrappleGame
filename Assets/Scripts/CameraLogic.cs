using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CameraLogic : MonoBehaviour
{
    public static CameraLogic Instance { get; private set; }

    [Header("Ссылки на объекты")]
    public Transform Target;
    public LevelGenerator lg;
    [Tooltip("Объект, в котором находятся пилы (контейнер/родитель пил)")]
    public Transform SawsContainer; 

    [Header("Настройки смещения камеры")]
    public float IdleDownSpeed = 0.15f; 
    [SerializeField] private float _lerpSpeed = 0.03f;    
    
    [Header("Логика пил")]
    [SerializeField] private float _midPointOffset = 0f;  
    [SerializeField] private float _sawsYOffset = 1.0f;    

    [Header("Настройки тряски экрана")]
    [SerializeField] private float _shakeDuration = 0f;    // Текущее оставшееся время тряски
    [SerializeField] private float _shakeMagnitude = 0.1f; // Сила тряски
    [SerializeField] private float _shakeDamping = 2.0f;   // Скорость затухания тряски

    private Camera _camera;
    private float _cameraHalfHeight;
    private float _lastCameraRawY;
    private float _triggerPosition = -2;
    private float _currentShift = 0f;
    private Rigidbody2D _targetRb;
    
    // Переменная для хранения базовой позиции перед применением эффектов тряски
    private float _calculatedNewY;
    private float _baseStartX;
    private Vector3 _shakeOffset = Vector3.zero;

    public bool IsSawsActivated { get; private set; } = false;

    void Awake()
    {
        // Проверяем, существует ли уже экземпляр менеджера
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Уничтожаем дубликат, если мы вернулись на сцену, где он создается
            return;
        }

        Instance = this;
    }

    void Start()
    {
        _camera = GetComponent<Camera>();
        if (_camera != null)
        {
            _cameraHalfHeight = _camera.orthographicSize;
        }

        if (Target != null)
        {
            _targetRb = Target.GetComponent<Rigidbody2D>();
        }
        
        _lastCameraRawY = transform.position.y;
        _calculatedNewY = transform.position.y;
        _baseStartX = transform.position.x;
    }

    void Update()
    {
        if (Target == null || SawsContainer == null || _camera == null) return;

        // 1. Проверка триггера генерации чанков
        if (transform.position.y <= _triggerPosition)
        {
            _triggerPosition -= 2;
            lg.GenerateRandomPattern();
        }

        // 2. Логика медленного опускания
        bool isCameraStatic = math.abs(transform.position.y - _lastCameraRawY) < 0.01f;
        _lastCameraRawY = transform.position.y;

        if (isCameraStatic)
        {
            _currentShift -= IdleDownSpeed * Time.deltaTime;
        }
        else
        {
            _currentShift = (float)math.lerp(_currentShift, 0f, Time.deltaTime * 5f);
        }

        // 3. Расчет целевой позиции камеры (сохраняем в переменную, применим в LateUpdate)
        float desiredY = Target.position.y + _currentShift;
        float targetY = (float)math.lerp(transform.position.y, desiredY, Time.deltaTime * 60 * _lerpSpeed);
        _calculatedNewY = (float)math.min(transform.position.y, targetY); 

        // 4. Логика пил
        if (!IsSawsActivated)
        {
            if (Target.position.y < transform.position.y + _midPointOffset)
            {
                IsSawsActivated = true;
                Debug.Log("Игрок прошел середину, пилы выезжают!");
            }
        }

        if (IsSawsActivated)
        {
            SetSawsPositionAboveCamera();
        }

        // 5. Логика расчета тряски
        HandleShake();
    }

    // Все манипуляции с позицией камеры Unity рекомендует делать тут, чтобы избежать дрожания
    void LateUpdate()
    {
        if (Target == null || _camera == null) return;

        // Вместо накопления transform.position.x берем чистый _baseStartX
        transform.position = new Vector3(
            _baseStartX + _shakeOffset.x,
            _calculatedNewY + _shakeOffset.y,
            transform.position.z
        );
    }

    /// <summary>
    /// Метод для запуска тряски из других скриптов (например, при получении урона или взрыве)
    /// </summary>
    /// <param name="duration">Длительность в секундах</param>
    /// <param name="magnitude">Сила тряски (смещение)</param>
    public void TriggerShake(float duration = 0.3f, float magnitude = 0.15f)
    {
        _shakeDuration = duration;
        _shakeMagnitude = magnitude;
    }

    private void HandleShake()
    {
        if (_shakeDuration > 0)
        {
            // Генерируем случайное смещение внутри круга
            Vector2 randomPoint = UnityEngine.Random.insideUnitCircle * _shakeMagnitude;
            _shakeOffset = new Vector3(randomPoint.x, randomPoint.y, 0f);

            // Уменьшаем время тряски
            _shakeDuration -= Time.deltaTime;

            // Плавно уменьшаем силу тряски к концу (затухание)
            _shakeMagnitude = Mathf.MoveTowards(_shakeMagnitude, 0f, Time.deltaTime * _shakeDamping);
        }
        else
        {
            // Если офсет еще не равен нулю, плавно возвращаем его к Vector3.zero
            if (_shakeOffset != Vector3.zero)
            {
                _shakeOffset = Vector3.MoveTowards(_shakeOffset, Vector3.zero, Time.deltaTime * 5f);
                
                // Защита от бесконечного приближения float: если смещение стало мизерным, принудительно обнуляем
                if (_shakeOffset.sqrMagnitude < 0.0001f)
                {
                    _shakeOffset = Vector3.zero;
                }
            }
        }
    }

    private void SetSawsPositionAboveCamera()
    {
        // Считаем от _calculatedNewY, чтобы пилы не тряслись ВМЕСТЕ с камерой, 
        // иначе визуально они будут казаться статичными относительно экрана во время тряски.
        float sawTargetY = _calculatedNewY + _cameraHalfHeight + _sawsYOffset;

        SawsContainer.position = Vector3.Lerp(SawsContainer.position, new Vector3(
            transform.position.x, 
            sawTargetY,
            SawsContainer.position.z
        ), Time.deltaTime * 60 * 0.2f);
    }
}
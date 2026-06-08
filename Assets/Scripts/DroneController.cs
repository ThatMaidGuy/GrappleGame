using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class DroneController : MonoBehaviour
{
    public GameObject explosion;

    [Header("Frames")]
    public SpriteRenderer spriteRenderer;
    public Sprite idle;
    public Sprite hurt;

    [Header("Movement Settings")]
    [SerializeField] private float speed = 5f;
    
    [Header("Target Settings")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private LayerMask visionObstaclesMask; // Слои стен

    [Header("Health & Knockback")]
    [SerializeField] private int maxHealth = 2;
    [SerializeField] private float knockbackForce = 8f;
    [SerializeField] private float knockbackDuration = 0.25f; // Как долго дрон летит назад в секундах

    private int currentHealth;
    private bool isKnockbackActive = false;
    private float knockbackTimer = 0f;

    private Transform playerTransform;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Настраиваем Rigidbody для корректной физики
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.useFullKinematicContacts = true; 
        
        // Включаем непрерывную проверку столкновений, чтобы дрон не пролетал сквозь стены
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // Ищем игрока
        GameObject player = GameObject.FindWithTag(playerTag);
        if (player != null)
        {
            playerTransform = player.transform;
        }

        currentHealth = maxHealth;
    }

    void FixedUpdate()
    {
        // Если дрон сейчас получает люлей — обрабатываем таймер отбрасывания
        if (isKnockbackActive)
        {
            knockbackTimer -= Time.fixedDeltaTime;
            if (knockbackTimer <= 0)
            {
                spriteRenderer.sprite = idle;
                isKnockbackActive = false; // Возвращаем управление дрону
            }
            return; // Выходим, не давая сработать логике преследования
        }

        // Если игрока нет или он за стеной — обнуляем скорость (дрон висит/ждет)
        if (playerTransform == null || !HasLineOfSightToPlayer())
        {
            HoverInPlace();
            return;
        }

        // Вычисляем направление к игроку
        Vector2 direction = ((Vector2)playerTransform.position - rb.position).normalized;

        // Задаем скорость в направлении игрока
        rb.linearVelocity = direction * speed;

        // Поворот/наклон в сторону движения
        RotateTowardsTarget(direction);
    }

    private void HoverInPlace()
    {
        // Гасим скорость, чтобы дрон послушно стонал/висел на месте
        rb.linearVelocity = Vector2.zero;

        // Сбрасываем наклон в дефолтное состояние (0 градусов), пока ждем
        transform.rotation = Quaternion.Euler(Vector3.zero);
    }

    private bool HasLineOfSightToPlayer()
    {
        Vector2 startPos = rb.position;
        Vector2 targetPos = playerTransform.position;
        Vector2 direction = targetPos - startPos;
        float distance = direction.magnitude;

        // Игнорируем сам триггер дрона при касте луча
        RaycastHit2D hit = Physics2D.Raycast(startPos, direction.normalized, distance, visionObstaclesMask);

        // Если луч встретил стену — игрок скрылся
        if (hit.collider != null)
        {
            return false; 
        }

        return true;
    }

    private void RotateTowardsTarget(Vector2 direction)
    {
        if (direction.x > 0.01f)
        {
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, -17f));
        }
        else if (direction.x < -0.01f)
        {
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, 17f));
        }
    }

    private void OnDrawGizmos()
    {
        if (playerTransform != null)
        {
            Gizmos.color = HasLineOfSightToPlayer() ? Color.green : Color.red;
            Gizmos.DrawLine(transform.position, playerTransform.position);
        }
    }

    public void Hurt()
    {
        currentHealth--;

        if (currentHealth <= 0) Die();
        else ApplyKnockback();
    }

    private void ApplyKnockback()
    {
        if (playerTransform == null) return;

        // Направление ОТ игрока к дрону (обратная сторона)
        Vector2 knockbackDirection = ((Vector2)transform.position - (Vector2)playerTransform.position).normalized;
        
        // Если они стоят идеально друг в друге, толкаем просто вверх-назад
        if (knockbackDirection == Vector2.zero) 
            knockbackDirection = new Vector2(-1f, 1f).normalized;

        // Включаем режим отбрасывания
        isKnockbackActive = true;
        knockbackTimer = knockbackDuration;
        spriteRenderer.sprite = hurt;

        // Задаем физическую скорость отскока
        rb.linearVelocity = knockbackDirection * knockbackForce;
        
        // (Опционально) Закручиваем дрон для визуального эффекта удара
        transform.rotation = Quaternion.Euler(0, 0, knockbackDirection.x > 0 ? -35f : 35f);
    }

    private void Die()
    {
        Instantiate(explosion, transform.position, Quaternion.identity);
        
        Debug.Log($"[DroneController] {gameObject.name} уничтожен!");
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.name != "Player") return;
        var player = other.gameObject.GetComponent<PlayerScript>();
        player.Die();
        Die();
    }
}
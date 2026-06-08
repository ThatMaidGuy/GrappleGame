using System;
using UnityEngine;

public class StompBlockLogic : MonoBehaviour
{
    [Header("Анимация")]
    [SerializeField] private Sprite idleSprite;
    [SerializeField] private Sprite movingSprite;
    
    [Header("Настройки движения")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float maxDistance = 15f; // Дальность видимости лучей

    [Header("Слои (Layers)")]
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Звуки")]
    public AudioSource heavyStopSound;
    public AudioSource onSound;
    public AudioSource offSound;

    private Vector2 moveDirection = Vector2.zero;
    private bool isMoving = false;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private BoxCollider2D bc;

    private bool disable = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        bc = GetComponent<BoxCollider2D>();
        
        // Сразу при старте игры ровняем блок по сетке, на случай если в редакторе поставили криво
        SnapToGrid();
    }

    void Update()
    {
        if (disable) return;

        // Если блок уже движется, лучи пускать не нужно
        if (!isMoving) CheckForPlayer();
    }

    void FixedUpdate()
    {
        if (isMoving) MoveBlock();
    }

    private void CheckForPlayer()
    {
        // Направления для проверки: Вверх, Вниз, Влево, Вправо
        Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };

        foreach (Vector2 dir in directions)
        {
            // Находим начальную точку луча на границе коллайдера, 
            // чтобы луч не сталкивался со стеной, к которой блок уже прижат
            Vector2 rayStartPoint = CalculateRayStartPoint(dir);

            // Пускаем луч со смещением
            RaycastHit2D hit = Physics2D.Raycast(rayStartPoint, dir, maxDistance, playerLayer | obstacleLayer);
            
            if (!hit.collider) continue;
            // Если первым на пути луча оказался игрок
            if (((1 << hit.collider.gameObject.layer) & playerLayer) == 0) continue;

            onSound.Play();
            
            StartMoving(dir);
            break; // Прерываем цикл, направление найдено
        }
    }
    
    private void MoveBlock()
    {
        // Вычисляем, на какое расстояние блок хочет сдвинуться в этом кадре
        float moveStep = speed * Time.fixedDeltaTime;
        Vector2 displacement = moveDirection * moveStep;

        // Сканируем дорогу впереди. Наш BoxCast теперь тоже равен уменьшенному размеру коллайдера (0.9),
        // поэтому он свободно пролетает сквозь коридор 1х1, замечая только стены ТУПИКА спереди.
        RaycastHit2D hitWall = Physics2D.BoxCast(
            transform.position, 
            bc.size, 
            0f, 
            moveDirection, 
            moveStep, 
            obstacleLayer
        );

        if (hitWall.collider)
        {
            // Встаем ровно вплотную к стене тупика
            Vector2 finalPos = (Vector2)transform.position + moveDirection * hitWall.distance;
            rb.MovePosition(finalPos);
            StopMoving();
        }
        else
        {
            rb.MovePosition(rb.position + displacement);
        }
    }
    
    private Vector2 CalculateRayStartPoint(Vector2 dir)
    {
        // Вычисляем край на основе текущего размера BoxCollider2D
        // Добавляем микро-отступ (0.01f), чтобы выйти за рамки собственной геометрии,
        // но остаться внутри габаритов тайла (так как коллайдер равен 0.9, запас огромный)
        Vector2 extents = bc.size * 0.5f;
        Vector2 offset = dir * (extents + new Vector2(0.01f, 0.01f));
        return (Vector2)transform.position + offset;
    }

    private void StartMoving(Vector2 direction)
    {
        moveDirection = direction;
        isMoving = true;
        sr.sprite = movingSprite;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Если блок находится в движении и врезается в препятствие (стену)
        if (isMoving && ((1 << collision.gameObject.layer) & obstacleLayer) != 0)
        {
            StopMoving();
        }
    }

    private void StopMoving()
    {
        isMoving = false;
        // Вызываем выравнивание по сетке
        SnapToGrid();

        CameraLogic.Instance.TriggerShake(0.1f, 0.1f);
        
        heavyStopSound.Play();
        offSound.Play();

        disable = true;
        
        moveDirection = Vector2.zero;
        sr.sprite = idleSprite;
    }
    
    private void SnapToGrid()
    {
        float gridSize = 0.32f;
        float halfGrid = gridSize / 2f;

        // Вектор смещения: берем позицию центра блока и ОТИМАЕМ половину сетки.
        // Это переносит расчетную точку в левый нижний угол блока (как у тайлмапа).
        float rawX = transform.position.x - halfGrid;
        float rawY = transform.position.y - halfGrid;

        // Делаем микро-коррекцию в зависимости от того, КУДА блок только что летел.
        // Если летел влево (moveDirection.x < 0), мы искусственно сдвигаем расчетную точку чуть левее, 
        // чтобы Mathf.Round гарантированно выбрал левую ячейку, а не правую.
        rawX += moveDirection.x * 0.01f;
        rawY += moveDirection.y * 0.01f;

        // Округляем координаты левого нижнего угла тайла
        float snappedX = Mathf.Round(rawX / gridSize) * gridSize;
        float snappedY = Mathf.Round(rawY / gridSize) * gridSize;

        // Возвращаем позицию к ЦЕНТРУ тайла, прибавляя половину размера обратно
        Vector2 snappedPosition = new Vector2(snappedX + halfGrid, snappedY + halfGrid);

        // Применяем координаты
        transform.position = snappedPosition;
        rb.position = snappedPosition;
    }

    // Визуализация лучей в редакторе для удобства настройки
    private void OnDrawGizmosSelected()
    {
        if (bc == null) bc = GetComponent<BoxCollider2D>();
        if (bc == null) return;

        Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
        Gizmos.color = Color.green;

        foreach (Vector2 dir in directions)
        {
            Vector2 start = CalculateRayStartPoint(dir);
            Gizmos.DrawRay(start, dir * maxDistance);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.name != "Player") return;
        var player = other.gameObject.GetComponent<PlayerScript>();
        player.Die();
    }
}

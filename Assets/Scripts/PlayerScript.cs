using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerScript : MonoBehaviour
{
    [Header("Настройки крюка")]
    public float maxDistance = 15f;
    public float hookSpeed = 40f;
    public float pullSpeed = 20f;
    public LayerMask grappleLayer;
    public float rotationSpeed = 15f; 
    public float stopDistance = 0.2f;

    [Header("Настройки падения (Кинематика)")]
    public float fallSpeed = 10f; 

    [Header("Компоненты")]
    public LineRenderer lineRenderer;
    public Rigidbody2D rb;
    public Animator animator;
    public SpriteRenderer sRenderer;
    public Transform sTransform;
    public Collider2D playerCollider; // ОБЯЗАТЕЛЬНО: перетащи сюда коллайдер игрока!

    private Vector2 targetPoint;
    private bool isGrappling = false; 
    private bool isFlying = false;    
    private Vector2 currentHookPos;
    private Key activeKey = Key.None;

    // Слой, на котором находится сам игрок (чтобы луч не спотыкался об себя)
    private ContactFilter2D movementFilter;
    private RaycastHit2D[] hitBuffer = new RaycastHit2D[1];

    void Start()
    {
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            // Выключаем встроенные симуляции, теперь мы полностью управляем движением
            rb.useFullKinematicContacts = false; 
        }

        if (playerCollider == null)
        {
            playerCollider = GetComponent<Collider2D>();
        }

        // Настраиваем фильтр для коллизий: проверяем только grappleLayer (стены/пол)
        movementFilter.SetLayerMask(grappleLayer);
        movementFilter.useLayerMask = true;
    }

    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (!isGrappling && !isFlying)
        {
            if (keyboard.wKey.wasPressedThisFrame || keyboard.upArrowKey.wasPressedThisFrame) 
                StartHook(Vector2.up, keyboard.wKey.wasPressedThisFrame ? Key.W : Key.UpArrow);
    
            else if (keyboard.sKey.wasPressedThisFrame || keyboard.downArrowKey.wasPressedThisFrame) 
                StartHook(Vector2.down, keyboard.sKey.wasPressedThisFrame ? Key.S : Key.DownArrow);
    
            else if (keyboard.aKey.wasPressedThisFrame || keyboard.leftArrowKey.wasPressedThisFrame) 
                StartHook(Vector2.left, keyboard.aKey.wasPressedThisFrame ? Key.A : Key.LeftArrow);
    
            else if (keyboard.dKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame) 
                StartHook(Vector2.right, keyboard.dKey.wasPressedThisFrame ? Key.D : Key.RightArrow);
        }

        if (activeKey != Key.None)
        {
            if (!keyboard[activeKey].isPressed)
            {
                StopGrapple();
            }
        }

        DrawRope();
        PlayAnimations();
    }

    void StartHook(Vector2 direction, Key key)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, maxDistance, grappleLayer);

        if (hit.collider != null)
        {
            rb.linearVelocity = Vector2.zero;
            activeKey = key; 
            
            Vector2 alignedPoint = hit.point;
            if (direction == Vector2.left || direction == Vector2.right)
            {
                alignedPoint.y = transform.position.y;
                sRenderer.flipX = direction == Vector2.right;
            }
            else if (direction == Vector2.up || direction == Vector2.down) 
            {
                alignedPoint.x = transform.position.x;
            }

            targetPoint = alignedPoint;
            StartCoroutine(FlyHookRoutine());
        }
    }

    IEnumerator FlyHookRoutine()
    {
        isFlying = true;
        currentHookPos = transform.position;

        while (Vector2.Distance(currentHookPos, targetPoint) > 0.2f)
        {
            currentHookPos = Vector2.MoveTowards(currentHookPos, targetPoint, hookSpeed * Time.deltaTime);
            yield return null;
        }

        isFlying = false;
        isGrappling = true;
    }

    void FixedUpdate()
    {
        Vector2 velocityThisFrame = Vector2.zero;

        if (isGrappling)
        {
            float distanceToTarget = Vector2.Distance(transform.position, targetPoint);
            if (distanceToTarget > stopDistance)
            {
                Vector2 direction = (targetPoint - (Vector2)transform.position).normalized;
                float currentSpeed = Mathf.Min(pullSpeed, distanceToTarget / Time.fixedDeltaTime);
                velocityThisFrame = direction * currentSpeed;
            }
        }
        else if (!isFlying)
        {
            // Фиксированное падение
            velocityThisFrame = new Vector2(0, -fallSpeed);
        }

        rb.linearVelocity = velocityThisFrame;

        // Если есть какое-то движение — кастомно двигаем с проверкой препятствий
        if (velocityThisFrame != Vector2.zero)
        {
            MoveKinematic(velocityThisFrame * Time.fixedDeltaTime);
        }
    }

    // НАША СОБСТВЕННАЯ СИСТЕМА КОЛЛИЗИЙ
    void MoveKinematic(Vector2 movement)
    {
        float distance = movement.magnitude;
        Vector2 direction = movement.normalized;

        // "Проектируем" форму нашего коллайдера вперед по направлению движения.
        // Добавляем крошечный отступ (0.01f), чтобы не застревать намертво в текстурах.
        int count = playerCollider.Cast(direction, movementFilter, hitBuffer, distance + 0.01f);

        if (count > 0)
        {
            // Корректируем дистанцию, чтобы встать вплотную к стене/полу, но не заходить внутрь
            distance = Mathf.Max(0, hitBuffer[0].distance - 0.01f);
            
            // Если мы падали и упёрлись во что-то снизу — обнуляем скорость падения
            if (!isGrappling && direction.y < 0)
            {
                rb.linearVelocity = Vector2.zero;
            }
        }

        // Перемещаем персонажа на безопасное вычисленное расстояние
        rb.MovePosition(rb.position + direction * distance);
    }

    void DrawRope()
    {
        if (isGrappling || isFlying)
        {
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, isFlying ? currentHookPos : targetPoint);
        }
        else
        {
            lineRenderer.positionCount = 0;
        }
    }

    void StopGrapple()
    {
        isGrappling = false;
        isFlying = false;
        activeKey = Key.None;
        rb.linearVelocity = Vector2.zero;
        StopAllCoroutines();
    }

    void PlayAnimations()
    {
        float targetZ = 0;

        if (isFlying || isGrappling)
        {
            animator.Play("dash");
            Vector2 dir = (targetPoint - (Vector2)transform.position).normalized;

            if (Mathf.Abs(dir.y) > 0.5f) 
            {
                if (dir.y > 0) targetZ = sRenderer.flipX ? 90f : -90f;
                else targetZ = sRenderer.flipX ? -90f : 90f;
            }
        }
        else
        {
            if (rb.linearVelocity.y < -0.1f) animator.Play("fall");
            else animator.Play("idle");
        
            targetZ = 0; 
        }

        Quaternion targetRotation = Quaternion.Euler(0, 0, targetZ);
        sTransform.localRotation = Quaternion.Lerp(
            sTransform.localRotation, 
            targetRotation, 
            Time.deltaTime * rotationSpeed
        );
    }

    public void Die()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
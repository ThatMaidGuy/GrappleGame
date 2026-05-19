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
    public float rotationSpeed = 15f; // Скорость наклона
    public float stopDistance = 0.2f;

    [Header("Компоненты")]
    public LineRenderer lineRenderer;
    public Rigidbody2D rb;
    public Animator animator;
    public SpriteRenderer sRenderer;
    public Transform sTransform;

    private Vector2 targetPoint;
    private bool isGrappling = false; 
    private bool isFlying = false;    
    private Vector2 currentHookPos;
    
    // Переменная для отслеживания текущей зажатой клавиши
    private Key activeKey = Key.None;

    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        // 1. ПРОВЕРКА НАЖАТИЯ (Запуск крюка)
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

        // 2. ПРОВЕРКА ОТПУСКАНИЯ (Разрыв троса)
        if (activeKey != Key.None)
        {
            // Если текущая активная клавиша была отпущена — останавливаем всё
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
            rb.gravityScale = 0; // Гравитация исчезает, пока мы держимся
            rb.linearVelocity = Vector2.zero;
            
            activeKey = key; // Запоминаем, какую клавишу нужно держать
            
            // Выравнивание
            Vector2 alignedPoint = hit.point;
            if (direction == Vector2.left || direction == Vector2.right)
            {
                alignedPoint.y = transform.position.y;
                sRenderer.flipX = direction == Vector2.right;
            }
            else if (direction == Vector2.up || direction == Vector2.down) alignedPoint.x = transform.position.x;

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
        if (isGrappling)
        {
            Vector2 direction = (targetPoint - (Vector2)transform.position).normalized;
            rb.linearVelocity = direction * pullSpeed;

            // Если уткнулись в стену (близко к точке), просто замираем
            if (Vector2.Distance(transform.position, targetPoint) < stopDistance)
            {
                rb.linearVelocity = Vector2.zero;
            }
        }
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
        rb.gravityScale = 1; // Возвращаем физику
        StopAllCoroutines();
    }

    void PlayAnimations()
    {
        float targetZ = 0; // По умолчанию смотрим прямо

        if (isFlying || isGrappling)
        {
            animator.Play("dash");

            // Вычисляем направление к крюку
            Vector2 dir = (targetPoint - (Vector2)transform.position).normalized;

            // Проверяем, зацеплены ли мы вертикально (по Y)
            // Используем порог 0.5f, чтобы небольшие отклонения не ломали логику
            if (Mathf.Abs(dir.y) > 0.5f) 
            {
                if (dir.y > 0) // Крюк сверху
                {
                    targetZ = sRenderer.flipX ? 90f : -90f;
                }
                else // Крюк снизу
                {
                    targetZ = sRenderer.flipX ? -90f : 90f;
                }
            }
        }
        else
        {
            // Обычные анимации
            if (rb.linearVelocity.y < -0.1f) animator.Play("fall");
            else animator.Play("idle");
        
            targetZ = 0; // Сбрасываем наклон
        }

        // Плавный поворот дочернего объекта
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

using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MovingBlock : MonoBehaviour
{
    public enum MovementDirection { Horizontal, Vertical }

    [Header("Настройки движения")]
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float speed = 3f;

    private Rigidbody2D rb;
    private Vector2 _currentVelocity;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void ChangeDirection(MovementDirection direction)
    {
        _currentVelocity = direction switch
        {
            MovementDirection.Horizontal => Vector2.right * speed,
            MovementDirection.Vertical => Vector2.up * speed,
            _ => _currentVelocity
        };
    }

    void FixedUpdate()
    {
        rb.linearVelocity = _currentVelocity;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & obstacleLayer) == 0) return;
        _currentVelocity = -_currentVelocity;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.name != "Player") return;
        var player = other.gameObject.GetComponent<PlayerScript>();
        player.Die();
    }
}

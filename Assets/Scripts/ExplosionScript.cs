using UnityEngine;

public class ExplosionScript : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Animator animator;

    public AudioSource sound;

    private float _timer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator.Play("explosion");
    }

    // Update is called once per frame
    void Update()
    {
        _timer += Time.deltaTime;

        if (_timer > 3f) Destroy(gameObject);

        if (_timer > 0.3f)
        {
            animator.enabled = false;
            spriteRenderer.enabled = false;
        }
    }
}

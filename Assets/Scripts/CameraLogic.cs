using Unity.Mathematics;
using UnityEngine;

public class CameraLogic : MonoBehaviour
{
    public Transform Target;
    public LevelGenerator lg;

    private float _triggerPosition = -2;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.y <= _triggerPosition)
        {
            _triggerPosition -= 2;
            lg.GenerateRandomPattern();
        }
        
        float targetY = (float)math.lerp(transform.position.y, Target.position.y, Time.deltaTime * 60 * 0.03);
        
        transform.position = new Vector3(
            transform.position.x,
            math.min(transform.position.y, targetY), // Выбирает только то, что ниже текущей позиции
            transform.position.z
        );
    }
}

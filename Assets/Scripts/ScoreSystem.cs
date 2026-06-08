using TMPro;
using UnityEngine;

public class ScoreSystem : MonoBehaviour
{
    public int score;
    
    [Header("Настройки")]
    [SerializeField] private GameObject _player;
    [SerializeField] private TextMeshProUGUI _scoreText;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var new_score = Mathf.Abs(Mathf.FloorToInt(_player.transform.position.y));
        
        if (new_score <= score) return;
        
        score = Mathf.Abs(Mathf.FloorToInt(_player.transform.position.y));
        _scoreText.text = score.ToString();
    }
}

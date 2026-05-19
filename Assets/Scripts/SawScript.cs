using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SawScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.name != "Player") return;
        var player = other.gameObject.GetComponent<PlayerScript>();
        player.Die();
    }
}

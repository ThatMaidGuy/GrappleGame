using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour
{
    [SerializeField] private AudioMixer _audioMixer;

    [SerializeField] private TextMeshProUGUI _volumeButtonText;
    [SerializeField] private TextMeshProUGUI _highscoreText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _highscoreText.text = "Рекорд: " + GlobalScript.Instance.Highscore;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayButtonClicked()
    {
        Destroy(MenuMusic.Instance.gameObject);
        SceneManager.LoadScene("Level", LoadSceneMode.Single);
    }

    public void VolumeButtonClicked()
    {
        _audioMixer.GetFloat("MasterVolume", out float vol);
        if (vol == -80f)
        {
            _volumeButtonText.text = "Выключить звук";
            _audioMixer.SetFloat("MasterVolume", 0f);
        } else
        {
            _volumeButtonText.text = "Включить звук";
            _audioMixer.SetFloat("MasterVolume", -80f);
        }
        
    }

    public void OnHowToClicked()
    {
        SceneManager.LoadScene("HowTo", LoadSceneMode.Single);
    }

    public void OnStoryClicked()
    {
        SceneManager.LoadScene("Story", LoadSceneMode.Single);
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;

public class HowToScript : MonoBehaviour
{
    public void OnToMenuClicked()
    {
        SceneManager.LoadScene("Menu", LoadSceneMode.Single);
    }
}

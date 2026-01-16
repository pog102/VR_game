using UnityEngine;
using UnityEngine.SceneManagement;

public class Autoloader : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
#if UNITY_SERVER
        // chnage scene to
        SceneManager.LoadScene("Game");
#endif
    }
}

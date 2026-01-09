using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
public class KeyboardManager : MonoBehaviour
{
    public TMP_InputField targetInput;
    public Client client;
    private AudioSource audioSource;
    public AudioClip  clickSound;

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }
    public void AddChar(string c)
    {
        if (!targetInput) return;

        targetInput.text += c;
        targetInput.caretPosition = targetInput.text.Length;
         audioSource.PlayOneShot(clickSound);
    }

    public void Backspace()
    {
        if (!targetInput) return;
        if (targetInput.text.Length == 0) return;

        targetInput.text =
            targetInput.text.Substring(0, targetInput.text.Length - 1);

        targetInput.caretPosition = targetInput.text.Length;
                 audioSource.PlayOneShot(clickSound);
    }


    public void Submit()
    {
        
        client.Send("name", targetInput.text);
        SceneManager.LoadScene("Game");
        // client.Send("name", "BOB");
    }
}

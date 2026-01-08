using UnityEngine;

using TMPro;
public class ServerLabel : MonoBehaviour
{
    public TextMeshProUGUI labelText; // Assign in Inspector

    void Start()
    {
        // Optional: clear at start
        if (labelText != null)
            labelText.text = "Waiting for the host";
    }

    // Function to update the label
    public void UpdateLabel(string msg)
    {
        if (labelText != null)
            labelText.text = msg;
    }
}

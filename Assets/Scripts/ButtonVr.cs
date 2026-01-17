using UnityEngine;
using UnityEngine.Events;

public class ButtonVr : MonoBehaviour
{
    [SerializeField]
    public bool EnableLight = true;
    public GameObject button;
    public UnityEvent onPress;
    public UnityEvent onRelease;
    private static bool changeColor = true; // 👈 shared by all buttons
    GameObject presser;
    private bool isPressed;
    private Material mat;

    void Start()
    {
        isPressed = false;
        mat = button.GetComponent<Renderer>().material;
    }

    public void ResetLigt()
    {
        mat.DisableKeyword("_EMISSION");
        changeColor = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isPressed)
        {
            button.transform.localPosition = new Vector3(0, 0.009f, 0);
            presser = other.gameObject;
            onPress.Invoke();
            isPressed = true;
        }
        if (changeColor && EnableLight)
        {
            mat.EnableKeyword("_EMISSION");
            // mat.SetColor("_EmissionColor", Color.white);
            changeColor = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == presser)
        {
            button.transform.localPosition = new Vector3(0, 0.015f, 0);
            onRelease.Invoke();
            isPressed = false;
        }
    }

    public void SpawnSphere()
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.localPosition = new Vector3(0.5f, 0.5f, 0.5f);
        sphere.transform.localPosition = new Vector3(0, 1, 2);
        sphere.AddComponent<Rigidbody>();
    }
}

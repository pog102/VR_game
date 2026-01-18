using UnityEngine;

public class NameTagAtPlayer : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private Transform cam;

    void Start()
    {
        cam = Camera.main.transform;
    }

    // Update is called once per frame
    private void LateUpdate()
    {
        transform.LookAt(cam);
        transform.Rotate(0, 180, 0);
    }
}

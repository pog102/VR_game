using UnityEngine;

public class VrRigReference : MonoBehaviour
{
    public static VrRigReference Singleton;

    public Transform head;
    public Transform leftHand;
    public Transform rightHand;
    public Transform root;

    private void Awake()
    {
        Singleton = this;
    }
}

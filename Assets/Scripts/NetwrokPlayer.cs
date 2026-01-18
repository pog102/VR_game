using Unity.Netcode;
using UnityEngine;

public class NetwrokPlayer : NetworkBehaviour
{
    public GameObject nameTag;

    // public Transform nameTag;
    public Transform head;
    public Transform leftHand;
    public Transform rightHand;
    public Transform root;

    public Renderer[] meshToDisable;

    // private Transform localCameraTransform;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        // localCameraTransform = Camera.main.transform;
        if (IsOwner)
        {
            // nameTag.enabled = false;
            foreach (var item in meshToDisable)
            {
                item.enabled = false;
            }
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Update()
    {
        if (IsOwner)
        {
            root.position = VrRigReference.Singleton.root.position;
            root.rotation = VrRigReference.Singleton.root.rotation;

            head.position = VrRigReference.Singleton.head.position;
            head.rotation = VrRigReference.Singleton.head.rotation;

            // nameTag.position = VrRigReference.Singleton.head.position;
            // nameTag.rotation = VrRigReference.Singleton.head.rotation;
            // nameTag.transform.position = VrRigReference.Singleton.root.position;
            nameTag.transform.position = head.position + Vector3.up * 0.25f;
            // nameTag.transform.LookAt(cam);
            // nameTag.transform.rotation = VrRigReference.Singleton.root.rotation;
            // nameTag.transform.Rotate(trans);
            leftHand.position = VrRigReference.Singleton.leftHand.position;
            leftHand.rotation = VrRigReference.Singleton.leftHand.rotation;

            rightHand.position = VrRigReference.Singleton.rightHand.position;
            rightHand.rotation = VrRigReference.Singleton.rightHand.rotation;
        }
        // if (Camera.main != null)
        // {
        //     // Make the nametag look at the local player's camera
        //     nameTag.transform.LookAt(localCameraTransform);
        //
        //     // Optional: Flip it 180 degrees if the text appears backward
        //     // nameTag.transform.Rotate(0, 180, 0);
        // }
    }
}

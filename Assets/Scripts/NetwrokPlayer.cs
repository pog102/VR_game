using Unity.Netcode;
using UnityEngine;

public class NetwrokPlayer : NetworkBehaviour
{
    public Transform head;
    public Transform leftHand;
    public Transform rightHand;
    public Transform root;

    public Renderer[] meshToDisable;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsOwner)
        {
            foreach (var item in meshToDisable)
            {
                item.enabled = false;
            }
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    // Update is called once per frame
    void Update()
    {
        if (IsOwner)
        {
            root.position = VrRigReference.Singleton.root.position;
            root.rotation = VrRigReference.Singleton.root.rotation;

            head.position = VrRigReference.Singleton.head.position;
            head.rotation = VrRigReference.Singleton.head.rotation;

            leftHand.position = VrRigReference.Singleton.leftHand.position;
            leftHand.rotation = VrRigReference.Singleton.leftHand.rotation;

            rightHand.position = VrRigReference.Singleton.rightHand.position;
            rightHand.rotation = VrRigReference.Singleton.rightHand.rotation;
        }
    }
}

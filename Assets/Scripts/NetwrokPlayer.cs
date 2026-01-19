using TMPro;
using Unity.Collections; // Required for FixedString
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
    public NetworkVariable<FixedString32Bytes> playerName = new NetworkVariable<FixedString32Bytes>(
        "",
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        TextMeshPro nameTagText = nameTag.GetComponent<TextMeshPro>();
        playerName.OnValueChanged += (oldValue, newValue) =>
        {
            nameTagText.text = newValue.ToString();
        };
        nameTagText.text = playerName.Value.ToString();
        if (IsOwner)
        {
            foreach (var item in meshToDisable)
            {
                item.enabled = false;
            }
        }
    }

    void Update()
    {
        if (IsOwner)
        {
            root.position = VrRigReference.Singleton.root.position;
            root.rotation = VrRigReference.Singleton.root.rotation;

            head.position = VrRigReference.Singleton.head.position;
            head.rotation = VrRigReference.Singleton.head.rotation;

            nameTag.transform.position = head.position + Vector3.up * 0.35f;
            leftHand.position = VrRigReference.Singleton.leftHand.position;
            leftHand.rotation = VrRigReference.Singleton.leftHand.rotation;

            rightHand.position = VrRigReference.Singleton.rightHand.position;
            rightHand.rotation = VrRigReference.Singleton.rightHand.rotation;
        }
    }
}

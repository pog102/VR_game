using UnityEngine;

public struct PoseData
{
    public Vector3 rootPos;
    public Quaternion rootRot;
    public Vector3 headPos;
    public Quaternion headRot;
    public Vector3 leftPos;
    public Quaternion leftRot;
    public Vector3 rightPos;
    public Quaternion rightRot;
}

public class NetwrokPlayer : MonoBehaviour
{
    public Transform head;
    public Transform leftHand;
    public Transform rightHand;
    public Transform root;

    public Renderer[] meshToDisable;
    public float RemoteSmoothing = 12f;

    private bool _isLocal = true;
    private PoseData _targetPose;
    private bool _hasPose = false;

    public bool IsLocal
    {
        get { return _isLocal; }
    }

    private void Start()
    {
        ApplyLocalVisuals();
    }

    public void SetLocal(bool isLocal)
    {
        _isLocal = isLocal;
        ApplyLocalVisuals();
    }

    void Update()
    {
        if (_isLocal)
        {
            if (VrRigReference.Singleton == null)
            {
                return;
            }
            root.position = VrRigReference.Singleton.root.position;
            root.rotation = VrRigReference.Singleton.root.rotation;

            head.position = VrRigReference.Singleton.head.position;
            head.rotation = VrRigReference.Singleton.head.rotation;

            leftHand.position = VrRigReference.Singleton.leftHand.position;
            leftHand.rotation = VrRigReference.Singleton.leftHand.rotation;

            rightHand.position = VrRigReference.Singleton.rightHand.position;
            rightHand.rotation = VrRigReference.Singleton.rightHand.rotation;
        }
        else if (_hasPose)
        {
            float t = 1f - Mathf.Exp(-RemoteSmoothing * Time.deltaTime);
            root.position = Vector3.Lerp(root.position, _targetPose.rootPos, t);
            root.rotation = Quaternion.Slerp(root.rotation, _targetPose.rootRot, t);

            head.position = Vector3.Lerp(head.position, _targetPose.headPos, t);
            head.rotation = Quaternion.Slerp(head.rotation, _targetPose.headRot, t);

            leftHand.position = Vector3.Lerp(leftHand.position, _targetPose.leftPos, t);
            leftHand.rotation = Quaternion.Slerp(leftHand.rotation, _targetPose.leftRot, t);

            rightHand.position = Vector3.Lerp(rightHand.position, _targetPose.rightPos, t);
            rightHand.rotation = Quaternion.Slerp(rightHand.rotation, _targetPose.rightRot, t);
        }
    }

    public void ApplyRemotePose(PoseData pose)
    {
        _targetPose = pose;
        _hasPose = true;
    }

    private void ApplyLocalVisuals()
    {
        if (meshToDisable == null)
        {
            return;
        }
        foreach (Renderer item in meshToDisable)
        {
            if (item != null)
            {
                item.enabled = !_isLocal;
            }
        }
    }
}

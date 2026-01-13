using UnityEngine;
using UnityEngine.InputSystem;

// using Unity.Netcode;

public class AnimateHandOnInput : MonoBehaviour
{
    public InputActionProperty triggerValue;
    public InputActionProperty gripValue;
    public Animator handAnimator;

    private NetwrokPlayer _player;

    private void Awake()
    {
        _player = GetComponentInParent<NetwrokPlayer>();
    }

    void Update()
    {
        if (_player != null && !_player.IsLocal)
        {
            return;
        }
        float trigger = triggerValue.action.ReadValue<float>();
        float grip = gripValue.action.ReadValue<float>();
        handAnimator.SetFloat("Trigger", trigger);
        handAnimator.SetFloat("Grip", grip);
    }
}

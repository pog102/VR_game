using UnityEngine;
using UnityEngine.InputSystem;

// using Unity.Netcode;

public class AnimateHandOnInput : MonoBehaviour
{
    public InputActionProperty triggerValue;
    public InputActionProperty gripValue;
    public Animator handAnimator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() { }

    // Update is called once per frame
    void Update()
    {
        // if (!IsOwner) return;
        float trigger = triggerValue.action.ReadValue<float>();
        float grip = gripValue.action.ReadValue<float>();
        handAnimator.SetFloat("Trigger", trigger);
        handAnimator.SetFloat("Grip", grip);
    }
}

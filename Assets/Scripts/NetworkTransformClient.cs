using UnityEngine;
using Unity.Netcode.Components;
public class NetworkTransformClient : NetworkTransform
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}

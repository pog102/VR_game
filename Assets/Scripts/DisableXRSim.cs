using UnityEngine;

public class DisableXRSim : MonoBehaviour
{
      void Awake()
    {
#if !UNITY_EDITOR
        gameObject.SetActive(false);
#endif
    }
}

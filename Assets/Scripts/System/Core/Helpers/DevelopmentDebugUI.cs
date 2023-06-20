using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DevelopmentDebugUI : MonoBehaviour
{
    void Awake()
    {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        gameObject.SetActive(true);
#else
        gameObject.SetActive(false);
#endif
    }
}

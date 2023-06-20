using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    private static CameraSwitcher instance;
    public static CameraSwitcher Instance
    {
        get
        {
            if (!instance)
            {
                instance = FindObjectOfType<CameraSwitcher>();
            }
            return instance;
        }
    }

    [System.Serializable]
    private struct CameraSetup
    {
        public Vector3 pos;
        public Vector3 rot;
        public float fieldOfView;
    }

    [SerializeField] private Camera targetCamera;
    [SerializeField] private CameraSetup initialSetup;
    [SerializeField] private CameraSetup alternativeSetup;
    [SerializeField] private float transitionSpeed = 0.25f;
    private CameraSetup currentSetup;
    private bool isInitialSetup = true;
    public bool IsInitialSetup { get { return isInitialSetup; } }

    private void Awake()
    {
        SetCameraSetup(initialSetup);
    }

    private void FixedUpdate()
    {
        targetCamera.transform.position = Vector3.Lerp(targetCamera.transform.position, currentSetup.pos, transitionSpeed);
        targetCamera.transform.rotation = Quaternion.Lerp(targetCamera.transform.rotation, Quaternion.Euler(currentSetup.rot), transitionSpeed);
        targetCamera.fieldOfView = Mathf.Lerp(targetCamera.fieldOfView, currentSetup.fieldOfView, transitionSpeed);
    }

    public void ToggleCameraSetup()
    {
        isInitialSetup = !isInitialSetup;
        SetCameraSetup(isInitialSetup ? initialSetup : alternativeSetup);
    }

    private void SetCameraSetup(CameraSetup setup)
    {
        currentSetup = setup;
    }
}

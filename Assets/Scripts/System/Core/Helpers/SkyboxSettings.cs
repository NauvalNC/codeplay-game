using UnityEngine;

public class SkyboxSettings : MonoBehaviour
{
    public bool isRotateSky;
    public float rotationSpeed = 1f;
    public Material skybox;

    private void Awake()
    {
        RenderSettings.skybox = skybox;
    }

    protected void Update()
    {
        if (isRotateSky) RenderSettings.skybox.SetFloat("_Rotation", Time.time * rotationSpeed);
    }
}
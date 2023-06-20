using UnityEngine;

public class SwitchingPlatform : MonoBehaviour
{
    [SerializeField] private GameObject[] platformGroups;
    [SerializeField] private float switchDelay = 1f;
    private int groupIndex;
    private float delay;

    private void Awake()
    {
        ResetPlatforms();
    }

    private void Update()
    {
        delay -= Time.deltaTime;
        if (delay <= 0)
        {
            delay = switchDelay;
            NextPlatform();
        }
    }

    private void ResetPlatforms()
    {
        groupIndex = 0;
        delay = switchDelay;
        SwitchPlatform(groupIndex);
    }

    private void NextPlatform()
    {
        SwitchPlatform(++groupIndex);
        if (groupIndex >= platformGroups.Length - 1) groupIndex = -1;
    }

    private void SwitchPlatform(int index)
    {
        int tIterator = 0;
        foreach(GameObject tGroup in platformGroups)
        {
            tGroup.SetActive(tIterator == index);
            tIterator++;
        }
    }
}
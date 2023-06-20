using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private bool isLoop = false;
    [SerializeField] private float transitionSpeed = 0.1f;
    [SerializeField] private float delay = 2f;
    [SerializeField] private Transform platform;
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;
    private Vector3 currentDestination;
    private bool isForward = true;

    private void Start()
    {
        currentDestination = platform.transform.position;
        StartCoroutine(MovePlatform());
    }

    private void Update()
    {
        platform.position = Vector3.Lerp(platform.position, currentDestination, transitionSpeed);
    }

    private IEnumerator MovePlatform()
    {
        Vector3 tDestination = GetDestination();
        int tDist = Mathf.FloorToInt((tDestination - platform.position).magnitude);

        while (tDist >= 0.1f)
        {
            currentDestination = platform.position + (tDestination - platform.position).normalized;
            tDist = Mathf.FloorToInt((tDestination - platform.position).magnitude);
            yield return new WaitForSeconds(delay);
        }

        OnEndMovePlatform();
    }

    private void OnEndMovePlatform()
    {
        if (isLoop)
        {
            isForward = !isForward;
            StartCoroutine(MovePlatform());
        }
    }

    private Vector3 GetDestination()
    {
        return isForward ? endPoint.position : startPoint.position;
    }

    private void OnDrawGizmos()
    {
        if (platform && startPoint && endPoint)
        {
            Gizmos.DrawLine(platform.position, startPoint.position);
            Gizmos.DrawLine(platform.position, endPoint.position);
        }
    }
}

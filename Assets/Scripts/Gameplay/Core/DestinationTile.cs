using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestinationTile : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        PlayerController tPlayer = other.GetComponent<PlayerController>();
        if (tPlayer)
        {
            Debug.Log("Destination reached.");
            GameManager.Instance.isGameOver = true;
        }
    }
}

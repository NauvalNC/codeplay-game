using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        PlayerController tPlayer = collision.collider.GetComponent<PlayerController>();
        if (tPlayer)
        {
            Debug.Log("Hit obstacle.");
            GameManager.Instance.ResetLevel();
        }
    }
}

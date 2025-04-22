using System;
using Enemy;
using UnityEngine;

public class KillEnemy : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        other.gameObject.GetComponent<EnemyAgent>().TransitionToDeath();
        Debug.Log("collided");
    }
}

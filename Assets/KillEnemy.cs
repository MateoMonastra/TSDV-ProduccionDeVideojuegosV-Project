using System;
using Enemy;
using UnityEngine;

public class KillEnemy : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent(out EnemyAgent enemy))
        {
            enemy.TransitionToDeath();
            Debug.Log("collided");
        }
    }
}
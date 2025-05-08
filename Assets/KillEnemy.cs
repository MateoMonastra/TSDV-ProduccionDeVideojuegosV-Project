using System;
using UnityEngine;
using Enemies.Enemy;

public class KillEnemy : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent(out EnemyAgent enemy))
        {
            enemy.TransitionToDeath();
        }
    }
}
﻿using System;
using KinematicCharacterController.Examples;
using UnityEngine;

namespace PickUps
{
    public class DashRefresher : Pickup
    {
        [SerializeField] private ParticleSystem pickUpParticles;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            if (!other.TryGetComponent(out ExampleCharacterController player)) return;

            player.AddExtraDashCharge();

            if (pickUpParticles.isPlaying)
                pickUpParticles.Stop();

            pickUpParticles.Play();
            RefreshCooldown();
        }
    }
}
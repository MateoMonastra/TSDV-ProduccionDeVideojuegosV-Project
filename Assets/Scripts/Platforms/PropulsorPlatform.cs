using System;
using System.Collections.Generic;
using KinematicCharacterController;
using KinematicCharacterController.Examples;
using UnityEngine;

namespace Platforms
{
    public class PropulsorPlatform : MonoBehaviour
    {
        public float jumpForce;
        private Dictionary<Collider, bool> collisions = new Dictionary<Collider, bool>();

        private void OnTriggerEnter(Collider other)
        {
            if (!collisions.ContainsKey(other))
                collisions.Add(other, false);
        }

        private void OnTriggerStay(Collider other)
        {
            if (collisions[other])
                return;


            if (other.gameObject.CompareTag("Player"))
            {
                var kinematicCharacterMotor = other.gameObject.GetComponent<KinematicCharacterMotor>();
                var characterController = other.gameObject.GetComponent<ExampleCharacterController>();

                var hammerController = other.gameObject.GetComponentInChildren<HammerController>();

                if (kinematicCharacterMotor != null && characterController != null)
                {
                    if (characterController.CurrentCharacterState == CharacterState.Default)
                    {
                        collisions[other] = true;
                        kinematicCharacterMotor.ForceUnground();
                        kinematicCharacterMotor.BaseVelocity = transform.up * jumpForce;
                    }
                }

                if (hammerController != null)
                {
                    hammerController.InterruptGroundSlam();
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (collisions.ContainsKey(other))
                collisions.Remove(other);
        }
    }
}
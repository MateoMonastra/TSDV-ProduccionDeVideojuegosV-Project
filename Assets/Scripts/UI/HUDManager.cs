﻿using System;
using System.Collections;
using KinematicCharacterController.Examples;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class HUDManager : MonoBehaviour
    {
        [SerializeField] private ExampleCharacterController characterController;
        [SerializeField] private Image dashImage;
        [SerializeField] private Image jumpImage;
        [SerializeField] private GameObject blindnessEffect;
        [SerializeField] private float blindnessDuration = 2f;

        private Coroutine _blindnessCoroutine;

        private void OnEnable()
        {
            GameEvents.GameEvents.OnPlayerBlinded += OnBlind;
        }

        private void OnDisable()
        {
            GameEvents.GameEvents.OnPlayerBlinded -= OnBlind;
        }

        private void Update()
        {
            if (characterController.CanDash())
                dashImage.enabled = true;
            else
                dashImage.enabled = false;

            if (characterController.HasExtraJumps())
                jumpImage.enabled = true;
            else
                jumpImage.enabled = false;
        }

        private void OnBlind()
        {
            _blindnessCoroutine = StartCoroutine(BlindPlayer());
        }

        private IEnumerator BlindPlayer()
        {
            if (blindnessEffect != null)
            {
                blindnessEffect.SetActive(true);
                yield return new WaitForSeconds(blindnessDuration);
                blindnessEffect.SetActive(false);
            }
            else
            {
                Debug.LogWarning("BlindnessEffect no asignado");
            }
        }
    }
}
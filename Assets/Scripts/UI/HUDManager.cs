using System;
using System.Collections;
using KinematicCharacterController.Examples;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class HUDManager : MonoBehaviour
    {
        private static readonly int PlayerHealth = Animator.StringToHash("PlayerHealth");
        
        [SerializeField] private ExampleCharacterController characterController;
        [SerializeField] private Image dashImage;
        [SerializeField] private Image jumpImage;

        [Header("Blindness")] 
        [SerializeField] private GameObject blindnessEffect;
        [SerializeField] private float blindnessDuration = 2f;

        [Header("Damaged")] 
        [SerializeField] private Image damagedImage;
        [SerializeField] private float damagedDuration = 0.3f;
        [SerializeField] private Animator playerHealthAnimator;

        private Coroutine _blindnessCoroutine;
        private Coroutine _damagedCoroutine;

        private void OnEnable()
        {
            GameEvents.GameEvents.OnPlayerBlinded += OnBlind;
            GameEvents.GameEvents.OnPlayerDamaged += OnDamaged;
            characterController.healthController.OnHeal += SwapLifeCards;
        }

        private void OnDisable()
        {
            GameEvents.GameEvents.OnPlayerBlinded -= OnBlind;
            GameEvents.GameEvents.OnPlayerDamaged -= OnDamaged;
            characterController.healthController.OnHeal -= SwapLifeCards;
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

        public void SwapLifeCards()
        {
            if (playerHealthAnimator)
                playerHealthAnimator.SetInteger(PlayerHealth,characterController.healthController.GetCurrentHealth());
        }

        private void OnDamaged()
        {
            _damagedCoroutine = StartCoroutine(DamagedPlayer());
            SwapLifeCards();
        }

        private IEnumerator DamagedPlayer()
        {
            if (damagedImage != null)
            {
                damagedImage.gameObject.SetActive(true);
                
                Color color = damagedImage.color;
                color.a = 0f;
                damagedImage.color = color;

                float timer = 0f;
                float halfDuration = damagedDuration / 2f;
                
                while (timer < halfDuration)
                {
                    timer += Time.deltaTime;
                    color.a = Mathf.Lerp(0f, 1f, timer / halfDuration);
                    damagedImage.color = color;
                    yield return null;
                }
                
                timer = 0f;
                while (timer < halfDuration)
                {
                    timer += Time.deltaTime;
                    color.a = Mathf.Lerp(1f, 0f, timer / halfDuration);
                    damagedImage.color = color;
                    yield return null;
                }

                damagedImage.gameObject.SetActive(false);
            }
            else
            {
                Debug.LogWarning("damagedImage no asignado");
            }
        }

    }
}
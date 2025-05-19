using System;
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

        private void Update()
        {
            if(characterController.CanDash())
                dashImage.enabled = true;
            else
                dashImage.enabled = false;
            
            if(characterController.HasExtraJumps())
                jumpImage.enabled = true;
            else
                jumpImage.enabled = false;
            
        }
    }
}
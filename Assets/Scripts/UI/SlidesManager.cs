using System;
using KinematicCharacterController.Examples;
using UnityEngine;
using UnityEngine.UI;

public class SlidesManager : MonoBehaviour
{
    [SerializeField] private Slider mouseSens;
    [SerializeField] private Slider joystickSens;

    [SerializeField] private ExampleCharacterCamera camera; 
    private void Start()
    {
        mouseSens.value = camera.MouseRotationSpeed;
        joystickSens.value = camera.JoystickRotationSpeed;
    }
    
    public void ChangeMouseSensibility()
    {
        camera.MouseRotationSpeed = mouseSens.value;
    }

    public void ChangeJoystickSensibility()
    {
        camera.JoystickRotationSpeed = joystickSens.value;
    }
}

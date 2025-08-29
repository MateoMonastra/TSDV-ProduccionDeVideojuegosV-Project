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
        if (mouseSens != null)
            mouseSens.value = camera.MouseRotationSpeed;

        if (joystickSens != null)
            joystickSens.value = camera.JoystickRotationSpeed;
    }

    public void ChangeMouseSensibility()
    {
        if (camera != null)
            camera.MouseRotationSpeed = mouseSens.value;
    }

    public void ChangeJoystickSensibility()
    {
        if (camera != null)
            camera.JoystickRotationSpeed = joystickSens.value;
    }
}
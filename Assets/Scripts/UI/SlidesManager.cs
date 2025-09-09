using System;
using KinematicCharacterController.Examples;
using Player.New;
using UnityEngine;
using UnityEngine.UI;

public class SlidesManager : MonoBehaviour
{
    [SerializeField] private Slider mouseSens;
    [SerializeField] private Slider joystickSens;

    [SerializeField] private MyCharacterCamera camera; 
    private void Start()
    {
        mouseSens.value = camera.mouseRotationSpeed;
        joystickSens.value = camera.joystickRotationSpeed;
    }
    
    public void ChangeMouseSensibility()
    {
        camera.mouseRotationSpeed = mouseSens.value;
    }

    public void ChangeJoystickSensibility()
    {
        camera.joystickRotationSpeed = joystickSens.value;
    }
}

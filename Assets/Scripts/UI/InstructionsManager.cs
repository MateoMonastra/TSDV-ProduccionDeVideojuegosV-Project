using System;
using System.Collections.Generic;
using Player;
using UnityEngine;
using UnityEngine.InputSystem;

public class InstructionsManager : MonoBehaviour
{
    [SerializeField] InputReader inputReader;
    [SerializeField] GameObject pcInputsParent;
    [SerializeField] GameObject gamepadInputsParent;

    private bool isCurrentlyGamepad = false;
    
    private void OnEnable()
    {
        inputReader.OnInputPressed += InputPressed;
    }

    private void OnDisable()
    {
        inputReader.OnInputPressed -= InputPressed;
    }

    private void InputPressed(InputDevice obj)
    {
        if (obj.deviceId > 2)
        {
            if (isCurrentlyGamepad)
                return;
            
            isCurrentlyGamepad = true;
            gamepadInputsParent.SetActive(true);
            pcInputsParent.SetActive(false);
        }
        else
        {
            if (!isCurrentlyGamepad)
                return;
            
            isCurrentlyGamepad = false;
            gamepadInputsParent.SetActive(false);
            pcInputsParent.SetActive(true);
        }
    }
}
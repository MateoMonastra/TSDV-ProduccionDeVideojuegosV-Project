using System;
using UnityEngine;

public class ComicEvents : MonoBehaviour
{
    [SerializeField] private GameObject characterObject;
    [SerializeField] private GameObject characterCanvas;
    [SerializeField] private GameObject ComicCamera;
    [SerializeField] private GameObject introCanvas;
    [SerializeField] private GameObject outroCanvas;


    private void OnEnable()
    {
        GameEvents.GameEvents.OnLevelStarted += StartIntro;
        GameEvents.GameEvents.OnLevelEnded += StartOutro;
    }

    private void OnDisable()
    {
        GameEvents.GameEvents.OnLevelStarted -= StartIntro;
        GameEvents.GameEvents.OnLevelEnded -= StartOutro;
    }

    public void AwakenLevel()
    {
        characterObject.SetActive(true);
        characterCanvas.SetActive(true);
        introCanvas.SetActive(false);
        ComicCamera.SetActive(false);
    }

    public void StartIntro()
    {
        ComicCamera.SetActive(true);
        outroCanvas.SetActive(false);
    }

    public void StartOutro()
    {
        ComicCamera.SetActive(true);
        characterObject.SetActive(false);
    }
}
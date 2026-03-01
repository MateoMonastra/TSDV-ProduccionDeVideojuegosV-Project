using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ComicEvents : MonoBehaviour
{
    private static readonly int Outro = Animator.StringToHash("Outro");
    [SerializeField] private GameObject characterObject;
    [SerializeField] private GameObject characterCanvas;
    [SerializeField] private Camera ComicCamera;
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
        ComicCamera.enabled = false;
    }

    public void EndLevel()
    {
        SceneManager.LoadScene("SplashScene");
    }

    public void StartIntro()
    {
        ComicCamera.enabled = true;
        outroCanvas.SetActive(false);
    }

    public void StartOutro()
    {
        ComicCamera.GetComponent<Animator>().SetTrigger(Outro);
        ComicCamera.enabled = true;
        outroCanvas.SetActive(true);
        characterObject.SetActive(false);
        characterCanvas.SetActive(false);
    }
}
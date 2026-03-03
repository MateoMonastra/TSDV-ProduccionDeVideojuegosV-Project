using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    private static readonly int Trigger = Animator.StringToHash("Trigger");
    [SerializeField] private Animator animator;
    [SerializeField] private Animator escAnimator;

    public void TriggerFeedback()
    {
        animator.SetTrigger(Trigger);
        StartCoroutine(ChangeScene());
    }

    public void TriggerExit()
    {
        escAnimator.SetTrigger(Trigger);
        StartCoroutine(QuitApp());
    }
    
    private IEnumerator ChangeScene()
    {
        yield return new WaitForSeconds(0.9f);
        SceneManager.LoadScene("Lvl_1_Castillo");
    }
    
    private IEnumerator QuitApp()
    {
        yield return new WaitForSeconds(0.9f);
        Application.Quit();
    }
}
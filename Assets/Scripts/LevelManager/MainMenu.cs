using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    private static readonly int Trigger = Animator.StringToHash("Trigger");
    [SerializeField] private Animator animator;

    public void TriggerFeedback()
    {
        animator.SetTrigger(Trigger);
        StartCoroutine(ChangeScene());
    }

    private IEnumerator ChangeScene()
    {
        yield return new WaitForSeconds(0.9f);
        SceneManager.LoadScene("Lvl_1_Castillo");
    }
}

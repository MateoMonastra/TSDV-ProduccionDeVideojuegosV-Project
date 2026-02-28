using UnityEngine;

public class StampNotificationEvents : MonoBehaviour
{
   public void OnFinishNotification()
   {
      gameObject.SetActive(false);
   }
}

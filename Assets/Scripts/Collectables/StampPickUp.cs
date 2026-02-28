using UnityEngine;

namespace Collectables
{
    public class StampPickup : MonoBehaviour
    {
        [SerializeField] private string stampId;
        [SerializeField] private GameObject stampNotification;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            if (StampCollection.Instance != null)
            {
                bool newlyUnlocked = StampCollection.Instance.Unlock(stampId);

                if (newlyUnlocked) stampNotification.SetActive(true);
            }
            
            Destroy(gameObject);
        }
    }
}
using UnityEngine;

namespace Collectables
{
    public class StampPickup : MonoBehaviour
    {
        [SerializeField] private string stampId;
        [SerializeField] private GameObject stampNotification;
        [SerializeField] private GameObject pickUpParticles;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            if (StampCollection.Instance != null)
            {
                bool newlyUnlocked = StampCollection.Instance.Unlock(stampId);

                Instantiate(pickUpParticles, transform.position, Quaternion.identity);

                
                if (newlyUnlocked) stampNotification.SetActive(true);
            }
            
            Destroy(gameObject);
        }
    }
}
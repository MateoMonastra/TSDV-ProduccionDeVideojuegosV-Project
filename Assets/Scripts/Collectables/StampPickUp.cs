using UnityEngine;
using Event = AK.Wwise.Event;

namespace Collectables
{
    public class StampPickup : MonoBehaviour
    {
        [SerializeField] private string stampId;
        [SerializeField] private GameObject stampNotification;
        [SerializeField] private GameObject pickUpParticles;
        [SerializeField] private Event pickUpSfx;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            if (StampCollection.Instance != null)
            {
                bool newlyUnlocked = StampCollection.Instance.Unlock(stampId);

                Instantiate(pickUpParticles, transform.position, Quaternion.identity);
                pickUpSfx?.Post(gameObject);
                
                if (newlyUnlocked) stampNotification.SetActive(true);
            }
            
            Destroy(gameObject);
        }
    }
}
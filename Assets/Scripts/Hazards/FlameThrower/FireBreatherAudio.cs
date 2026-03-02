using Enemies.BaseEnemy;
using UnityEngine;
using Event = AK.Wwise.Event;

namespace Hazards.FlameThrower
{
    public class FireBreatherAudio : MonoBehaviour
    {
        [SerializeField] private FireBreatherAgent fireBreatherAgent;
        [SerializeField] private Event akFireIgnite;
        [SerializeField] private Event akFireStop;
        private void OnEnable()
        {
            fireBreatherAgent.onStartFireEvent.AddListener(OnFireIgnite);
            fireBreatherAgent.onStopFireEvent.AddListener(OnFireStop);
        }

        private void OnDisable()
        {
            fireBreatherAgent.onStartFireEvent.AddListener(OnFireIgnite);
            fireBreatherAgent.onStopFireEvent.AddListener(OnFireStop);
        }

        private void OnFireIgnite()
        {
            akFireIgnite?.Post(gameObject);
        }

        private void OnFireStop()
        {
            akFireStop?.Post(gameObject);
        }
    }
}
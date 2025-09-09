using UnityEngine;
using UnityEngine.Events;
using Player.New;

namespace CheckPoint
{
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    public class Checkpoint : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private CheckPointManagerRef checkpointManagerRef;

        [Header("Opciones")]
        [Tooltip("Si está activo, el checkpoint se consume y no vuelve a activarse.")]
        [SerializeField] private bool oneShot = false;

        [Tooltip("Al usar colisión, coloca el respawn sobre el top del collider del checkpoint.")]
        [SerializeField] private bool useSafeHeightOnCollision = true;

        [Header("Feedback")]
        public UnityEvent onActivate;
        [SerializeField] private GameObject visuals;

        private Collider _col;
        private Rigidbody _rb;
        private bool _used;

        private void Awake()
        {
            _col = GetComponent<Collider>();
            _rb  = GetComponent<Rigidbody>();
            
            _rb.isKinematic = true;
            _rb.useGravity  = false;

            if (!visuals) visuals = gameObject;
        }

        private void OnTriggerEnter(Collider other)
        {
            var agent = other.GetComponentInParent<PlayerAgent>();
            if (agent == null) return;
            ActivateCheckpoint(agent, fromCollision:false);
        }

        private void OnCollisionEnter(Collision other)
        {
            var agent = other.gameObject.GetComponentInParent<PlayerAgent>();
            if (agent == null) return;
            ActivateCheckpoint(agent, fromCollision:true);
        }

        private void ActivateCheckpoint(PlayerAgent agent, bool fromCollision)
        {
            if (_used && oneShot) return;

            var mgr = checkpointManagerRef ? checkpointManagerRef.manager : null;
            if (mgr == null) return;

     
            Vector3 pos;
            Quaternion rot = transform.rotation;

            if (fromCollision && useSafeHeightOnCollision)
                pos = CalculateSafeRespawnPosition(agent);
            else
                pos = transform.position;
            
            
            if (!mgr.IsLastCheckpoint(pos))
            {
                mgr.SetCheckpoint(pos, rot);
                onActivate?.Invoke();

                if (oneShot)
                {
                    _used = true;
                    if (visuals) visuals.SetActive(false);
                    _col.enabled = false;
                }
            }
        }

        private Vector3 CalculateSafeRespawnPosition(PlayerAgent agent)
        {
            var cpCol = _col;                           
            var pCol  = agent.GetComponentInChildren<Collider>();

            // Fallbacks seguros
            if (!cpCol) return transform.position;
            if (!pCol)  return cpCol.bounds.center;

            Vector3 cpCenter = cpCol.bounds.center;
            float cpTop = cpCol.bounds.max.y;
            float playerHeight = pCol.bounds.size.y;

            return new Vector3(
                cpCenter.x,
                cpTop + (playerHeight * 0.5f),
                cpCenter.z
            );
        }
    }
}

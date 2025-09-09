using System.Collections.Generic;
using Health;
using UnityEngine;

namespace Hazards.CandyPendulum
{
    /// <summary>
    /// Péndulo que oscila y daña al jugador cuando entra en su trigger.
    /// - Requiere Collider (isTrigger) + Rigidbody (isKinematic) para disparar OnTriggerEnter con KCC.
    /// - Usa LayerMask para filtrar jugador.
    /// - Envía DamageInfo(damage, origin, (horiz, vert)).
    /// - Cooldown para no golpear múltiples veces durante la misma pasada.
    /// - Gizmos para visualizar arco y área de colisión.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    public class CandyPendulum : MonoBehaviour
    {
        [System.Serializable]
        public struct Knockback
        {
            [Tooltip("Impulso/velocidad horizontal aplicada al víctima")]
            public int horizontal;
            [Tooltip("Impulso/velocidad vertical aplicada al víctima")]
            public int vertical;
        }

        [Header("Oscilación")]
        [SerializeField, Tooltip("Ángulo máximo (±) en grados")]
        private float swingAngle = 45f;
        [SerializeField, Tooltip("Ciclos por segundo (1 = ida y vuelta por segundo)")]
        private float swingSpeed = 1.5f;
        [SerializeField, Tooltip("Eje local de rotación (por defecto Z)")]
        private Vector3 localSwingAxis = Vector3.forward;

        [Header("Daño")]
        [SerializeField] private int damage = 1;
        [SerializeField] private Knockback knockback = new Knockback { horizontal = 20, vertical = 35 };

        [Header("Detección")]
        [SerializeField, Tooltip("Capas consideradas jugador")]
        private LayerMask playerMask = 0;

        [Header("Anti multi-hit")]
        [SerializeField, Tooltip("Tiempo mínimo entre golpes a la misma víctima")]
        private float rehitCooldown = 0.25f;
        
        private float _time;
        private Quaternion _initialRotation;
        private Collider _col;
        private Rigidbody _rb;
        
        private readonly Dictionary<HealthController, float> _nextAllowedHit = new();

        private void Awake()
        {
            _col = GetComponent<BoxCollider>();
            _rb  = GetComponent<Rigidbody>();
            
            _col.isTrigger = true;
            _rb.isKinematic = true;
            _rb.useGravity = false;
        }

        private void OnEnable()
        {
            _initialRotation = transform.localRotation;
        }

        private void Update()
        {
            _time += Time.deltaTime;
            
            float angle = swingAngle * Mathf.Sin(_time * swingSpeed * Mathf.PI * 2f);
            
            Quaternion swing = Quaternion.AngleAxis(angle, localSwingAxis.normalized);
            transform.localRotation = _initialRotation * swing;
            
            if (_nextAllowedHit.Count > 0)
            {
                float now = Time.time;
                var toRemove = new List<HealthController>();
                foreach (var kvp in _nextAllowedHit)
                {
                    if (now >= kvp.Value) toRemove.Add(kvp.Key);
                }
                for (int i = 0; i < toRemove.Count; i++) _nextAllowedHit.Remove(toRemove[i]);
            }
        }

        private static bool IsInLayerMask(int layer, LayerMask mask) => (mask.value & (1 << layer)) != 0;

        private void OnTriggerEnter(Collider other)
        {
            int layer = other.gameObject.layer;
            if (!IsInLayerMask(layer, playerMask)) return;
            
            var controller = other.GetComponentInParent<HealthController>();
            if (!controller) return;
            
            float now = Time.time;
            if (_nextAllowedHit.TryGetValue(controller, out var next) && now < next)
                return;

            _nextAllowedHit[controller] = now + rehitCooldown;
            
            controller.Damage(new DamageInfo(damage, transform.position, (knockback.horizontal, knockback.vertical)));
        }
    }
}

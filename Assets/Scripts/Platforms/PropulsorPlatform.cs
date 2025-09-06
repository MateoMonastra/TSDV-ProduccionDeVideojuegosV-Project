using System.Collections.Generic;
using Player.New;
using UnityEngine;

namespace Platforms
{
    /// <summary>
    /// Plataforma propulsora para el nuevo Kinematic Controller.
    /// Al entrar el Player, aplica un impulso en la 'up' de la plataforma.
    /// Se ejecuta una sola vez por "stay" y se vuelve a habilitar al salir.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    public class PropulsorPlatform : MonoBehaviour
    {
        [Header("Impulse")]
        [SerializeField, Tooltip("Velocidad de salida que se aplica en la dirección UP de la plataforma (m/s)")]
        private float launchSpeed = 12f;

        [SerializeField, Tooltip("Sólo impulsa si el player está grounded (recomendado).")]
        private bool onlyWhenGrounded = true;

        [Header("Debug / Safety")]
        [SerializeField, Tooltip("Loguear activaciones en consola")]
        private bool logs;

        private readonly Dictionary<Collider, bool> _consumed = new();

        private Collider _col;
        private Rigidbody _rb;

        private void Awake()
        {
            _col = GetComponent<Collider>();
            _rb  = GetComponent<Rigidbody>();
            
            _col.isTrigger      = true;
            _rb.isKinematic     = true;
            _rb.useGravity      = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!_consumed.ContainsKey(other))
                _consumed.Add(other, false);
        }

        private void OnTriggerStay(Collider other)
        {
            if (!_consumed.ContainsKey(other))
                _consumed.Add(other, false);
            
            if (_consumed[other]) return;
            
            var agent = other.GetComponentInParent<PlayerAgent>();
            if (agent == null) return;

            var motor = agent.GetComponent<MyKinematicMotor>();
            if (motor == null) return;
            
            if (onlyWhenGrounded && !motor.IsGrounded)
                return;

           
            Vector3 dir = transform.up.normalized;
            Vector3 v   = motor.Velocity;
            
            v = Vector3.ProjectOnPlane(v, dir) + (dir * Mathf.Max(0f, launchSpeed));
            motor.SetVelocity(v);
            motor.ForceUnground(0.1f);
            
            _consumed[other] = true;

            if (logs) Debug.Log($"PropulsorPlatform: impulso aplicado a {agent.name} → {v}", this);
            
            agent.GetPlayerModel().ClearActionLocks();
        }

        private void OnTriggerExit(Collider other)
        {
            if (_consumed.ContainsKey(other))
                _consumed.Remove(other);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position, transform.up * 1.0f);
        }
#endif
    }
}

using System;
using Enemies.BaseEnemy.States;
using FSM;
using Health;
using UnityEngine;
using UnityEngine.AI;

namespace Enemies.BaseEnemy
{
    /// <summary>
    /// Orquesta la FSM del enemigo, conecta animaciones/daño/IA (NavMesh)
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public class BaseEnemyAgent : MonoBehaviour
    {
        // ───────────────────────────────────────────────────────────────────────
        #region Inspector References
        [Header("Refs")]
        [SerializeField] private Transform player;
        [SerializeField] private BaseEnemyModel model;
        [SerializeField] private NavMeshAgent navMeshAgent;
        [SerializeField] private Rigidbody rb;
        [SerializeField] private Collider hitBox;
        [SerializeField] private EnemyAnimationController anims;
        [SerializeField] private HealthController health;
        #endregion

        // ───────────────────────────────────────────────────────────────────────
        #region FSM (Core)
        private Fsm _fsm;
        private Idle    _sIdle;
        private Chase   _sChase;
        private Attack  _sAttack;
        private Impulse _sImpulse;
        private Death   _sDeath;

        private EnemyContext _ctx;
        private bool _god;
        
        private const string ToChaseID = "ToChase";
        private const string ToIdleID = "ToIdle";
        private const string ToImpulseID = "ToImpulse";
        private const string ToAttackID = "ToAttack";
        
        #endregion

        // ───────────────────────────────────────────────────────────────────────
        #region Unity Messages
        private void Awake()
        {
            InitRefs();
            BuildFsm();
        }

        private void OnEnable()
        {
            if (health)
            {
                health.OnTakeDamage += OnDamaged;
                health.OnDeath      += OnDied;
            }
        }

        private void OnDisable()
        {
            if (health)
            {
                health.OnTakeDamage -= OnDamaged;
                health.OnDeath      -= OnDied;
            }
        }

        private void Update()
        {
            if (_god) return;
            _fsm?.Update();
        }

        private void FixedUpdate()
        {
            if (_god) return;
            _fsm?.FixedUpdate();
        }
        #endregion

        // ───────────────────────────────────────────────────────────────────────
        #region Event Handlers
        private void OnDamaged(DamageInfo info)
        {
            _fsm?.ForceTransition(_sImpulse);
        }

        private void OnDied()
        {
            if (_fsm.GetCurrentState() == _sDeath) return;
            _fsm?.ForceTransition(_sDeath);
        }
        #endregion

        // ───────────────────────────────────────────────────────────────────────
        #region Setup / Wiring
        private void InitRefs()
        {
            if (!rb) rb = GetComponent<Rigidbody>();

            if (navMeshAgent)
            {
                navMeshAgent.updateRotation  = false;
                navMeshAgent.stoppingDistance = Mathf.Max(0.1f, model.AttackRange * 0.9f);
            }

            _ctx = new EnemyContext(transform, player, model, navMeshAgent, rb, hitBox, anims);
        }

        private void BuildFsm()
        {
            void Go(string id) => _fsm.TryTransitionTo(id);

            _sIdle    = new Idle(_ctx,   toChase:  () => Go(ToChaseID));
            _sChase   = new Chase(_ctx,  toIdle:   () => Go(ToIdleID),
                                                                  toAttack: () => Go(ToAttackID));
            _sAttack  = new Attack(_ctx, toChase:  () => Go(ToChaseID));
            _sImpulse = new Impulse(_ctx,onEnd:    () => Go(ToChaseID));

            _sDeath = new Death(_ctx);
            
            _sIdle.AddTransition   (new Transition { From = _sIdle,   To = _sChase,   ID = ToChaseID });
            _sIdle.AddTransition   (new Transition { From = _sIdle,   To = _sImpulse, ID = ToImpulseID});
            _sChase.AddTransition  (new Transition { From = _sChase,  To = _sAttack,  ID = ToAttackID });
            _sChase.AddTransition  (new Transition { From = _sChase,  To = _sIdle,    ID = ToIdleID   });
            _sChase.AddTransition  (new Transition { From = _sChase,  To = _sImpulse, ID = ToImpulseID});
            _sAttack.AddTransition (new Transition { From = _sAttack, To = _sChase,   ID =ToChaseID  });
            _sAttack.AddTransition (new Transition { From = _sAttack, To = _sImpulse, ID = ToImpulseID});
            _sImpulse.AddTransition(new Transition { From = _sImpulse,To = _sChase,   ID = ToChaseID  });

            _fsm = new Fsm(_sIdle);
        }
        #endregion

        // ───────────────────────────────────────────────────────────────────────
        #region Utilities
        public void SetGodMode(bool on)
        {
            _god = on;
            if (navMeshAgent) navMeshAgent.isStopped = on;
            if (on) anims?.SetWalkAnimation(false);
        }

        public void SetIdle()  => _fsm?.ForceTransition(_sIdle);
        public void SetChase() => _fsm?.ForceTransition(_sChase);
        #endregion
    }
}

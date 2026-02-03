using System.Collections.Generic;
using Enemies.BaseEnemy.States;
using FSM;
using Health;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace Enemies.BaseEnemy
{
    public class BaseEnemyAgent : MonoBehaviour, IEnemy
    {
        public UnityEvent onAttackDelay;
        public UnityEvent onAttackHit;
        public UnityEvent onAttackFinish;
        public UnityEvent onImpulseStarted;
        public UnityEvent onImpulseEnded;
        public UnityEvent onSpinningImpulseStarted;
        public UnityEvent onSpinningImpulseEnded;
        public UnityEvent<bool> onChase;
        public UnityEvent onIdle;
        public UnityEvent onDeath;

        //TODO: pasar conocimiento del player a un scriptable object
        [SerializeField] private HealthController healthController;
        [SerializeField] private Transform player;
        [SerializeField] private BaseEnemyModel model;
        [SerializeField] private NavMeshAgent navMeshAgent;
        [SerializeField] private Rigidbody rigidbody;
        [SerializeField] private Collider hitBox;
        [SerializeField] private EnemyAnimationController animator;
        [SerializeField] private TrailRenderer trailRenderer;

        private Fsm _fsm;

        private State _deathImpulse;
        private Impulse impulse;
        private SpinningImpulse _spinningImpulse;

        private List<State> _states = new List<State>();
        private bool _isGodModeActive = false;

        private const string ToChaseID = "toChase";
        private const string ToAttackID = "toAttack";
        private const string ToIdleID = "toIdle";
        private const string ToImpulseID = "toImpulse";
        private const string ToSpinningImpulseID = "toSpinningImpulse";
        private const string ToDeathID = "toDeath";

        private void Start()
        {
            State idle = new Idle(this.transform, player, model, TransitionToChase);

            State attack = new Attack(this.transform, player, model, navMeshAgent, hitBox, animator, AttackOnDelay,
                AttackOnHit,
                TransitionToChase);

            State chase = new Chase(this.transform, player, model, navMeshAgent,
                onExitChase: TransitionToIdle,
                onEnterAttack: TransitionToAttack);

            impulse = new Impulse(this.transform, player, model, navMeshAgent, rigidbody,
                onImpulseStarted: ImpulseOnStart, onImpulseEnded: ImpulseOnEnd);

            _spinningImpulse = new SpinningImpulse(this.transform, player, trailRenderer, model, navMeshAgent,
                rigidbody,
                onImpulseStarted: ImpulseOnStart, onImpulseEnded: ImpulseOnEnd);

            _deathImpulse = new Impulse(this.transform, player, model, navMeshAgent, rigidbody,
                onImpulseStarted: ImpulseOnStart, onImpulseEnded: DeathImpulseOnEnd);

            State death = new Death(this.gameObject, model);
            _states.Add(death);

            //Idle Transitions
            Transition idleToChase = new Transition() { From = idle, To = chase, ID = ToChaseID };
            idle.AddTransition(idleToChase);

            Transition idleToImpulse = new Transition() { From = idle, To = impulse, ID = ToImpulseID };
            idle.AddTransition(idleToImpulse);

            Transition idleToSpinImpulse = new Transition() { From = idle, To = _spinningImpulse, ID = ToImpulseID };
            idle.AddTransition(idleToSpinImpulse);
            _states.Add(idle);

            //Chase Transitions
            Transition chaseToAttack = new Transition() { From = chase, To = attack, ID = ToAttackID };
            chase.AddTransition(chaseToAttack);

            Transition chaseToIdle = new Transition() { From = chase, To = idle, ID = ToIdleID };
            chase.AddTransition(chaseToIdle);

            Transition chaseToImpulse = new Transition() { From = chase, To = impulse, ID = ToImpulseID };
            chase.AddTransition(chaseToImpulse);

            Transition chaseToSpinImpulse = new Transition()
                { From = chase, To = _spinningImpulse, ID = ToSpinningImpulseID };
            chase.AddTransition(chaseToSpinImpulse);
            _states.Add(chase);

            //Attack Transitions
            Transition attackToChase = new Transition() { From = attack, To = chase, ID = ToChaseID };
            attack.AddTransition(attackToChase);

            Transition attackToImpulse = new Transition() { From = attack, To = impulse, ID = ToImpulseID };
            attack.AddTransition(attackToImpulse);

            Transition attackToSpinImpulse = new Transition()
                { From = attack, To = _spinningImpulse, ID = ToSpinningImpulseID };
            attack.AddTransition(attackToSpinImpulse);
            _states.Add(attack);

            //Impulse transitions
            Transition impulseToChase = new Transition() { From = impulse, To = chase, ID = ToChaseID };
            impulse.AddTransition(impulseToChase);

            Transition impulseToImpulse = new Transition() { From = impulse, To = impulse, ID = ToImpulseID };
            impulse.AddTransition(impulseToImpulse);

            Transition impulseToSpinImpulse = new Transition()
                { From = impulse, To = _spinningImpulse, ID = ToSpinningImpulseID };
            impulse.AddTransition(impulseToSpinImpulse);
            _states.Add(impulse);

            //Spin impulse transition

            Transition spinImpulseToImpulse = new Transition()
                { From = _spinningImpulse, To = impulse, ID = ToImpulseID };
            _spinningImpulse.AddTransition(spinImpulseToImpulse);

            Transition spinImpulseToSpinImpulse = new Transition()
                { From = _spinningImpulse, To = _spinningImpulse, ID = ToSpinningImpulseID };
            _spinningImpulse.AddTransition(spinImpulseToSpinImpulse);

            Transition spinImpulseToChase = new Transition() { From = _spinningImpulse, To = chase, ID = ToChaseID };
            _spinningImpulse.AddTransition(spinImpulseToChase);
            _states.Add(_spinningImpulse);

            //Death Impulse transitions
            Transition deathImpulseToDeath = new Transition() { From = _deathImpulse, To = death, ID = ToDeathID };
            _deathImpulse.AddTransition(deathImpulseToDeath);
            _states.Add(_deathImpulse);

            _fsm = new Fsm(idle);
        }

        private void OnEnable()
        {
            GameEvents.GameEvents.OnPlayerGodMode += SetGodModeValue;
            healthController.OnTakeDamage += OnBeingAttacked;
            healthController.OnDeath += TransitionToDeathImpulse;
        }

        private void OnDisable()
        {
            GameEvents.GameEvents.OnPlayerGodMode -= SetGodModeValue;
            healthController.OnTakeDamage -= OnBeingAttacked;
            healthController.OnDeath -= TransitionToDeathImpulse;
        }

        private void TransitionToChase()
        {
            onChase?.Invoke(true);
            _fsm.TryTransitionTo(ToChaseID);
        }

        private void TransitionToAttack()
        {
            _fsm.TryTransitionTo(ToAttackID);
        }

        private void TransitionToIdle()
        {
            onIdle?.Invoke();
            _fsm.TryTransitionTo(ToIdleID);
        }

        private void TransitionToImpulse()
        {
            _fsm.TryTransitionTo(ToImpulseID);
        }

        private void TransitionToSpinningImpulse()
        {
            _fsm.TryTransitionTo(ToSpinningImpulseID);
        }

        private void TransitionToDeath()
        {
            onDeath?.Invoke();
            _fsm.TryTransitionTo(ToDeathID);
        }

        private void TransitionToDeathImpulse()
        {
            if (_fsm.GetCurrentState() != _deathImpulse)
            {
                _fsm.ForceTransition(_deathImpulse);
            }
        }

        private void AttackOnDelay()
        {
            onAttackDelay?.Invoke();
        }

        private void AttackOnHit()
        {
            onAttackHit?.Invoke();
        }

        private void ImpulseOnStart()
        {
            onImpulseStarted?.Invoke();
        }

        private void ImpulseOnEnd()
        {
            onImpulseEnded?.Invoke();

            TransitionToChase();
        }

        private void DeathImpulseOnEnd()
        {
            onImpulseEnded?.Invoke();

            TransitionToDeath();
        }

        private void SetGodModeValue(bool value)
        {
            _isGodModeActive = value;
        }

        private void Update()
        {
            if (!_isGodModeActive)
                _fsm.Update();
        }

        private void FixedUpdate()
        {
            if (!_isGodModeActive)
                _fsm.FixedUpdate();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, model.InnerRadius);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, model.OuterRadius);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, model.AttackRange);
        }

        public void OnBeingAttacked(DamageInfo damageOrigin)
        {
            if (healthController.GetCurrentHealth() > 0)
            {
                if (damageOrigin.DamageName == "PlayerVerticalAttack")
                {
                    _spinningImpulse.SetImpulse(damageOrigin.Knockback);
                    _spinningImpulse.SetImpulseSource(damageOrigin.DamageOrigin);
                    TransitionToSpinningImpulse();
                }
                else
                {
                    impulse.SetImpulse(damageOrigin.Knockback);
                    impulse.SetImpulseSource(damageOrigin.DamageOrigin);
                    TransitionToImpulse();
                }
            }
        }
    }
}
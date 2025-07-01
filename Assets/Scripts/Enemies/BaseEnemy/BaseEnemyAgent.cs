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

        private Fsm _fsm;
        private List<State> _states = new List<State>();
        private bool _isGodModeActive = false;
        private bool _isDeath = false;

        private const string ToChaseID = "toChase";
        private const string ToAttackID = "toAttack";
        private const string ToIdleID = "toIdle";
        private const string ToImpulseID = "toImpulse";

        private void Awake()
        {
            healthController.OnTakeDamage += OnBeingAttacked;
            healthController.OnDeath += TransitionToDeath;
        }

        private void Start()
        {
            State idle = new Idle(this.transform, player, model, TransitionToChase);

            State attack = new Attack(this.transform, player, model, navMeshAgent, hitBox, AttackOnDelay, AttackOnHit,
                TransitionToChase);

            State chase = new Chase(this.transform, player, model, navMeshAgent,
                onExitChase: TransitionToIdle,
                onEnterAttack: TransitionToAttack);

            State impulse = new Impulse(this.transform, player, model, navMeshAgent, rigidbody, onImpulseStarted: ImpulseOnStart, onImpulseEnded: ImpulseOnEnd);

            //Idle Transitions
            Transition idleToChase = new Transition() { From = idle, To = chase, ID = ToChaseID };
            idle.AddTransition(idleToChase);

            Transition idleToImpulse = new Transition() { From = idle, To = impulse, ID = ToImpulseID };
            idle.AddTransition(idleToImpulse);
            _states.Add(idle);

            //Chase Transitions
            Transition chaseToAttack = new Transition() { From = chase, To = attack, ID = ToAttackID };
            chase.AddTransition(chaseToAttack);

            Transition chaseToIdle = new Transition() { From = chase, To = idle, ID = ToIdleID };
            chase.AddTransition(chaseToIdle);

            Transition chaseToImpulse = new Transition() { From = chase, To = impulse, ID = ToImpulseID };
            chase.AddTransition(chaseToImpulse);
            _states.Add(chase);
            
            //Atack Transitions
            Transition attackToChase = new Transition() { From = attack, To = chase, ID = ToChaseID };
            attack.AddTransition(attackToChase);
            
            Transition attackToImpulse = new Transition() { From = attack, To = impulse, ID = ToImpulseID };
            attack.AddTransition(attackToImpulse);
            _states.Add(attack);

            //Impulse transitions
            Transition impulseToChase = new Transition() { From = impulse, To = chase, ID = ToChaseID };
            impulse.AddTransition(impulseToChase);
            _states.Add(impulse);
            
            _fsm = new Fsm(idle);
        }
        
        private void OnEnable()
        {
            GameEvents.GameEvents.OnPlayerGodMode += SetGodModeValue;
        }

        private void OnDisable()
        {
            GameEvents.GameEvents.OnPlayerGodMode -= SetGodModeValue;
        }

        private void OnDestroy()
        {
            healthController.OnTakeDamage -= OnBeingAttacked;
            healthController.OnDeath -= TransitionToDeath;
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
        
        private void TransitionToDeath()
        {
            onDeath?.Invoke();
            _isDeath = true;
            State death = new Death(this.gameObject, model);
            _fsm.ForceSetCurrentState(death);
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

        public void OnBeingAttacked()
        {
            TransitionToImpulse();
        }
    }
}
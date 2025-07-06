using System.Collections.Generic;
using FSM;
using Hazards.Cannon.States;
using UnityEngine;
using UnityEngine.Events;

namespace Hazards.Cannon
{
    public class CannonAgent : MonoBehaviour
    {
        public UnityEvent onAttack;
        public UnityEvent onIdle;
        public UnityEvent onDeath;

        [SerializeField] private CannonModel model;
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private GameObject groundMarkPrefab;
        [SerializeField] private Transform shootPoint;
        [SerializeField] private Transform target;

        private Fsm _fsm;
        private List<State> _states = new List<State>();
        private bool _isGodModeActive = false;

        private const string ToAttackID = "toAttack";
        private const string ToIdleID = "toIdle";
        private const string ToDeathID = "toDeath";
        
        private void Start()
        {
            State idle = new Idle(transform, target, model, TransitionToAttack);

            State attack = new Attack(shootPoint, bulletPrefab, groundMarkPrefab, target, model, TransitionToIdle);

            State death = new Death();
            _states.Add(death);

            //Idle Transitions
            Transition idleToAttack = new Transition() { From = idle, To = attack, ID = ToAttackID };
            idle.AddTransition(idleToAttack);
            _states.Add(idle);

            //Attack Transitions
            Transition attackToIdle = new Transition() { From = attack, To = idle, ID = ToIdleID };
            attack.AddTransition(attackToIdle);

            Transition attackToDeath = new Transition() { From = attack, To = death, ID = ToDeathID };
            attack.AddTransition(attackToDeath);
            _states.Add(attack);

            _fsm = new Fsm(idle);
        }

        private void TransitionToAttack()
        {
            onAttack.Invoke();
            _fsm.TryTransitionTo(ToAttackID);
        }

        private void TransitionToDeath()
        {
            onDeath.Invoke();
            _fsm.TryTransitionTo(ToDeathID);
        }

        private void TransitionToIdle()
        {
            onIdle.Invoke();
            _fsm.TryTransitionTo(ToIdleID);
        }

        private void OnEnable()
        {
            GameEvents.GameEvents.OnPlayerGodMode += SetGodModeValue;
        }

        private void OnDisable()
        {
            GameEvents.GameEvents.OnPlayerGodMode -= SetGodModeValue;
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
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, model.AttackRange);
        }
    }
}
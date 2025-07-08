using System.Collections.Generic;
using Enemies.RangeEnemy.States;
using FSM;
using Health;
using UnityEngine;
using UnityEngine.Events;

namespace Enemies.RangeEnemy
{
    public class RangedEnemyAgent : MonoBehaviour, IEnemy
    {
        public UnityEvent onAttack;
        public UnityEvent onIdle;
        public UnityEvent onSpecialAttack;
        public UnityEvent onDeath;

        //TODO: pasar conocimiento del player a un scriptable object
        [SerializeField] private ParticleSystem shootParticle;
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private GameObject specialBulletPrefab;
        [SerializeField] private GameObject groundMarkerPrefab;
        [SerializeField] private Transform shootPoint;
        [SerializeField] private Transform player;
        [SerializeField] private RangedEnemyModel model;
        [SerializeField] private HealthController healthController;

        private Fsm _fsm;
        private List<State> _states = new List<State>();
        private bool _isGodModeActive = false;

        private const string ToAttackID = "toAttack";
        private const string ToIdleID = "toIdle";
        private const string ToSpecialAttackID = "toSpecialAttack";

        private void Start()
        {
            State idle = new Idle(this.transform, player, model, TransitionToAttack,
                TransitionToSpecialAttack);

            State specialAttack = new SpecialAttack(this.transform, player, model, TransitionToIdle,
                groundMarkerPrefab, specialBulletPrefab, shootPoint.position);

            State attack = new Attack(this.transform, player, model, bulletPrefab, shootParticle, shootPoint,
                TransitionToIdle);

            //Idle Transitions
            Transition idleToAttack = new Transition() { From = idle, To = attack, ID = ToAttackID };
            idle.AddTransition(idleToAttack);

            Transition idleToSpecialAttack = new Transition()
                { From = idle, To = specialAttack, ID = ToSpecialAttackID };
            idle.AddTransition(idleToSpecialAttack);
            _states.Add(idle);

            //Atack Transitions
            Transition attackToIdle = new Transition() { From = attack, To = idle, ID = ToIdleID };
            attack.AddTransition(attackToIdle);
            _states.Add(attack);

            //SpecialAttack Transitions
            Transition specialAttackToIdle = new Transition() { From = specialAttack, To = idle, ID = ToIdleID };
            specialAttack.AddTransition(specialAttackToIdle);
            _states.Add(attack);


            _fsm = new Fsm(idle);
        }

        private void OnEnable()
        {
            GameEvents.GameEvents.OnPlayerGodMode += SetGodModeValue;
            healthController.OnDeath += OnBeingAttacked;
        }

        private void OnDisable()
        {
            GameEvents.GameEvents.OnPlayerGodMode -= SetGodModeValue;
            healthController.OnDeath -= OnBeingAttacked;
        }

        private void TransitionToAttack()
        {
            onAttack.Invoke();
            _fsm.TryTransitionTo(ToAttackID);
        }

        private void TransitionToIdle()
        {
            onIdle.Invoke();
            _fsm.TryTransitionTo(ToIdleID);
        }

        private void TransitionToSpecialAttack()
        {
            onSpecialAttack.Invoke();
            _fsm.TryTransitionTo(ToSpecialAttackID);
        }

        private void TransitionToDeath()
        {
            onDeath.Invoke();
            State death = new Death(this.gameObject);
            _fsm.ForceSetCurrentState(death);
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

        public void OnBeingAttacked(DamageInfo damageOrigin)
        {
            TransitionToDeath();
        }
    }
}
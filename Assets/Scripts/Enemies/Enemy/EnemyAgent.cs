using System.Collections.Generic;
using Enemies.Enemy.States;
using FSM;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace Enemies.Enemy
{
    public class EnemyAgent : MonoBehaviour
    {
        public UnityEvent onAttack;
        public UnityEvent<bool> onChase;
        public UnityEvent onIdle;
        public UnityEvent onDeath;

        //TODO: pasar conocimiento del player a un scriptable object
        [SerializeField] private Transform player;
        [SerializeField] private BaseEnemyModel model;
        [SerializeField] private NavMeshAgent navMeshAgent;

        private Fsm _fsm;
        private List<State> _states = new List<State>();

        private const string ToChaseID = "toChase";
        private const string ToAttackID = "toAttack";
        private const string ToIdleID = "toIdle";

        private void OnEnable()
        {
            State idle = new Idle(this.transform, player, model, TransitionToChase);

            State attack = new Attack(this.transform, player, model, navMeshAgent, TransitionToChase);

            State chase = new Chase(this.transform, player, model, navMeshAgent,
                onExitChase: TransitionToIdle,
                onEnterAttack: TransitionToAttack);

            //Idle Transitions
            Transition idleToChase = new Transition() { From = idle, To = chase, ID = ToChaseID };
            idle.AddTransition(idleToChase);
            _states.Add(idle);

            //Chase Transitions
            Transition chaseToAttack = new Transition() { From = chase, To = attack, ID = ToAttackID };
            chase.AddTransition(chaseToAttack);

            Transition chaseToIdle = new Transition() { From = chase, To = idle, ID = ToIdleID };
            chase.AddTransition(chaseToIdle);
            _states.Add(chase);

            //Atack Transitions
            Transition attackToChase = new Transition() { From = attack, To = chase, ID = ToChaseID };
            attack.AddTransition(attackToChase);
            _states.Add(attack);


            _fsm = new Fsm(idle);
        }

        private void TransitionToChase()
        {
            onChase.Invoke(true);
            _fsm.TryTransitionTo(ToChaseID);
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

        public void TransitionToDeath()
        {
            onDeath.Invoke();
            State death = new Death(this.gameObject);
            _fsm.ForceSetCurrentState(death);
        }

        private void Update()
        {
            _fsm.Update();
        }

        private void FixedUpdate()
        {
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
    }
}
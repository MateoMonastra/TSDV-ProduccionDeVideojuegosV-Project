using System;
using System.Collections.Generic;
using FSM;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Enemy
{
    public class EnemyAgent : MonoBehaviour
    {
        public UnityEvent onAttack;
        public UnityEvent<bool> onChase;
        public UnityEvent onIdle;
        
        [SerializeField] private Transform player;
        [SerializeField] private EnemyModel model;
        [SerializeField] private NavMeshAgent navMeshAgent;

        private Fsm _fsm;
        public List<State> _states = new List<State>();

        private State death;
        private void OnEnable()
        {
            State idle = new Idle(this.transform, player, model, TransitionToChase);

            State attack = new Attack(this.transform, player, navMeshAgent, TransitionToChase, model);
            
            death = new Death(this.gameObject);
            _states.Add(death);

            State chase = new Chase(this.transform, player, navMeshAgent, model,
                onExitChase: TransitionToIdle,
                onEnterAttack: TransitionToAttack);

            //Idle Transitions
            Transition idleToChase = new Transition() { From = idle, To = chase, ID = "toChase" };
            idle.AddTransition(idleToChase);
            _states.Add(idle);

            //Chase Transitions
            Transition chaseToAttack = new Transition() { From = chase, To = attack, ID = "toAttack" };
            chase.AddTransition(chaseToAttack);

            Transition chaseToIdle = new Transition() { From = chase, To = idle, ID = "toIdle" };
            chase.AddTransition(chaseToIdle);
            _states.Add(chase);

            //Atack Transitions
            Transition attackToChase = new Transition() { From = attack, To = chase, ID = "toChase" };
            attack.AddTransition(attackToChase);
            _states.Add(attack);


            _fsm = new Fsm(idle);
        }

        private void TransitionToChase()
        {
            onChase.Invoke(true);
            _fsm.TryTransitionTo("toChase");
        }

        private void TransitionToAttack()
        {
            onAttack.Invoke();
            _fsm.TryTransitionTo("toAttack");
        }

        private void TransitionToIdle()
        {
            onIdle.Invoke();
            _fsm.TryTransitionTo("toIdle");
        }
        public void TransitionToDeath()
        {
            _fsm.SetCurrentState(death);
            Debug.Log("death");
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
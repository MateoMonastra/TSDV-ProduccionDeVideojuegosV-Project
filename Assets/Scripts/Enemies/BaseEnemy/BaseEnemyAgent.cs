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
        [SerializeField] private Collider bodyCollider;
        [SerializeField] private EnemyAnimationController animator;
        [SerializeField] private TrailRenderer trailRenderer;

        private Fsm _fsm;

        private Impulse _deathImpulse;
        private Impulse impulse;
        private SpinningVerticalImpulse _spinningVerticalImpulse;
        private SpinningHorizontalImpulse _spinningHorizontalImpulse;

        private List<State> _states = new List<State>();
        private bool _isGodModeActive = false;

        private const string ToChaseID = "toChase";
        private const string ToAttackID = "toAttack";
        private const string ToIdleID = "toIdle";
        private const string ToImpulseID = "toImpulse";
        private const string ToSpinningVerticalImpulseID = "toSpinningVerticalImpulse";
        private const string ToSpinningHorizontalImpulseID = "toSpinningHorizontalImpulse";
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

            _spinningVerticalImpulse = new SpinningVerticalImpulse(this.transform, player, trailRenderer, model,
                navMeshAgent,
                rigidbody,
                onImpulseStarted: ImpulseOnStart, onImpulseEnded: ImpulseOnEnd);

            _spinningHorizontalImpulse = new SpinningHorizontalImpulse(this.transform, player, model, navMeshAgent,
                rigidbody, onImpulseStarted: ImpulseOnStart, onImpulseEnded: DeathImpulseOnEnd);

            _deathImpulse = new Impulse(this.transform, player, model, navMeshAgent, rigidbody,
                onImpulseStarted: ImpulseOnStart, onImpulseEnded: DeathImpulseOnEnd);

            State death = new Death(this.gameObject, model);
            _states.Add(_deathImpulse);

            //Idle Transitions
            Transition idleToChase = new Transition() { From = idle, To = chase, ID = ToChaseID };
            idle.AddTransition(idleToChase);

            Transition idleToImpulse = new Transition() { From = idle, To = impulse, ID = ToImpulseID };
            idle.AddTransition(idleToImpulse);

            Transition idleToSpinVerticalImpulse = new Transition()
                { From = idle, To = _spinningVerticalImpulse, ID = ToSpinningVerticalImpulseID };
            idle.AddTransition(idleToSpinVerticalImpulse);

            Transition idleToSpinHorizontalImpulse = new Transition()
                { From = idle, To = _spinningHorizontalImpulse, ID = ToSpinningHorizontalImpulseID };
            idle.AddTransition(idleToSpinHorizontalImpulse);
            _states.Add(idle);

            //Chase Transitions
            Transition chaseToAttack = new Transition() { From = chase, To = attack, ID = ToAttackID };
            chase.AddTransition(chaseToAttack);

            Transition chaseToIdle = new Transition() { From = chase, To = idle, ID = ToIdleID };
            chase.AddTransition(chaseToIdle);

            Transition chaseToImpulse = new Transition() { From = chase, To = impulse, ID = ToImpulseID };
            chase.AddTransition(chaseToImpulse);

            Transition chaseToSpinVerticalImpulse = new Transition()
                { From = chase, To = _spinningVerticalImpulse, ID = ToSpinningVerticalImpulseID };
            chase.AddTransition(chaseToSpinVerticalImpulse);

            Transition chaseToSpinHorizontalImpulse = new Transition()
                { From = chase, To = _spinningHorizontalImpulse, ID = ToSpinningHorizontalImpulseID };
            chase.AddTransition(chaseToSpinHorizontalImpulse);
            _states.Add(chase);

            //Attack Transitions
            Transition attackToChase = new Transition() { From = attack, To = chase, ID = ToChaseID };
            attack.AddTransition(attackToChase);

            Transition attackToImpulse = new Transition() { From = attack, To = impulse, ID = ToImpulseID };
            attack.AddTransition(attackToImpulse);

            Transition attackToSpinVerticalImpulse = new Transition()
                { From = attack, To = _spinningVerticalImpulse, ID = ToSpinningVerticalImpulseID };
            attack.AddTransition(attackToSpinVerticalImpulse);

            Transition attackToSpinHorizontalImpulse = new Transition()
                { From = attack, To = _spinningHorizontalImpulse, ID = ToSpinningHorizontalImpulseID };
            attack.AddTransition(attackToSpinHorizontalImpulse);
            _states.Add(attack);

            //Impulse transitions
            Transition impulseToChase = new Transition() { From = impulse, To = chase, ID = ToChaseID };
            impulse.AddTransition(impulseToChase);

            Transition impulseToImpulse = new Transition() { From = impulse, To = impulse, ID = ToImpulseID };
            impulse.AddTransition(impulseToImpulse);

            Transition impulseToSpinVerticalImpulse = new Transition()
                { From = impulse, To = _spinningVerticalImpulse, ID = ToSpinningVerticalImpulseID };
            impulse.AddTransition(impulseToSpinVerticalImpulse);

            Transition impulseToSpinHorizontalImpulse = new Transition()
            {
                From = impulse, To = _spinningHorizontalImpulse, ID = ToSpinningHorizontalImpulseID
            };
            impulse.AddTransition(impulseToSpinHorizontalImpulse);
            _states.Add(impulse);

            //Spin impulse transition

            Transition spinImpulseToImpulse = new Transition()
                { From = _spinningVerticalImpulse, To = impulse, ID = ToImpulseID };
            _spinningVerticalImpulse.AddTransition(spinImpulseToImpulse);

            Transition spinImpulseToSpinImpulse = new Transition()
                { From = _spinningVerticalImpulse, To = _spinningVerticalImpulse, ID = ToSpinningVerticalImpulseID };
            _spinningVerticalImpulse.AddTransition(spinImpulseToSpinImpulse);

            Transition spinImpulseToChase = new Transition()
                { From = _spinningVerticalImpulse, To = chase, ID = ToChaseID };
            _spinningVerticalImpulse.AddTransition(spinImpulseToChase);
            _states.Add(_spinningVerticalImpulse);

            Transition spinImpulseToDeath = new Transition()
                { From = _spinningHorizontalImpulse, To = death, ID = ToDeathID };
            _spinningHorizontalImpulse.AddTransition(spinImpulseToDeath);
            _states.Add(_spinningHorizontalImpulse);


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

        private void TransitionToSpinningVerticalImpulse()
        {
            _fsm.TryTransitionTo(ToSpinningVerticalImpulseID);
        }

        private void TransitionToSpinningHorizontalImpulse()
        {
            _fsm.TryTransitionTo(ToSpinningHorizontalImpulseID);
        }

        private void TransitionToDeath()
        {
            onDeath?.Invoke();
            _fsm.TryTransitionTo(ToDeathID);
        }

        private void TransitionToDeathImpulse(DamageInfo damageInfo)
        {
            if (damageInfo.DamageName == "PlayerSpinLastAttack")
            {
                _spinningHorizontalImpulse.SetImpulse(damageInfo.Knockback);
                _spinningHorizontalImpulse.SetImpulseSource(damageInfo.DamageOrigin);
                
                _fsm.ForceTransition(_spinningHorizontalImpulse);
            }
            else
            {
                _deathImpulse.SetImpulse(damageInfo.Knockback);
                _deathImpulse.SetImpulseSource(damageInfo.DamageOrigin);
                _deathImpulse.SetImpulseDuration(damageInfo.StunDuration);

                onDeath?.Invoke();
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

        public void OnBeingAttacked(DamageInfo damageInfo)
        {
            if (healthController.GetCurrentHealth() > 0)
            {
                if (damageInfo.DamageName == "PlayerVerticalAttack")
                {
                    _spinningVerticalImpulse.SetImpulse(damageInfo.Knockback);
                    _spinningVerticalImpulse.SetImpulseSource(damageInfo.DamageOrigin);
                    TransitionToSpinningVerticalImpulse();
                }
                else if (damageInfo.DamageName == "PlayerSpinLastAttack")
                {
                    Debug.Log("I entered here lol");
                    
                }
                else
                {
                    impulse.SetImpulse(damageInfo.Knockback);
                    impulse.SetImpulseSource(damageInfo.DamageOrigin);
                    impulse.SetImpulseDuration(damageInfo.StunDuration);
                    TransitionToImpulse();
                }
            }
        }
    }
}
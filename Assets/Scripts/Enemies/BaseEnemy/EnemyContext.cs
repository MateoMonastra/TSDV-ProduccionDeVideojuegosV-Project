using UnityEngine;
using UnityEngine.AI;

namespace Enemies.BaseEnemy
{
    /// <summary>
    /// Contexto compartido por todos los estados del enemigo.
    /// </summary>
    public sealed class EnemyContext
    {
        public Transform Self { get; }
        public Transform Target { get; }
        public BaseEnemyModel Model { get; }
        public NavMeshAgent Agent { get; }
        public Rigidbody Rb { get; }
        public Collider HitBox { get; }
        public EnemyAnimationController Anims { get; }
        
        public float SqrInnerRadius => Model.InnerRadius * Model.InnerRadius;
        public float SqrOuterRadius => Model.OuterRadius * Model.OuterRadius;
        public float SqrAttackRange => Model.AttackRange * Model.AttackRange;

        public EnemyContext(
            Transform self, Transform target, BaseEnemyModel model,
            NavMeshAgent agent, Rigidbody rb, Collider hitBox, EnemyAnimationController anims)
        {
            Self   = self;
            Target = target;
            Model  = model;
            Agent  = agent;
            Rb     = rb;
            HitBox = hitBox;
            Anims  = anims;
        }
    }
}
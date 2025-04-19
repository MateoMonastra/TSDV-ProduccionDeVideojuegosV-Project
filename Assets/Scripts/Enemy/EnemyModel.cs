using UnityEngine;

namespace Enemy
{
    [CreateAssetMenu(fileName = "EnemyModel", menuName = "Models/Enemy")]
    public class EnemyModel : ScriptableObject
    {
        [SerializeField] private float innerRadius;
        [SerializeField] private float outerRadius;
        [SerializeField] private float attackRange;
        [SerializeField] private float attackDuration;
        
        public float InnerRadius { get => innerRadius; set => innerRadius = value; }
        public float OuterRadius { get => outerRadius; set => outerRadius = value; }
        public float AttackRange{ get => attackRange; set => attackRange = value; }
        public float AttackDuration{ get => attackDuration; set => attackDuration = value; }
    }
}
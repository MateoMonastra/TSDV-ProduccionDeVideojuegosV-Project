using TMPro;
using UnityEngine;

namespace Enemies
{
    public class EnemyStateIndicator : MonoBehaviour
    {
        [Header("UI Reference")]
        [SerializeField] private TextMeshProUGUI currentStateText;

        [Header("Labels")]
        [SerializeField] private string onAttack  = "Attack";
        [SerializeField] private string onIdle    = "Idle";
        [SerializeField] private string onSpecial = "SpecialAttack";
        [SerializeField] private string onDamaged = "Damaged";
        [SerializeField] private string onChase   = "Chase";
        [SerializeField] private string onImpulse = "Impulse";
        [SerializeField] private string onDeath   = "Death";

        [Header("Colors")]
        [SerializeField] private Color attackColor  = Color.red;
        [SerializeField] private Color idleColor    = Color.white;
        [SerializeField] private Color specialColor = Color.magenta;
        [SerializeField] private Color damagedColor = Color.yellow;
        [SerializeField] private Color chaseColor   = Color.green;
        [SerializeField] private Color impulseColor = new Color(1f, 0.5f, 0f);
        [SerializeField] private Color deathColor   = Color.black;

        private void Awake()
        {
            if (!currentStateText) currentStateText = GetComponentInChildren<TextMeshProUGUI>();
        }

        private void OnEnable()
        {
            SetIdleState();
        }
        
        public void SetAttackState()  => SetState(onAttack,  attackColor);
        public void SetIdleState()    => SetState(onIdle,    idleColor);
        public void SetSpecialState() => SetState(onSpecial, specialColor);
        public void SetDamageState()  => SetState(onDamaged, damagedColor);
        public void SetChaseState()   => SetState(onChase,   chaseColor);
        public void SetImpulseState() => SetState(onImpulse, impulseColor);
        public void SetDeathState()   => SetState(onDeath,   deathColor);
        
        public void SetState(string label, Color color)
        {
            if (!currentStateText) return;
            currentStateText.text  = label;
            currentStateText.color = color;
        }
    }
}

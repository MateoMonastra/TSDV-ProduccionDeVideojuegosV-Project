using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Enemies
{
    public class EnemyStateIndicator : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI currentStateText;
        
        private string _onAttack = "Attack";
        private string _onIdle = "Idle";
        private string _onSpecialAttack = "SpecialAttack";
        private string _onChase = "Chase";
        private string _onDeath = "Death";

        private void OnEnable()
        {
            SetIdleState();
        }

        public void SetAttackState()
        {
            currentStateText.text = _onAttack;
            currentStateText.color = Color.red;
        }

        public void SetIdleState()
        {
            currentStateText.text = _onIdle;
            currentStateText.color = Color.white;
        }

        public void SetSpecialState()
        {
            currentStateText.text = _onSpecialAttack;
            currentStateText.color = Color.magenta;
        }

        public void SetChaseState()
        {
            currentStateText.text = _onChase;
            currentStateText.color = Color.green;
        }
        
        public void SetDeathState()
        {
            currentStateText.text = _onDeath;
            currentStateText.color = Color.black;
        }
    }
}
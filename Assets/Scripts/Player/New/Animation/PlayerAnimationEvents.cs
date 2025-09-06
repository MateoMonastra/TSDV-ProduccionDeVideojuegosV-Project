using UnityEngine;
using UnityEngine.Serialization;

namespace Player.New.Animation
{
    public class PlayerAnimationEvents : MonoBehaviour
    {
        [SerializeField] private PlayerAnimationController anim;
        public void OnDeathFinished()
        {
            anim.OnAnimEvent_DeathFinished();
            anim.TriggerLand();
        }
    }
}

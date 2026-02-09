
using Unity.VisualScripting;
using UnityEngine;

namespace Player.New.Audio
{
    public class PlayerAudioController : MonoBehaviour
    {
        [SerializeField]
        private AK.Wwise.Event _akJumpAudio;
        [SerializeField]
        private AK.Wwise.Event _akSuperJumpAudio;
        [SerializeField]
        private AK.Wwise.Event _akJump2Audio;
        [SerializeField]
        private AK.Wwise.Event _akPlayerDash;
        [SerializeField]
        private AK.Wwise.Event _akPlayerSuperDash;
        [SerializeField]
        private AK.Wwise.Event _akPlayerGetsHit;
        [SerializeField]
        private AK.Wwise.Event _akPlayerAttack1;
        [SerializeField]
        private AK.Wwise.Event _akPlayerAttack2;
        [SerializeField]
        private AK.Wwise.Event _akPlayerAttack3;
        [SerializeField]
        private AK.Wwise.Event _akPlayPlayerAttackSmash;
        [SerializeField]
        private AK.Wwise.Event _akPlayerChargeStart;
        [SerializeField]
        private AK.Wwise.Event _akPlayerChargeStopFail;
        [SerializeField]
        private AK.Wwise.Event _akPlayerChargeAttackStart;
        [SerializeField]
        private AK.Wwise.Event _akPlayerChargeAttackStop;
        [SerializeField]
        private AK.Wwise.Event _akPlayerAttackSmashHitFloor;
        [SerializeField]
        private AK.Wwise.Event _akPlayerSpinCharge1;
        [SerializeField]
        private AK.Wwise.Event _akPlayerSpinCharge2;
        [SerializeField]
        private AK.Wwise.Event _akPlayerSpinCharge3;


        public void PlayJumpAudio()
        {
            //Debug.Log("PlayJumpAudio");
            _akJumpAudio.Post(this.gameObject);
        }
        public void PlaySuperJumpAudio()
        {
            //Debug.Log("PlaySuperJumpAudio");
            _akSuperJumpAudio.Post(this.gameObject);
        }

        public void PlayJump2Audio()
        {
            //Debug.Log("PlayJump2Audio");
            _akJump2Audio.Post(this.gameObject);
        }

        public void PlayDashAudio()
        {
            //Debug.Log("PlayDashAudio");
            _akPlayerDash.Post(this.gameObject);
        }
        public void PlaySuperDashAudio()
        {
            //Debug.Log("PlaySuperDashAudio");
            _akPlayerSuperDash.Post(this.gameObject);
        }

        public void PlayPlayerGetsHitAudio()
        {
            //Debug.Log("PlayPlayerGetsHitAudio");
            _akPlayerGetsHit.Post(this.gameObject);
        }

        public void PlayPlayerAttack1()
        {
            //Debug.Log("PlayPlayerAttack1");
            _akPlayerAttack1.Post(this.gameObject);
        }

        public void PlayPlayerAttack2()
        {
            //Debug.Log("PlayPlayerAttack2");
            _akPlayerAttack2.Post(this.gameObject);
        }

        public void PlayPlayerAttack3()
        {
            //Debug.Log("PlayPlayerAttack3");
            _akPlayerAttack3.Post(this.gameObject);
        }

        public void PlayPlayerAttackSmash()
        {
            //Debug.Log("PlayPlayerAttackSmash");
            _akPlayPlayerAttackSmash.Post(this.gameObject);
        }

        public void PlayPlayerChargeStart()
        {
            //Debug.Log("PlayerChargeStart");
            _akPlayerChargeStart.Post(this.gameObject);
        }

        public void PlayPlayerChargeStopFail()
        {
            //Debug.Log("PlayerChargeStopFail");
            _akPlayerChargeStopFail.Post(this.gameObject);
        }
        public void PlayPlayerChargeAttackStart()
        {
            //Debug.Log("PlayerChargeAttackStart");
            _akPlayerChargeAttackStart.Post(this.gameObject);
        }
        public void PlayPlayerChargeAttackStop()
        {
            //Debug.Log("PlayerChargeAttackStop");
            _akPlayerChargeAttackStop.Post(this.gameObject);
        }

        public void PlayPlayerAttackSmashHitFloor()
        {
            //Debug.Log("PlayPlayerAttackSmashHitFloor");
            _akPlayerAttackSmashHitFloor.Post(this.gameObject);

        }
        public void PlayPlayerSpinCharge1()
        {
            //Debug.Log("PlayPlayerSpinCharge1");
            _akPlayerSpinCharge1.Post(this.gameObject);

        }
        public void PlayPlayerSpinCharge2()
        {
            //Debug.Log("PlayPlayerSpinCharge2");
            _akPlayerSpinCharge2.Post(this.gameObject);

        }
        public void PlayPlayerSpinCharge3()
        {
            //Debug.Log("PlayPlayerSpinCharge3");
            _akPlayerSpinCharge3.Post(this.gameObject);

        }

    }
}
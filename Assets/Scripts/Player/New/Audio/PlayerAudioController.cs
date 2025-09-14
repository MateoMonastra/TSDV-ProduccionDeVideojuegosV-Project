
using UnityEngine;

namespace Player.New.Audio
{
    public class PlayerAudioController : MonoBehaviour
    {
        [SerializeField]
        private AK.Wwise.Event _akJumpAudio; 
        [SerializeField]
        private AK.Wwise.Event _akJump2Audio; 
        [SerializeField]
        private AK.Wwise.Event _akPlayerDash; 
        [SerializeField]
        private AK.Wwise.Event _akPlayerGetsHit; 

        public void PlayJumpAudio()
        {
            //Debug.Log("PlayJumpAudio");
            _akJumpAudio.Post(this.gameObject);
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

        public void PlayPlayerGetsHitAudio()
        {
            //Debug.Log("PlayPlayerGetsHitAudio");
            _akPlayerGetsHit.Post(this.gameObject);
        }
    }
}
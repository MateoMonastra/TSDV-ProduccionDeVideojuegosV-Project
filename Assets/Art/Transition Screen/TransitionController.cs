using UnityEngine;
using UnityEngine.UI;

namespace Art.Transition_Screen
{
    public class TransitionController : MonoBehaviour
    {
        private static readonly int IsPlay = Animator.StringToHash("Dead");
        private static readonly int IsRevive = Animator.StringToHash("Revive");
        
        private Animator _animator;
        private Image _image;
        private readonly int _cicleSizeId = Shader.PropertyToID("_CircleSize");

        public float circleSize = 0;

        void Awake()
        {
            _animator = GetComponent<Animator>();
            _image = GetComponent<Image>();
        }

        private void OnEnable()
        {
            GameEvents.GameEvents.OnPlayerDied += PlayDead;
            GameEvents.GameEvents.OnPlayerRevived += PlayRevive;
        }

        private void OnDisable()
        {
            GameEvents.GameEvents.OnPlayerDied  -= PlayDead;
            GameEvents.GameEvents.OnPlayerRevived  -= PlayRevive;
        }

        void Update()
        {
            if (_image != null && _image.materialForRendering != null)
            {
                _image.materialForRendering.SetFloat(_cicleSizeId, circleSize);
            }
        }

        private void PlayDead()
        {
            if (_animator == null)
                return;
        
            _animator.SetTrigger(IsPlay);
        }
        
        public void PlayRevive()
        {
            if (_animator == null)
                return;
        
            _animator.SetTrigger(IsRevive);
        }
    }
}

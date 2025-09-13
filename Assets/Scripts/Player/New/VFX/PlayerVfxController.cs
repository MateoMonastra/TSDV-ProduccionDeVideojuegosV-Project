using System.Collections.Generic;
using UnityEngine;

namespace Player.New.VFX
{
    public class PlayerVfxController : MonoBehaviour
    {
        [SerializeField] private GameObject onBaseAttackParticles;
        [SerializeField] private GameObject onHitParticles;
        [SerializeField] private GameObject onJumpParticles;
        [SerializeField] private GameObject onDashParticles;
        [SerializeField] private GameObject onSpinAttackParticles;
        [SerializeField] private GameObject onVerticalAttackLandParticles;

        private ParticleSystem[] _onBaseAttackParticlesArray;
        private ParticleSystem[] _onHitParticlesArray;
        private ParticleSystem[] _onJumpParticlesArray;
        private ParticleSystem[] _onDashParticlesArray;
        private ParticleSystem[] _onSpinAttackParticlesArray;
        private ParticleSystem[] _onVerticalAttackLandParticlesArray;

        private void OnEnable()
        {
            // if (onBaseAttackParticles)
            //     _onBaseAttackParticlesArray = onBaseAttackParticles?.GetComponentsInChildren<ParticleSystem>();
            _onHitParticlesArray = onHitParticles?.GetComponentsInChildren<ParticleSystem>();
            _onJumpParticlesArray = onJumpParticles?.GetComponentsInChildren<ParticleSystem>();
            _onDashParticlesArray = onDashParticles?.GetComponentsInChildren<ParticleSystem>();
            _onSpinAttackParticlesArray = onSpinAttackParticles?.GetComponentsInChildren<ParticleSystem>();
            _onVerticalAttackLandParticlesArray =
                onVerticalAttackLandParticles?.GetComponentsInChildren<ParticleSystem>();
        }

        private void OnDisable()
        {
            _onBaseAttackParticlesArray =  null;
            _onHitParticlesArray =  null;
            _onJumpParticlesArray =  null;
            _onDashParticlesArray =  null;
            _onSpinAttackParticlesArray  =  null;
            _onVerticalAttackLandParticlesArray = null;
        }

        // public void OnBaseAttack()
        // {
        //     foreach (var particle in _onBaseAttackParticlesArray)
        //     {
        //         particle?.Play();
        //     }
        // }

        public void OnHit()
        {
            foreach (var particle in _onHitParticlesArray)
            {
                particle?.Play();
            }
        }

        public void OnJump()
        {
            foreach (var particle in _onJumpParticlesArray)
            {
                particle?.Play();
            }
        }

        public void OnDash()
        {
            foreach (var particle in _onDashParticlesArray)
            {
                particle?.Play();
            }
        }

        public void OnSpinAttack()
        {
            foreach (var particle in _onSpinAttackParticlesArray)
            {
                particle?.Play();
            }
        }

        public void OnVerticalAttackLand()
        {
            foreach (var particle in _onVerticalAttackLandParticlesArray)
            {
                particle?.Play();
            }
        }
    }
}
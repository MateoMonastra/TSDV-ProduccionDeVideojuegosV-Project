using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace Art.VFX.Script_VFX
{
    public class DissolvingController : MonoBehaviour
    {
        [SerializeField] private SkinnedMeshRenderer skinnedMesh;
        [SerializeField] private float dissolveRate = 0.0125f;
        [SerializeField] private float refreshRate = 0.025f;
        [SerializeField] private Material[] origMaterials;
        [SerializeField] private Material[] dissolvingMaterials;

        private Coroutine _dissolveCoroutine;

        private Material[] _skinnedMaterials;

        private void Start()
        {
            if (skinnedMesh != null)
                _skinnedMaterials = skinnedMesh.sharedMaterials;
        }

        private void OnDisable()
        {
            if (_dissolveCoroutine != null)
                StopCoroutine(_dissolveCoroutine);
        }


        public void StartDissolve()
        {
            skinnedMesh.materials = dissolvingMaterials;

            _skinnedMaterials = skinnedMesh.sharedMaterials;
            _dissolveCoroutine = StartCoroutine(Dissolveco());
        }


        private IEnumerator Dissolveco()
        {
            if (_skinnedMaterials.Length > 0)
            {
                float counter = 0;

                while (_skinnedMaterials[0].GetFloat("_DissolveAmonunt") < 1)
                {
                    counter += dissolveRate;
                    for (int i = 0; i < _skinnedMaterials.Length; i++)
                    {
                        _skinnedMaterials[i].SetFloat("_DissolveAmonunt", counter);
                    }

                    yield return new WaitForSeconds(refreshRate);
                }
            }
        }

        public void Reset()
        {
            if (_dissolveCoroutine != null)
                StopCoroutine(_dissolveCoroutine);

            foreach (var material in _skinnedMaterials)
            {
                material.SetFloat("_DissolveAmonunt", 0f);
            }

            skinnedMesh.materials = origMaterials;
            _skinnedMaterials = skinnedMesh.sharedMaterials;
        }
    }
}
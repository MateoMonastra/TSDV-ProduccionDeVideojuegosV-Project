using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Player.New.VFX
{
    public enum VfxEvent
    {
        BaseAttack,
        Hit,
        Jump,
        Dash,
        SpinAttack,
        VerticalAttackLand,
        Run
    }

    [Serializable]
    public class VfxGroup
    {
        [SerializeField] internal VfxEvent eventKey;
        [SerializeField] internal GameObject root;

        internal ParticleSystem[] CachedSystems;
    }

    public class PlayerVfxController : MonoBehaviour
    {
        [SerializeField] private List<VfxGroup> groups = new();
        
        private readonly Dictionary<VfxEvent, List<ParticleSystem[]>> _map = new();

        private void OnEnable()
        {
            BuildMap();
        }

        private void OnDisable()
        {
            _map.Clear();
        }

        private void BuildMap()
        {
            _map.Clear();

            foreach (var vfxGroup in groups)
            {
                if (vfxGroup == null || vfxGroup.root == null) continue;

                vfxGroup.CachedSystems = vfxGroup.root.GetComponentsInChildren<ParticleSystem>(true);

                if (!_map.TryGetValue(vfxGroup.eventKey, out var list))
                {
                    list = new List<ParticleSystem[]>();
                    _map[vfxGroup.eventKey] = list;
                }

                list.Add(vfxGroup.CachedSystems ?? Array.Empty<ParticleSystem>());
            }
        }
        
        public void Play(VfxEvent key)
        {
            if (!_map.TryGetValue(key, out var variants) || variants == null) return;

            foreach (var systems in variants)
            {
                if (systems == null) continue;
                
                var any = systems.Length > 0 ? systems[0] : null;
                if (any && !any.gameObject.scene.IsValid()) continue;

                foreach (var system in systems)
                {
                    if (!system) continue;

                    if (!system.gameObject.activeInHierarchy)
                        system.gameObject.SetActive(true);

                    system.Clear(true);
                    system.Play(true);
                }
            }
        }

        /// <summary>Reproduce solo una variante por índice.</summary>
        public void Play(VfxEvent key, int variantIndex)
        {
            if (!_map.TryGetValue(key, out var variants) || variants == null) return;
            if (variantIndex < 0 || variantIndex >= variants.Count) return;

            var systems = variants[variantIndex];
            if (systems == null) return;

            var any = systems.Length > 0 ? systems[0] : null;
            if (any != null && !any.gameObject.scene.IsValid()) return;

            foreach (var system in systems)
            {
                if (system == null) continue;

                if (!system.gameObject.activeInHierarchy)
                    system.gameObject.SetActive(true);

                system.Clear(true);
                system.Play(true);
            }
        }

        // Reproduce los grupos del evento en una posición/rotación opcionales.
        public void PlayAt(VfxEvent key, Vector3? position = null, Quaternion? rotation = null)
        {
            if (!_map.TryGetValue(key, out var variants) || variants == null) return;

            foreach (var systems in variants)
            {
                if (systems == null) continue;

                foreach (var system in systems)
                {
                    if (!system) continue;

                    var t = system.transform;
                    var targetPos = position ?? t.position;
                    var targetRot = rotation ?? t.rotation;
                    t.SetPositionAndRotation(targetPos, targetRot);

                    if (!system.gameObject.activeInHierarchy)
                        system.gameObject.SetActive(true);

                    system.Clear(true);
                    system.Play(true);
                }
            }
        }

        // Reproduce una variante con posición/rotación opcionales.
        public void PlayAt(VfxEvent key, int variantIndex, Vector3? position = null, Quaternion? rotation = null)
        {
            if (!_map.TryGetValue(key, out var variants) || variants == null) return;
            if (variantIndex < 0 || variantIndex >= variants.Count) return;

            var systems = variants[variantIndex];
            if (systems == null) return;

            foreach (var system in systems)
            {
                if (!system) continue;

                var t = system.transform;
                var targetPos = position ?? t.position;
                var targetRot = rotation ?? t.rotation;
                t.SetPositionAndRotation(targetPos, targetRot);

                if (!system.gameObject.activeInHierarchy)
                    system.gameObject.SetActive(true);

                system.Clear(true);
                system.Play(true);
            }
        }


        /// <summary>Detiene todos los PS de TODOS los grupos de un evento.</summary>
        public void Stop(VfxEvent key, bool clear = false)
        {
            if (!_map.TryGetValue(key, out var variants) || variants == null) return;

            foreach (var systems in variants)
            {
                if (systems == null) continue;

                foreach (var system in systems)
                {
                    if (system == null) continue;

                    system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    if (clear) system.Clear(true);
                }
            }
        }

        /// <summary>Detiene solo una variante por índice.</summary>
        public void Stop(VfxEvent key, int variantIndex, bool clear = false)
        {
            if (!_map.TryGetValue(key, out var variants) || variants == null) return;
            if (variantIndex < 0 || variantIndex >= variants.Count) return;

            var systems = variants[variantIndex];
            if (systems == null) return;

            foreach (var system in systems)
            {
                if (system == null) continue;

                system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                if (clear) system.Clear(true);
            }
        }
    }
}

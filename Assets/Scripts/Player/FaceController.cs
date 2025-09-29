using System;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    public class FaceController : MonoBehaviour
    {
        [SerializeField] private List<GameObject> eyes = new();
        [SerializeField] private List<GameObject> mouth = new();

        private readonly Dictionary<string, int> _eyesIdx = new Dictionary<string, int>();
        private readonly Dictionary<string, int> _mouthIdx  = new Dictionary<string, int>();

        private void Start()
        {
            Rebuild();
        }
        private void Rebuild()
        {
            _eyesIdx.Clear();
            for (int i = 0; i < eyes.Count; i++) if (eyes[i]) _eyesIdx[eyes[i].name] = i;

            _mouthIdx.Clear();
            for (int i = 0; i < mouth.Count; i++) if (mouth[i]) _mouthIdx[mouth[i].name] = i;
        }

        public void SetEyes(string name)
        {
            if (!_eyesIdx.TryGetValue(name, out var i)) return;
            ActivateExclusive(eyes, i);
        }

        public void SetEyesIndex(int index) => ActivateExclusive(eyes, index);

        public void SetMouth(string name)
        {
            if (!_mouthIdx.TryGetValue(name, out var i)) return;
            ActivateExclusive(mouth, i);
        }

        public void SetMouthIndex(int index) => ActivateExclusive(mouth, index);

        private static void ActivateExclusive(List<GameObject> list, int index)
        {
            if (list == null || list.Count == 0 || index < 0 || index >= list.Count) return;
            for (int i = 0; i < list.Count; i++) if (list[i]) list[i].SetActive(i == index);
        }
    }
}
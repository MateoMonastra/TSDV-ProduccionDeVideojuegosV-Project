using TMPro;
using UnityEngine;

namespace Collectables
{
    public class PauseStampsView : MonoBehaviour
    {
        [SerializeField] private StampSlot[] slots;
        [SerializeField] private TextMeshProUGUI collectedText;

        private void OnEnable()
        {
            Refresh();

            if (StampCollection.Instance != null)
                StampCollection.Instance.OnStampUnlocked += OnStampUnlocked;
        }

        private void OnDisable()
        {
            if (StampCollection.Instance != null)
                StampCollection.Instance.OnStampUnlocked -= OnStampUnlocked;
        }

        private void OnStampUnlocked(string stampId)
        {
            RefreshOne(stampId);
        }

        public void Refresh()
        {
            if (StampCollection.Instance == null) return;

            foreach (var s in slots)
            {
                bool unlocked = StampCollection.Instance.IsUnlocked(s.stampId);
                s.SetUnlocked(unlocked);
            }
            
            RefreshCollectedText();
        }

        private void RefreshOne(string stampId)
        {
            if (StampCollection.Instance == null) return;

            foreach (var s in slots)
            {
                if (s.stampId == stampId)
                {
                    s.SetUnlocked(true);
                    return;
                }
            }
            
            RefreshCollectedText();
        }
        
        private void RefreshCollectedText()
        {
            if (collectedText == null || StampCollection.Instance == null) return;

            int total = slots.Length;
            int collected = 0;

            foreach (var s in slots)
            {
                if (StampCollection.Instance.IsUnlocked(s.stampId))
                    collected++;
            }

            collectedText.text = $"Collected: {collected}/{total}";
        }
    }
}
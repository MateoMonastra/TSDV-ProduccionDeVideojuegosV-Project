using UnityEngine;
using UnityEngine.UI;

namespace Collectables
{
    [System.Serializable]
    public class StampSlot
    {
        public string stampId;
        public Image icon;
        public Color lockedColor = new Color(1,1,1,0.25f);
        public Color unlockedColor = new Color(1,1,1,1f);

        public void SetUnlocked(bool unlocked)
        {
            if (icon == null) return;
            icon.color = unlocked ? unlockedColor : lockedColor;
        }
    }
}
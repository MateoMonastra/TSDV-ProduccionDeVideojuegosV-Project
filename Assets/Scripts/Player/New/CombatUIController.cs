using UnityEngine;
using UnityEngine.UI;

namespace Player.New.UI
{
    public class CombatUIController : MonoBehaviour
    {
        [Header("Refs")]
        public PlayerModel model;             // arrastra tu ScriptableObject
        [Tooltip("HUD del jugador (Canvas). Dejar vacío si este script ya está en el Canvas.")]
        public Canvas rootCanvas;

        [Header("Spin - Carga")]
        public Image spinChargeFill;          // Image (Filled Radial 360)
        public Image spinChargeMinMark;       // (opcional) una imagen/aro fino para marcar el mínimo
        public Color spinBelowMin = new Color(1f, 0.6f, 0.2f, 1f);
        public Color spinAboveMin = new Color(0.2f, 1f, 0.4f, 1f);

        [Header("Cooldowns")]
        public Image spinCdFill;              // Image (Filled Radial 360)
        public Text  spinCdText;              // Text (u opcional TMP_Text)
        public Image dashCdFill;
        public Text  dashCdText;
        public Image vertCdFill;
        public Text  vertCdText;

        [Header("Opciones")]
        [Tooltip("Cuando queda menos que este tiempo, el color del fill se pone pleno.")]
        public float readyBlinkThreshold = 0.15f;

        void Awake()
        {
            if (!rootCanvas) rootCanvas = GetComponentInParent<Canvas>();
            HideSpinChargeUI();
        }

        void Update()
        {
            if (!model) return;

            // SPIN (cooldown)
            UpdateCooldown(spinCdFill, spinCdText, model.SpinOnCooldown ? model.SpinCooldownLeft : 0f, model.SpinCooldown);

            // DASH (cooldown)
            UpdateCooldown(dashCdFill, dashCdText, model.DashOnCooldown ? model.DashCooldownLeft : 0f, model.DashCooldown);

            // VERTICAL (cooldown)
            UpdateCooldown(vertCdFill, vertCdText, model.VerticalOnCooldown ? model.VerticalCooldownLeft : 0f, model.VerticalAttackCooldown);
        }

        private void UpdateCooldown(Image fill, Text txt, float left, float total)
        {
            if (!fill && !txt) return;
            bool onCd = left > 0.0001f && total > 0.0001f;
            float ratio = onCd ? Mathf.Clamp01(1f - (left / total)) : 1f;

            if (fill)
            {
                fill.enabled = true;
                fill.fillAmount = ratio;
                // si está listo, lo vemos lleno
                if (!onCd || left <= readyBlinkThreshold)
                    fill.color = Color.white;
            }

            if (txt)
            {
                txt.text = onCd ? left.ToString("0.0") : "";
            }
        }

        // ============ API pública para SpinCharge ============

        /// <summary>Actualiza la barra de carga del Spin. t = tiempo acumulado, min/max = umbrales.</summary>
        public void OnSpinChargeProgress(float t, float min, float max)
        {
            if (!spinChargeFill) return;

            spinChargeFill.enabled = true;

            // progreso de 0..max (visual)
            float visual = Mathf.InverseLerp(0f, Mathf.Max(0.0001f, max), t);
            spinChargeFill.fillAmount = visual;

            // color según si superó el mínimo
            bool aboveMin = t >= min;
            spinChargeFill.color = aboveMin ? spinAboveMin : spinBelowMin;

            // marcar arco mínimo (si hay)
            if (spinChargeMinMark)
            {
                spinChargeMinMark.enabled = true;
                spinChargeMinMark.fillAmount = Mathf.Clamp01(min / Mathf.Max(0.0001f, max));
            }
        }

        /// <summary>Oculta la UI de carga (al cancelar o al soltar).</summary>
        public void OnSpinChargeEnd() => HideSpinChargeUI();

        private void HideSpinChargeUI()
        {
            if (spinChargeFill) { spinChargeFill.enabled = false; spinChargeFill.fillAmount = 0f; }
            if (spinChargeMinMark) spinChargeMinMark.enabled = false;
        }

        /// <summary>Recibe el tiempo de cooldown restante del Spin (ya lo triggea SpinRelease).</summary>
        public void OnSpinCooldown(float cdLeft)
        {
            // opcional: podrías forzar un flash/anim acá cuando cdLeft pasa de >0 a 0
            // (dejamos vacío porque el Update() ya renderiza el CD cada frame a partir del model)
        }
    }
}

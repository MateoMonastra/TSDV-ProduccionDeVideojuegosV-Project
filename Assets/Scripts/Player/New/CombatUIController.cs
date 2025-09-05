using UnityEngine;
using UnityEngine.UI;

namespace Player.New.UI
{
    /// <summary>
    /// HUD de combate: cooldowns, carga de spin y estados de pickups (Extra Jump / Dash Buff).
    /// </summary>
    public class CombatUIController : MonoBehaviour
    {
        [Header("Refs")]
        [Tooltip("Modelo del jugador (ScriptableObject o instancia).")]
        public PlayerModel model;             // arrastra tu Model
        [Tooltip("HUD del jugador (Canvas). Dejar vacío si este script ya está en el Canvas.")]
        public Canvas rootCanvas;

        // ─────────────────────────────────────────────────────────────────────
        // Spin - Carga
        // ─────────────────────────────────────────────────────────────────────
        [Header("Spin - Carga")]
        public Image spinChargeFill;          // Image (Filled Radial 360)
        public Image spinChargeMinMark;       // (opcional) aro mínimo
        public Color spinBelowMin = new Color(1f, 0.6f, 0.2f, 1f);
        public Color spinAboveMin = new Color(0.2f, 1f, 0.4f, 1f);

        // ─────────────────────────────────────────────────────────────────────
        // Cooldowns
        // ─────────────────────────────────────────────────────────────────────
        [Header("Cooldowns")]
        public Image spinCdFill;              // Image (Filled Radial 360)
        public Text  spinCdText;              // o TMP_Text si preferís
        public Image dashCdFill;
        public Text  dashCdText;
        public Image vertCdFill;
        public Text  vertCdText;

        [Header("Opciones")]
        [Tooltip("Cuando queda menos que este tiempo, el color del fill se pone pleno.")]
        public float readyBlinkThreshold = 0.15f;

        // ─────────────────────────────────────────────────────────────────────
        // Pickups (HUD)
        // ─────────────────────────────────────────────────────────────────────
        [Header("Pickups (HUD)")]
        [Tooltip("Ícono/Widget que indica que hay un salto extra pendiente.")]
        public Image extraJumpIcon;
        [Tooltip("Ícono/Widget que indica que el próximo dash tiene buff de distancia.")]
        public Image dashBuffIcon;

        [Tooltip("Si está activo, el ícono hace un pulso sutil mientras el pickup esté disponible.")]
        public bool pulseActivePickups = true;
        [Tooltip("Velocidad del pulso para íconos de pickups.")]
        public float pickupPulseSpeed = 4.5f;
        [Tooltip("Escala mínima del pulso (1 = sin pulso).")]
        [Range(0.6f, 1f)] public float pickupPulseMinScale = 0.9f;

        private float _tPulse; // acumulador para pulso

        void Awake()
        {
            if (!rootCanvas) rootCanvas = GetComponentInParent<Canvas>();
            HideSpinChargeUI();
            // Inicializar estado de pickups en HUD
            SetIconActive(extraJumpIcon, false);
            SetIconActive(dashBuffIcon, false);
        }

        void Update()
        {
            if (!model) return;

            // ========== COOLDOWNS ==========
            UpdateCooldown(spinCdFill, spinCdText, model.SpinOnCooldown ? model.SpinCooldownLeft : 0f, model.SpinCooldown);
            UpdateCooldown(dashCdFill, dashCdText, model.DashOnCooldown ? model.DashCooldownLeft : 0f, model.DashCooldown);
            UpdateCooldown(vertCdFill, vertCdText, model.VerticalOnCooldown ? model.VerticalCooldownLeft : 0f, model.VerticalAttackCooldown);

            // ========== PICKUPS (flags del Model) ==========
            UpdatePickupIcons(Time.deltaTime);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Cooldowns
        // ─────────────────────────────────────────────────────────────────────
        private void UpdateCooldown(Image fill, Text txt, float left, float total)
        {
            if (!fill && !txt) return;
            bool onCd = left > 0.0001f && total > 0.0001f;
            float ratio = onCd ? Mathf.Clamp01(1f - (left / total)) : 1f;

            if (fill)
            {
                fill.enabled = true;
                fill.fillAmount = ratio;
                if (!onCd || left <= readyBlinkThreshold)
                    fill.color = Color.white; // listo → color pleno
            }

            if (txt)
                txt.text = onCd ? left.ToString("0.0") : "";
        }

        // ─────────────────────────────────────────────────────────────────────
        // Spin Charge (API pública)
        // ─────────────────────────────────────────────────────────────────────
        public void OnSpinChargeProgress(float t, float min, float max)
        {
            if (!spinChargeFill) return;

            spinChargeFill.enabled = true;

            float visual = Mathf.InverseLerp(0f, Mathf.Max(0.0001f, max), t);
            spinChargeFill.fillAmount = visual;

            bool aboveMin = t >= min;
            spinChargeFill.color = aboveMin ? spinAboveMin : spinBelowMin;

            if (spinChargeMinMark)
            {
                spinChargeMinMark.enabled = true;
                spinChargeMinMark.fillAmount = Mathf.Clamp01(min / Mathf.Max(0.0001f, max));
            }
        }

        public void OnSpinChargeEnd() => HideSpinChargeUI();

        private void HideSpinChargeUI()
        {
            if (spinChargeFill) { spinChargeFill.enabled = false; spinChargeFill.fillAmount = 0f; }
            if (spinChargeMinMark) spinChargeMinMark.enabled = false;
        }

        public void OnSpinCooldown(float cdLeft)
        {
            // El Update ya refleja el cooldown; acá podrías hacer un flash si querés.
        }

        // ─────────────────────────────────────────────────────────────────────
        // Pickups (UI)
        // ─────────────────────────────────────────────────────────────────────
        private void UpdatePickupIcons(float dt)
        {
            bool hasExtra = model.HasExtraJump;
            bool hasDashB = model.DashBuffPending;

            SetIconActive(extraJumpIcon, hasExtra);
            SetIconActive(dashBuffIcon, hasDashB);

            if (!pulseActivePickups) return;

            // Pulso sutil de los íconos activos
            _tPulse += dt * pickupPulseSpeed;
            float s = Mathf.Lerp(pickupPulseMinScale, 1f, 0.5f * (1f + Mathf.Sin(_tPulse)));

            if (hasExtra && extraJumpIcon)
            {
                extraJumpIcon.transform.localScale = new Vector3(s, s, 1f);
            }
            else if (extraJumpIcon)
            {
                extraJumpIcon.transform.localScale = Vector3.one;
            }

            if (hasDashB && dashBuffIcon)
            {
                dashBuffIcon.transform.localScale = new Vector3(s, s, 1f);
            }
            else if (dashBuffIcon)
            {
                dashBuffIcon.transform.localScale = Vector3.one;
            }
        }

        private static void SetIconActive(Graphic g, bool active)
        {
            if (!g) return;
            if (g.canvasRenderer != null)
            {
                g.CrossFadeAlpha(active ? 1f : 0f, 0.1f, true);
            }
            g.enabled = active;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Hooks opcionales (llamarlos desde los pickups para un flash instantáneo)
        // ─────────────────────────────────────────────────────────────────────
        public void OnPickupExtraJumpGained()
        {
            if (extraJumpIcon)
            {
                extraJumpIcon.enabled = true;
                extraJumpIcon.canvasRenderer.SetAlpha(1f);
                extraJumpIcon.transform.localScale = Vector3.one;
            }
        }

        public void OnPickupDashBuffGained()
        {
            if (dashBuffIcon)
            {
                dashBuffIcon.enabled = true;
                dashBuffIcon.canvasRenderer.SetAlpha(1f);
                dashBuffIcon.transform.localScale = Vector3.one;
            }
        }
    }
}

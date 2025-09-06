using System.Collections;
using Health;
using UnityEngine;
using UnityEngine.UI;

namespace Player.New.UI
{
    /// <summary>
    /// HUD unificado (sin agregar GameEvents nuevos).
    /// - Spin: carga (fill + marca mínimo) y cooldown (polling del PlayerModel).
    /// - Pickups: extra jump / dash buff (polling del PlayerModel + métodos públicos opcionales).
    /// - Cooldowns: Spin / Dash / Vertical (polling del PlayerModel).
    /// - Vida, daño y ceguera (métodos públicos).
    /// </summary>
    public class HUDManager : MonoBehaviour
    {
        private static readonly int IsPlayerHealth = Animator.StringToHash("PlayerHealth");

        [Header("Modelo (para polling)")]
        [SerializeField] private Player.New.PlayerModel model;

        [Header("Spin - Carga")]
        [SerializeField] private Image spinChargeFill;    // Filled Radial360
        [SerializeField] private Image spinChargeMinMark; // Filled Radial360 (marca en % del mínimo)
        [SerializeField] private Color spinBelowMin = new Color(1f, 0.6f, 0.2f, 1f);
        [SerializeField] private Color spinAboveMin = new Color(0.2f, 1f, 0.4f, 1f);

        [Header("Cooldowns")]
        [SerializeField] private Image spinCdFill;
        [SerializeField] private Text  spinCdText;
        [SerializeField] private Image dashCdFill;
        [SerializeField] private Text  dashCdText;
        [SerializeField] private Image vertCdFill;
        [SerializeField] private Text  vertCdText;
        [SerializeField, Tooltip("Cuando queda menos de esto (s), el fill se presenta 'listo'.")]
        private float readyBlinkThreshold = 0.15f;

        [Header("Pickups (iconos)")]
        [SerializeField, Tooltip("Ícono: hay salto extra disponible (pickup activo o flag del model).")]
        private Image extraJumpIcon;
        [SerializeField, Tooltip("Ícono: el próximo dash está buffeado (pickup activo o flag del model).")]
        private Image dashBuffIcon;
        [SerializeField, Tooltip("Pulso suave mientras un pickup está activo.")]
        private bool pulseActivePickups = true;
        [SerializeField] private float pickupPulseSpeed = 4.5f;
        [SerializeField, Range(0.6f, 1f)] private float pickupPulseMinScale = 0.9f;

        [Header("Vida / Daño / Ceguera")]
        [SerializeField] private Animator playerHealthAnimator;
        [SerializeField, Tooltip("Nombre del parámetro entero del Animator para vida actual")]
        private string playerHealthParam = "PlayerHealth";
        [SerializeField, Tooltip("Overlay de daño (Image con alpha)")]
        private Image damagedImage;
        [SerializeField] private float damagedDuration = 0.3f;
        [SerializeField, Tooltip("Overlay/GO para 'ceguera' temporal")]
        private GameObject blindnessEffect;
        [SerializeField] private float blindnessDuration = 2f;
        
        private Coroutine _blindnessCo;
        private Coroutine _damagedCo;
        private float _pulseT;
        private bool _spinChargeVisible;

        private void Awake()
        {
            // Asegurar configuración correcta de imágenes filled para el spin
            EnsureFilledSetup(spinChargeFill);
            EnsureFilledSetup(spinChargeMinMark);

            HideSpinChargeUI();

            // Inicializar iconos de pickups
            SetGraphicEnabled(extraJumpIcon, false);
            SetGraphicEnabled(dashBuffIcon, false);
            ResetScale(extraJumpIcon);
            ResetScale(dashBuffIcon);
        }

        private void Update()
        {
            if (!model) return;

            // ===== Cooldowns por polling =====
            UpdateCooldown(spinCdFill, spinCdText,
                model.SpinOnCooldown ? model.SpinCooldownLeft : 0f, model.SpinCooldown);

            UpdateCooldown(dashCdFill, dashCdText,
                model.DashOnCooldown ? model.DashCooldownLeft : 0f, model.DashCooldown);

            UpdateCooldown(vertCdFill, vertCdText,
                model.VerticalOnCooldown ? model.VerticalCooldownLeft : 0f, model.VerticalAttackCooldown);

            // ===== Pickups por polling =====
            UpdatePickupIcons(Time.deltaTime, model.HasExtraJump, model.DashBuffPending);
        }

        // =============== API pública existente ===============

        /// <summary>
        /// Progreso de carga del spin (llamado mientras se mantiene el input).
        /// </summary>
        public void OnSpinChargeProgress(float current, float min, float max)
        {
            if (!spinChargeFill) return;

            _spinChargeVisible = true;
            spinChargeFill.enabled = true;

            // Asegurar tipo filled/radial
            EnsureFilledSetup(spinChargeFill);
            EnsureFilledSetup(spinChargeMinMark);

            float visual = Mathf.InverseLerp(0f, Mathf.Max(0.0001f, max), current);
            spinChargeFill.fillAmount = visual;

            bool aboveMin = current >= min;
            spinChargeFill.color = aboveMin ? spinAboveMin : spinBelowMin;

            if (spinChargeMinMark)
            {
                spinChargeMinMark.enabled = true;
                spinChargeMinMark.fillAmount = Mathf.Clamp01(min / Mathf.Max(0.0001f, max));
            }
        }

        /// <summary>
        /// Fin de la carga (al soltar o cancelar).
        /// </summary>
        public void OnSpinChargeEnd()
        {
            _spinChargeVisible = false;
            HideSpinChargeUI();
        }

        /// <summary>
        /// Llamada por tu lógica actual (ej. SpinRelease -> OnSpinCooldownUI) con tiempo restante.
        /// </summary>
        public void OnSpinCooldown(float cdLeft)
        {
            // No forzamos nada acá; el Update ya pinta desde el modelo.
            // Si quisieras un flash al quedar listo, podrías añadirlo aquí.
        }

        // =============== Pickups (métodos opcionales directos) ===============

        /// <summary>Marcar explícitamente el icono de extra jump activo/inactivo (si preferís no depender de polling).</summary>
        public void SetPickupExtraJumpActive(bool active)
        {
            SetGraphicEnabled(extraJumpIcon, active);
            if (!active) ResetScale(extraJumpIcon);
        }

        /// <summary>Marcar explícitamente el icono de dash buff activo/inactivo (si preferís no depender de polling).</summary>
        public void SetPickupDashBuffActive(bool active)
        {
            SetGraphicEnabled(dashBuffIcon, active);
            if (!active) ResetScale(dashBuffIcon);
        }

        // =============== Vida / Daño / Ceguera ===============

        public void SetHealth(int current)
        {
            if (playerHealthAnimator)
                playerHealthAnimator.SetInteger(IsPlayerHealth,current);
        }

        public void OnDamaged()
        {
            if (_damagedCo != null) StopCoroutine(_damagedCo);
            _damagedCo = StartCoroutine(DamagedCo());
        }

        public void OnBlind()
        {
            if (_blindnessCo != null) StopCoroutine(_blindnessCo);
            _blindnessCo = StartCoroutine(BlindCo());
        }

        // =============== Render helpers ===============

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
                    fill.color = Color.white;
            }

            if (txt)
                txt.text = onCd ? left.ToString("0.0") : "";
        }

        /// <summary>Polling + pulso para los iconos de pickups.</summary>
        private void UpdatePickupIcons(float dt, bool hasExtraJump, bool dashBuffPending)
        {
            SetGraphicEnabled(extraJumpIcon, hasExtraJump);
            SetGraphicEnabled(dashBuffIcon, dashBuffPending);

            if (!pulseActivePickups)
            {
                ResetScale(extraJumpIcon);
                ResetScale(dashBuffIcon);
                return;
            }

            _pulseT += dt * pickupPulseSpeed;
            float s = Mathf.Lerp(pickupPulseMinScale, 1f, 0.5f * (1f + Mathf.Sin(_pulseT)));

            if (hasExtraJump) SetScale(extraJumpIcon, s); else ResetScale(extraJumpIcon);
            if (dashBuffPending) SetScale(dashBuffIcon, s); else ResetScale(dashBuffIcon);
        }

        private void HideSpinChargeUI()
        {
            if (spinChargeFill) { spinChargeFill.enabled = false; spinChargeFill.fillAmount = 0f; }
            if (spinChargeMinMark) spinChargeMinMark.enabled = false;
        }

        private static void EnsureFilledSetup(Image img)
        {
            if (!img) return;
            if (img.type != Image.Type.Filled) img.type = Image.Type.Filled;
            // Forzamos radial 360 para evitar que en el prefab quede mal configurado
            if (img.fillMethod != Image.FillMethod.Radial360) img.fillMethod = Image.FillMethod.Radial360;
            if (img.fillOrigin != (int)Image.Origin360.Top) img.fillOrigin = (int)Image.Origin360.Top;
            img.fillClockwise = true;
        }

        private static void SetGraphicEnabled(Graphic g, bool enabled)
        {
            if (!g) return;
            g.enabled = enabled;
            var cr = g.canvasRenderer;
            if (cr != null) cr.SetAlpha(enabled ? 1f : 0f);
        }

        private static void SetScale(Graphic g, float s)
        {
            if (!g) return;
            if (g.transform is RectTransform rt) rt.localScale = new Vector3(s, s, 1f);
        }

        private static void ResetScale(Graphic g)
        {
            if (!g) return;
            if (g.transform is RectTransform rt) rt.localScale = Vector3.one;
        }

        private IEnumerator DamagedCo()
        {
            if (!damagedImage) yield break;

            damagedImage.gameObject.SetActive(true);
            var color = damagedImage.color;

            float half = Mathf.Max(0.01f, damagedDuration * 0.5f);

            // Fade-in
            float t = 0f;
            while (t < half)
            {
                t += Time.deltaTime;
                color.a = Mathf.Lerp(0f, 1f, t / half);
                damagedImage.color = color;
                yield return null;
            }

            // Fade-out
            t = 0f;
            while (t < half)
            {
                t += Time.deltaTime;
                color.a = Mathf.Lerp(1f, 0f, t / half);
                damagedImage.color = color;
                yield return null;
            }

            damagedImage.gameObject.SetActive(false);
        }

        private IEnumerator BlindCo()
        {
            if (!blindnessEffect) yield break;

            blindnessEffect.SetActive(true);
            yield return new WaitForSeconds(Mathf.Max(0f, blindnessDuration));
            blindnessEffect.SetActive(false);
        }
    }
}

using System;
using System.Collections;
using Health;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Player.New.UI
{
    /// <summary>
    /// - Spin: carga (fill + marca mínimo) y cooldown (polling del PlayerModel).
    /// - Pickups: extra jump / dash buff (polling del PlayerModel + métodos públicos opcionales).
    /// - Cooldowns: Spin / Dash / Vertical (polling del PlayerModel).
    /// - Vida, daño y ceguera (métodos públicos) + delega opcional a HeartsUIManager.
    /// </summary>
    public class HUDManager : MonoBehaviour
    {
        [Header("Modelo")]
        [SerializeField] private PlayerModel model;

        [Header("Spin - Carga")]
        [SerializeField] private Image spinChargeFill;
        [SerializeField] private Image spinChargeMinMark;
        [SerializeField] private Color spinBelowMin = new Color(1f, 0.6f, 0.2f, 1f);
        [SerializeField] private Color spinAboveMin = new Color(0.2f, 1f, 0.4f, 1f);

        [Header("Cooldowns")]
        [SerializeField] private Image spinCdFill;
        [SerializeField] private TextMeshProUGUI spinCdText; // TMP
        [SerializeField] private Image dashCdFill;
        [SerializeField] private TextMeshProUGUI dashCdText; // TMP
        [SerializeField] private Image vertCdFill;
        [SerializeField] private TextMeshProUGUI vertCdText; // TMP
        [SerializeField, Tooltip("Cuando queda menos de esto (s), se muestra listo (blink).")]
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
        [SerializeField, Tooltip("Nombre del parámetro entero del Animator para vida actual.")]
        private string playerHealthParam = "PlayerHealth";
        [SerializeField, Tooltip("Overlay de daño (Image con alpha).")]
        private Image damagedImage;
        [SerializeField] private float damagedDuration = 0.3f;
        [SerializeField, Tooltip("Overlay/GO para 'ceguera' temporal.")]
        private GameObject blindnessEffect;
        [SerializeField] private float blindnessDuration = 2f;

        [FormerlySerializedAs("hearts")]
        [Header("Corazones")]
        [SerializeField, Tooltip("Si se asigna, también actualiza el UI de corazones.")]
        private HeartsUIManager heartsController;

        // Caches / estado interno
        private Coroutine _blindnessCo;
        private Coroutine _damagedCo;
        private float _pulseT;
        private bool _spinChargeVisible;
        private int _playerHealthHash;
        private int _spinTenths, _dashTenths, _vertTenths;

        private void Awake()
        {
            _playerHealthHash = Animator.StringToHash(
                string.IsNullOrEmpty(playerHealthParam) ? "PlayerHealth" : playerHealthParam);

            // Setup radial para spin
            EnsureFilledSetup(spinChargeFill, Image.FillMethod.Radial360, (int)Image.Origin360.Top, true);
            EnsureFilledSetup(spinChargeMinMark, Image.FillMethod.Radial360, (int)Image.Origin360.Top, true);

            HideSpinChargeUI();

            SetGraphicEnabled(extraJumpIcon, false);
            SetGraphicEnabled(dashBuffIcon, false);
            
            ResetScale(extraJumpIcon);
            ResetScale(dashBuffIcon);
        }

        private void OnEnable()
        {
            GameEvents.GameEvents.OnPlayerBlinded += OnBlind;
        }

        private void OnDisable()
        {
            GameEvents.GameEvents.OnPlayerBlinded -= OnBlind;

            if (_blindnessCo != null) { StopCoroutine(_blindnessCo); _blindnessCo = null; }
            if (_damagedCo != null) { StopCoroutine(_damagedCo); _damagedCo = null; }

            if (blindnessEffect) blindnessEffect.SetActive(false);
            if (damagedImage) damagedImage.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (!model) return;

            UpdateCooldown(spinCdFill, spinCdText,
                model.SpinOnCooldown ? model.SpinCooldownLeft : 0f, model.SpinCooldown, ref _spinTenths);

            UpdateCooldown(dashCdFill, dashCdText,
                model.DashOnCooldown ? model.DashCooldownLeft : 0f, model.DashCooldown, ref _dashTenths);

            UpdateCooldown(vertCdFill, vertCdText,
                model.VerticalOnCooldown ? model.VerticalCooldownLeft : 0f, model.VerticalAttackCooldown, ref _vertTenths);

            UpdatePickupIcons(Time.deltaTime, model.HasExtraJump, model.DashBuffPending);
        }

        /// <summary>Progreso de carga del spin (llamado mientras se mantiene el input).</summary>
        public void OnSpinChargeProgress(float current, float min, float max)
        {
            if (!spinChargeFill) return;

            _spinChargeVisible = true;
            spinChargeFill.enabled = true;

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

        /// <summary>Fin de la carga (al soltar o cancelar).</summary>
        public void OnSpinChargeEnd()
        {
            _spinChargeVisible = false;
            HideSpinChargeUI();
        }

        /// <summary>Marcar explícitamente el icono de extra jump activo/inactivo (si no querés depender de polling).</summary>
        public void SetPickupExtraJumpActive(bool active)
        {
            SetGraphicEnabled(extraJumpIcon, active);
            if (!active) ResetScale(extraJumpIcon);
        }

        /// <summary>Marcar explícitamente el icono de dash buff activo/inactivo (si no querés depender de polling).</summary>
        public void SetPickupDashBuffActive(bool active)
        {
            SetGraphicEnabled(dashBuffIcon, active);
            if (!active) ResetScale(dashBuffIcon);
        }

        /// <summary>Actualiza la vida en Animator; opcionalmente también corazones.</summary>
        public void SetHealth(int current)
        {
            if (heartsController)
                heartsController.SetHearts(current);
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

        private void UpdateCooldown(Image fill, TextMeshProUGUI txt, float left, float total, ref int cachedTenths)
        {
            bool onCd = left > 0.0001f && total > 0.0001f;
            float ratio = onCd ? Mathf.Clamp01(1f - (left / total)) : 1f;

            if (fill)
            {
                fill.enabled = true;
                fill.fillAmount = ratio;

                // Blink sutil cuando está listo
                if (!onCd || left <= readyBlinkThreshold)
                {
                    float blink = (!onCd) ? 1f : 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 12f);
                    var c = fill.color; c.a = blink; fill.color = c;
                }
                else
                {
                    if (fill.color.a != 1f)
                    {
                        var c = fill.color; c.a = 1f; fill.color = c;
                    }
                }
            }

            if (txt)
            {
                if (!onCd)
                {
                    if (txt.text.Length > 0) txt.text = "";
                }
                else
                {
                    int tenths = Mathf.Clamp(Mathf.RoundToInt(left * 10f), 0, 9999);
                    if (tenths != cachedTenths)
                    {
                        cachedTenths = tenths;
                        txt.SetText("{0:0.0}", tenths / 10f);
                    }
                }
            }
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

            _pulseT = (_pulseT + dt * pickupPulseSpeed) % (Mathf.PI * 2f);
            float s = Mathf.Lerp(pickupPulseMinScale, 1f, 0.5f * (1f + Mathf.Sin(_pulseT)));

            if (hasExtraJump) SetScale(extraJumpIcon, s); else ResetScale(extraJumpIcon);
            if (dashBuffPending) SetScale(dashBuffIcon, s); else ResetScale(dashBuffIcon);
        }

        private void HideSpinChargeUI()
        {
            if (spinChargeFill) { spinChargeFill.enabled = false; spinChargeFill.fillAmount = 0f; }
            if (spinChargeMinMark) spinChargeMinMark.enabled = false;
        }

        private static void EnsureFilledSetup(Image img,
            Image.FillMethod method = Image.FillMethod.Radial360,
            int origin = (int)Image.Origin360.Top,
            bool clockwise = true)
        {
            if (!img) return;
            if (img.type != Image.Type.Filled) img.type = Image.Type.Filled;
            if (img.fillMethod != method) img.fillMethod = method;
            if (img.fillOrigin != origin) img.fillOrigin = origin;
            img.fillClockwise = clockwise;
        }

        private static void SetGraphicEnabled(Graphic g, bool enabled)
        {
            if (!g) return;
            if (g.enabled == enabled) return; 
            
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

            float t = 0f;
            while (t < half)
            {
                t += Time.unscaledDeltaTime;
                color.a = Mathf.Lerp(0f, 1f, t / half);
                damagedImage.color = color;
                yield return null;
            }

            t = 0f;
            while (t < half)
            {
                t += Time.unscaledDeltaTime;
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
            yield return new WaitForSecondsRealtime(Mathf.Max(0f, blindnessDuration));
            blindnessEffect.SetActive(false);
        }
    }
}

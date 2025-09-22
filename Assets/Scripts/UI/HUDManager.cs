using System.Collections;
using Player.New;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// - Spin: carga (fill + marca mínimo) y cooldown (polling del PlayerModel).
    /// - Pickups: dispara animaciones Get/Use en widgets (polling del PlayerModel).
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
        [SerializeField] private TextMeshProUGUI spinCdText;
        [SerializeField] private Image dashCdFill;
        [SerializeField] private TextMeshProUGUI dashCdText;
        [SerializeField] private Image vertCdFill;
        [SerializeField] private TextMeshProUGUI vertCdText;
        [SerializeField, Tooltip("Cuando queda menos de esto (s), se muestra listo (blink).")]
        private float readyBlinkThreshold = 0.15f;

        [Header("Pickups (widgets)")]
        [SerializeField, Tooltip("Manager que dispara animaciones Get/Use en los widgets de pickup.")]
        private PickupsUIManager pickups;

        [Header("Vida / Daño / Ceguera")]
        [SerializeField, Tooltip("Overlay de daño (Image con alpha).")]
        private Image damagedImage;
        [SerializeField] private float damagedDuration = 0.3f;
        [SerializeField, Tooltip("Overlay/GO para 'ceguera' temporal.")]
        private GameObject blindnessEffect;
        [SerializeField] private float blindnessDuration = 2f;

        [Header("Corazones")]
        [SerializeField, Tooltip("Si se asigna, también actualiza el UI de corazones.")]
        private HeartsUIManager heartsController;
        
        private Coroutine _blindnessCo;
        private Coroutine _damagedCo;
        private int _spinTenths, _dashTenths, _vertTenths;

        private bool _prevExtraJump;
        private bool _prevDashBuff;

        private void Awake()
        {
            
            EnsureFilledSetup(spinChargeFill, Image.FillMethod.Radial360, (int)Image.Origin360.Top, true);
            EnsureFilledSetup(spinChargeMinMark, Image.FillMethod.Radial360, (int)Image.Origin360.Top, true);
            HideSpinChargeUI();
            
            _prevExtraJump = model && model.HasExtraJump;
            _prevDashBuff  = model && model.DashBuffPending;
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
            
            UpdatePickupWidgets(model.HasExtraJump, model.DashBuffPending);
        }

        /// <summary>Progreso de carga del spin (llamado mientras se mantiene el input).</summary>
        public void OnSpinChargeProgress(float current, float min, float max)
        {
            if (!spinChargeFill) return;

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
            HideSpinChargeUI();
        }

        /// <summary>Actualiza la vida; delega a la UI de corazones si está asignada.</summary>
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

        /// <summary>Dispara animaciones Get/Use en los widgets de pickups (por flancos).</summary>
        private void UpdatePickupWidgets(bool hasExtraJump, bool dashBuffPending)
        {
            if (!pickups) return;

            if (hasExtraJump != _prevExtraJump)
            {
                if (hasExtraJump) pickups.OnPickupGet(PickupId.ExtraJump);
                else              pickups.OnPickupUse(PickupId.ExtraJump);
                _prevExtraJump = hasExtraJump;
            }

            if (dashBuffPending != _prevDashBuff)
            {
                if (dashBuffPending) pickups.OnPickupGet(PickupId.DashBuff);
                else                 pickups.OnPickupUse(PickupId.DashBuff);
                _prevDashBuff = dashBuffPending;
            }
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
        
        public void TriggerPickupGet(PickupId id)
        {
            if (!pickups) return;
            pickups.OnPickupGet(id);
            
            if (id == PickupId.ExtraJump) _prevExtraJump = true;
            else if (id == PickupId.DashBuff) _prevDashBuff = true;
        }

        public void TriggerPickupUse(PickupId id)
        {
            if (!pickups) return;
            pickups.OnPickupUse(id);

            if (id == PickupId.ExtraJump) _prevExtraJump = false;
            else if (id == PickupId.DashBuff) _prevDashBuff = false;
        }

    }
}

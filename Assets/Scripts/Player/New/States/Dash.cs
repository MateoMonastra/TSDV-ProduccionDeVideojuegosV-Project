using FSM;
using UnityEngine;

namespace Player.New
{
    public class Dash : FinishableState
    {
        public const string ToFall = "Dash->Fall";
        public const string ToWalkIdle = "Dash->WalkIdle";
        public const string ToDash = "Locomotion->Dash";

        private readonly MyKinematicMotor _m;
        private readonly PlayerModel _model;
        private readonly System.Action<string> _req;
        private readonly PlayerAnimationController _anim;

        private Vector3 _dir;
        private float _duration;
        private float _t;

        // Recuperación (ease-out)
        private bool _recovering;
        private float _recoverT;

        public System.Action<float> OnDashCooldownUI; // opcional UI

        public Dash(MyKinematicMotor m, PlayerModel model, System.Action<string> req, PlayerAnimationController anim = null)
        { _m = m; _model = model; _req = req; _anim = anim; }

        public override void Enter()
        {
            base.Enter();
            
            _model.sprintArmed = false;

            // Evitar que el snap al suelo corte el inicio
            _m.ForceUnground(0.08f);

            _anim?.TriggerDash();

            // Dirección: input si hay, sino forward del personaje
            Vector3 forward = _m.transform.forward;
            Vector3 inputDir = _model.moveInputWorld.sqrMagnitude > 1e-5f ? _model.moveInputWorld.normalized : forward;
            _dir = new Vector3(inputDir.x, 0f, inputDir.z).normalized;
            if (_dir.sqrMagnitude < 1e-5f) _dir = forward;

            // Duración = distancia / velocidad
            _duration = Mathf.Max(0.01f, _model.dashDistance / Mathf.Max(0.01f, _model.dashSpeed));
            _t = 0f;

            _recovering = false;
            _recoverT = 0f;

            // Invulnerable durante la fase "rápida"
            _model.invulnerableToEnemies = true;

            // Velocidad horizontal constante
            var v = _m.Velocity;
            v.x = _dir.x * _model.dashSpeed;
            v.z = _dir.z * _model.dashSpeed;
            _m.SetVelocity(v);

            // Cooldown
            _model.dashOnCooldown = true;
            _model.dashCooldownLeft = _model.dashCooldown;
            OnDashCooldownUI?.Invoke(_model.dashCooldownLeft);
        }

        public override void Exit()
        {
            base.Exit();
            // por las dudas, aseguramos que no quede invulnerable
            _model.sprintArmed = true;
            _model.invulnerableToEnemies = false;
        }

        public override void Tick(float dt)
        {
            base.Tick(dt);

            if (!_recovering)
            {
                // Fase rápida
                _t += dt;

                var v = _m.Velocity;
                v.x = _dir.x * _model.dashSpeed;
                v.z = _dir.z * _model.dashSpeed;
                _m.SetVelocity(v);

                if (_t >= _duration)
                {
                    // Inicia la fase de recuperación (suavizado)
                    _recovering = true;
                    _recoverT = 0f;

                    // Vuelve a ser vulnerable al terminar el desplazamiento "rápido"
                    _model.invulnerableToEnemies = false;
                }
                return;
            }

            // ---------- FASE DE RECUPERACIÓN (EASE-OUT) ----------
            _recoverT += dt;

            bool inAir = !_m.IsGrounded;

            // Target de velocidad horizontal:
            // - Si hay input, hacia el movimiento normal (Walk/Air).
            // - Si NO hay input:
            //      * en suelo: hacia 0 (pero de forma suave)
            //      * en aire: mantener dirección del dash, a velocidad aérea
            Vector3 desiredDir =
                _model.moveInputWorld.sqrMagnitude > 1e-5f
                    ? _model.moveInputWorld
                    : (inAir ? _dir : Vector3.zero);

            float targetSpeed = inAir ? _model.airHorizontalSpeed : _model.moveSpeed;
            Vector3 targetHoriz = desiredDir * targetSpeed;

            var vel = _m.Velocity;
            Vector3 horiz = new Vector3(vel.x, 0f, vel.z);

            // Interpolación suave: expo hacia el target
            float alpha = 1f - Mathf.Exp(-_model.dashExitSharpness * dt);
            horiz = Vector3.Lerp(horiz, targetHoriz, alpha);

            vel.x = horiz.x; vel.z = horiz.z;
            _m.SetVelocity(vel);

            // Fin de la recuperación: ahora sí salimos del estado
            if (_recoverT >= _model.dashExitBlendTime)
            {
                if (inAir) _req?.Invoke(ToFall);
                else       _req?.Invoke(ToWalkIdle);
                Finish();
            }
        }

        public static bool CanUse(PlayerModel model) => !model.dashOnCooldown;
    }
}

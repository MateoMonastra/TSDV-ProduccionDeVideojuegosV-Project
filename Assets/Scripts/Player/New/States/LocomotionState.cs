using FSM;
using UnityEngine;

namespace Player.New
{
    public abstract class LocomotionState : State
    {
        protected readonly MyKinematicMotor Motor;
        protected readonly PlayerModel Model;
        protected readonly Transform Cam;
        protected readonly System.Action<string> RequestTransition;

        protected LocomotionState(MyKinematicMotor motor, PlayerModel model, Transform cam, System.Action<string> req)
        { Motor = motor; Model = model; Cam = cam; RequestTransition = req; }

        protected void ApplyLocomotion(float dt, bool inAir, bool limitAirSpeed = false, float maxAirSpeed = 7f)
        {
            // Si está bloqueada la locomoción (ataque vertical, spin release), no mover ni rotar
            if (Model.locomotionBlocked)
            {
                ZeroHorizontalIfGrounded();
                return;
            }

            // Ejes planos de cámara
            Vector3 up = Motor.CharacterUp;
            Vector3 camPlanarForward = Vector3.ProjectOnPlane(Cam.forward, up).normalized;
            if (camPlanarForward.sqrMagnitude < 1e-4f)
                camPlanarForward = Vector3.ProjectOnPlane(Cam.up, up).normalized;
            Vector3 camPlanarRight = Vector3.Cross(up, camPlanarForward);

            // Dirección cámara-relativa (input crudo clamp a 1)
            Vector3 desiredDir = (camPlanarForward * Model.rawMoveInput.y + camPlanarRight * Model.rawMoveInput.x);
            desiredDir = Vector3.ClampMagnitude(desiredDir, 1f);

            // Velocidad deseada (suelo/aire) con multiplicador de Acciones
            float baseSpeed = inAir ? Model.airHorizontalSpeed : Model.moveSpeed;
            float targetSpeed = baseSpeed * Mathf.Max(0f, Model.actionMoveSpeedMultiplier);
            Vector3 desiredVel = desiredDir * targetSpeed;

            Vector3 v = Motor.Velocity;
            Vector3 horiz = new Vector3(v.x, 0f, v.z);

            if (inAir)
            {
                // Aire: nos acercamos con aceleración fija (suave) y cap opcional
                horiz = Vector3.MoveTowards(horiz, desiredVel, Model.moveAcceleration * dt);
                if (limitAirSpeed && horiz.magnitude > maxAirSpeed)
                    horiz = horiz.normalized * maxAirSpeed;
            }
            else
            {
                // Suelo: aceleración fija hacia el target, pero si no hay input, frenamos de golpe (sin patinar)
                if (desiredDir.sqrMagnitude <= 1e-5f)
                {
                    horiz = Vector3.zero;
                }
                else
                {
                    horiz = Vector3.MoveTowards(horiz, desiredVel, Model.moveAcceleration * dt);
                }
            }

            v.x = horiz.x; v.z = horiz.z;
            Motor.SetVelocity(v);

            // Rotación (aim-lock si aplica)
            Vector3 lookDir;
            if (Model.aimLockActive && Model.aimLockDirection.sqrMagnitude > 1e-6f)
                lookDir = Model.aimLockDirection.normalized;
            else
                lookDir = (Model.orientationMethod == OrientationMethod.TowardsCamera)
                    ? camPlanarForward
                    : (desiredDir.sqrMagnitude > 1e-5f ? desiredDir : camPlanarForward);

            Motor.SmoothRotation(lookDir, Model.orientationSharpness, dt);
        }

        protected void ZeroHorizontalIfGrounded()
        {
            if (Motor.IsGrounded)
            {
                var v = Motor.Velocity;
                v.x = 0f; v.z = 0f;
                Motor.SetVelocity(v);
            }
        }
    }
}

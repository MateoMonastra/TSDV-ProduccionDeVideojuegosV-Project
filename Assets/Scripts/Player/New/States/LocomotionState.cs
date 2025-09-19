using FSM;
using UnityEngine;

namespace Player.New
{
    /// <summary>
    /// Base para estados de locomoción. Ofrece utilidades para mover y orientar
    /// al personaje según la cámara y los parámetros del <see cref="PlayerModel"/>.
    /// </summary>
    public abstract class LocomotionState : State
    {
        protected readonly MyKinematicMotor Motor;
        protected readonly PlayerModel Model;
        protected readonly Transform Cam;
        protected readonly System.Action<string> RequestTransition;

        protected LocomotionState(MyKinematicMotor motor, PlayerModel model, Transform cam, System.Action<string> req)
        {
            Motor = motor;
            Model = model;
            Cam = cam;
            RequestTransition = req;
        }

        /// <summary>
        /// Mueve y rota al personaje:
        /// - En suelo: acelera hacia la velocidad objetivo o frena en seco sin input.
        /// - En aire: acelera suave y permite limitar velocidad horizontal.
        /// Respeta <see cref="PlayerModel.LocomotionBlocked"/> y multiplicadores de acción.
        /// </summary>
        protected void ApplyLocomotion(float dt, bool inAir, bool limitAirSpeed = false, float maxAirSpeed = 7f)
        {
            // Si una acción bloquea locomoción, detiene el horizontal en suelo
            if (Model.LocomotionBlocked)
            {
                ZeroHorizontalIfGrounded();
                return;
            }

            // Ejes de cámara en plano
            Vector3 up = Motor.CharacterUp;
            Vector3 camPlanarForward = Vector3.ProjectOnPlane(Cam.forward, up).normalized;
            if (camPlanarForward.sqrMagnitude < 1e-4f)
                camPlanarForward = Vector3.ProjectOnPlane(Cam.up, up).normalized;
            Vector3 camPlanarRight = Vector3.Cross(up, camPlanarForward);

            // Dirección deseada desde input (clamp a 1)
            Vector3 desiredDir = (camPlanarForward * Model.RawMoveInput.y + camPlanarRight * Model.RawMoveInput.x);
            desiredDir = Vector3.ClampMagnitude(desiredDir, 1f);

            // Velocidad objetivo (suelo/aire) con multiplicador de acciones
            float baseSpeed = inAir ? Model.AirHorizontalSpeed : Model.MoveSpeed;
            float targetSpeed = baseSpeed * Mathf.Max(0f, Model.ActionMoveSpeedMultiplier);
            Vector3 desiredVel = desiredDir * targetSpeed;

            // Mezcla de velocidades
            Vector3 v = Motor.Velocity;
            Vector3 horiz = new Vector3(v.x, 0f, v.z);

            if (inAir)
            {
                if (limitAirSpeed)
                    desiredVel = Vector3.ClampMagnitude(desiredVel, maxAirSpeed);

                horiz = Vector3.MoveTowards(horiz, desiredVel, Model.MoveAcceleration * dt);
            }
            else
            {
                bool isSprinting = Model.ActionMoveSpeedMultiplier > 1.01f;
                
                float reorientSharp = isSprinting
                    ? Model.SprintReorientationSharpness
                    : Model.GroundReorientationSharpness;

                float turnT = 1f - Mathf.Exp(-Mathf.Max(1f, reorientSharp) * dt);

                if (desiredDir.sqrMagnitude > 1e-5f)
                {
                    float speed = horiz.magnitude;

                    if (speed > 0.0001f)
                    {
                        Vector3 turned = Vector3.Slerp(horiz.normalized, desiredDir, turnT);
                        horiz = turned * speed;
                    }
                    
                    horiz = AntiSkid(dt, desiredDir, horiz, isSprinting);
                    
                    horiz = Vector3.MoveTowards(horiz, desiredVel, Model.MoveAcceleration * dt);
                    
                    float maxGround = Model.MoveSpeed * Mathf.Max(0f, Model.ActionMoveSpeedMultiplier);
                    if (horiz.magnitude > maxGround)
                        horiz = horiz.normalized * maxGround;
                }
                else
                {
                    float braking = isSprinting ? Model.SprintBrakingDecel : Model.GroundBrakingDecel;
                    horiz = Vector3.MoveTowards(horiz, Vector3.zero, Mathf.Max(0f, braking) * dt);
                }
            }


            v.x = horiz.x;
            v.z = horiz.z;
            Motor.SetVelocity(v);

            Vector3 lookDir = Vector3.zero;

            if (Model.AimLockActive && Model.AimLockDirection.sqrMagnitude > 1e-6f)
            {
                lookDir = Model.AimLockDirection.normalized;
            }
            else
            {
                if (desiredDir.sqrMagnitude > 1e-5f)
                {
                    lookDir = desiredDir;
                }
                else
                {
                    if (Model.Orientation == PlayerModel.OrientationMethod.TowardsCamera &&
                        Model.OrientWithCameraWhileIdle)
                    {
                        lookDir = camPlanarForward;
                    }
                }
            }

            if (lookDir.sqrMagnitude > 1e-6f)
            {
                Motor.SmoothRotation(lookDir, Model.OrientationSharpness, dt);
            }
        }

        private Vector3 AntiSkid(float dt, Vector3 desiredDir, Vector3 horiz, bool isSprinting)
        {
            Vector3 fwd = desiredDir;
            Vector3 lateral = horiz - Vector3.Project(horiz, fwd);
            float latFriction = isSprinting ? Model.SprintLateralFriction : Model.GroundLateralFriction;
            horiz -= lateral * (latFriction * dt);
            return horiz;
        }

        /// <summary>Si está en suelo, anula el movimiento horizontal.</summary>
        protected void ZeroHorizontalIfGrounded()
        {
            if (Motor.IsGrounded)
            {
                var v = Motor.Velocity;
                v.x = 0f;
                v.z = 0f;
                Motor.SetVelocity(v);
            }
        }
    }
}
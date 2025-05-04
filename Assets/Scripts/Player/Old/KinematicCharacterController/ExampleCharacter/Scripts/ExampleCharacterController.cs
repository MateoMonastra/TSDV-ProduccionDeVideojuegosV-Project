using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using System;
using Player.Old.PlayerPrototype;
using UnityEngine.Serialization;

namespace KinematicCharacterController.Examples
{
    public enum CharacterState
    {
        Default,
        Dashing,
    }

    public enum OrientationMethod
    {
        TowardsCamera,
        TowardsMovement,
    }

    public struct PlayerCharacterInputs
    {
        public float MoveAxisForward;
        public float MoveAxisRight;
        public Quaternion CameraRotation;
        public bool JumpDown;
        public bool CrouchDown;
        public bool CrouchUp;
        public bool DashDown;
    }

    public struct AICharacterInputs
    {
        public Vector3 MoveVector;
        public Vector3 LookVector;
    }

    public enum BonusOrientationMethod
    {
        None,
        TowardsGravity,
        TowardsGroundSlopeAndGravity,
    }

    public class ExampleCharacterController : MonoBehaviour, ICharacterController
    {
        public PlayerModel Model;
        public KinematicCharacterMotor Motor;

        [Header("Animation")]
        [SerializeField] private Animator animator;
        private static readonly int IsWalking = Animator.StringToHash("IsWalking");
        private static readonly int IsDashing = Animator.StringToHash("IsDashing");
        private static readonly int IsJumping = Animator.StringToHash("IsJumping");
        private static readonly int IsFalling = Animator.StringToHash("IsFalling");
        private static readonly int IsIdle = Animator.StringToHash("IsIdle");

        

        [Header("Misc")]
        public List<Collider> IgnoredColliders = new List<Collider>();
        public BonusOrientationMethod BonusOrientationMethod = BonusOrientationMethod.None;
        public float BonusOrientationSharpness = 10f;
        public Transform MeshRoot;
        public Transform CameraFollowPoint;
        public float CrouchedCapsuleHeight = 1f;

        
        private int _extraDashCharges = 0;

        public CharacterState CurrentCharacterState { get; private set; }

        private Collider[] _probedColliders = new Collider[8];
        private RaycastHit[] _probedHits = new RaycastHit[8];
        private Vector3 _moveInputVector;
        private Vector3 _lookInputVector;
        private bool _jumpRequested = false;
        private bool _jumpConsumed = false;
        private bool _jumpedThisFrame = false;
        private float _timeSinceJumpRequested = Mathf.Infinity;
        private float _timeSinceLastAbleToJump = 0f;
        private Vector3 _internalVelocityAdd = Vector3.zero;
        private bool _shouldBeCrouching = false;
        private bool _isCrouching = false;

        private Vector3 lastInnerNormal = Vector3.zero;
        private Vector3 lastOuterNormal = Vector3.zero;

        private bool _isDashing = false;
        private float _dashTimeRemaining = 0f;
        private float _dashCooldownRemaining = 0f;
        private Vector3 _dashDirection;
        private bool _hasExtraCharge = false;

        private int _jumpsRemaining; // Track remaining jumps

        private void Awake()
        {
            // Handle initial state
            TransitionToState(CharacterState.Default);

            // Assign the characterController to the motor
            Motor.CharacterController = this;

            // Get animator reference if not set
            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }

            // Initialize jumps
            _jumpsRemaining = Model.MaxJumps;
        }

        /// <summary>
        /// Handles movement state transitions and enter/exit callbacks
        /// </summary>
        public void TransitionToState(CharacterState newState)
        {
            CharacterState tmpInitialState = CurrentCharacterState;
            OnStateExit(tmpInitialState, newState);
            CurrentCharacterState = newState;
            OnStateEnter(newState, tmpInitialState);
        }

        /// <summary>
        /// Event when entering a state
        /// </summary>
        public void OnStateEnter(CharacterState state, CharacterState fromState)
        {
            switch (state)
            {
                case CharacterState.Default:
                    {
                        break;
                    }
                case CharacterState.Dashing:
                    {
                        // Update animation immediately when entering dash state
                        UpdateAnimationParameters();
                        break;
                    }
            }
        }

        /// <summary>
        /// Event when exiting a state
        /// </summary>
        public void OnStateExit(CharacterState state, CharacterState toState)
        {
            switch (state)
            {
                case CharacterState.Default:
                    {
                        break;
                    }
                case CharacterState.Dashing:
                    {
                        // Update animation immediately when exiting dash state
                        UpdateAnimationParameters();
                        break;
                    }
            }
        }

        /// <summary>
        /// This is called every frame by ExamplePlayer in order to tell the character what its inputs are
        /// </summary>
        public void SetInputs(ref PlayerCharacterInputs inputs)
        {
            // Clamp input
            Vector3 moveInputVector = Vector3.ClampMagnitude(new Vector3(inputs.MoveAxisRight, 0f, inputs.MoveAxisForward), 1f);

            // Calculate camera direction and rotation on the character plane
            Vector3 cameraPlanarDirection = Vector3.ProjectOnPlane(inputs.CameraRotation * Vector3.forward, Motor.CharacterUp).normalized;
            if (cameraPlanarDirection.sqrMagnitude == 0f)
            {
                cameraPlanarDirection = Vector3.ProjectOnPlane(inputs.CameraRotation * Vector3.up, Motor.CharacterUp).normalized;
            }
            Quaternion cameraPlanarRotation = Quaternion.LookRotation(cameraPlanarDirection, Motor.CharacterUp);

            switch (CurrentCharacterState)
            {
                case CharacterState.Default:
                    {
                        // Move and look inputs
                        _moveInputVector = cameraPlanarRotation * moveInputVector;

                        switch (Model.OrientationMethod)
                        {
                            case OrientationMethod.TowardsCamera:
                                _lookInputVector = cameraPlanarDirection;
                                break;
                            case OrientationMethod.TowardsMovement:
                                _lookInputVector = _moveInputVector.normalized;
                                break;
                        }

                        // Dash input
                        if (inputs.DashDown && CanDash())
                        {
                            _isDashing = true;
                            _dashTimeRemaining = Model.DashDuration;
                            UseDash();
                            
                            // Calculate dash direction based on input or camera
                            Vector3 inputDirection = Vector3.zero;
                            if (moveInputVector.sqrMagnitude > 0.01f)
                            {
                                // Use input direction if there is input
                                inputDirection = cameraPlanarRotation * moveInputVector;
                            }
                            else
                            {
                                // Fall back to camera direction if no input
                                inputDirection = cameraPlanarDirection;
                            }
                            
                            _dashDirection = inputDirection.normalized;
                            TransitionToState(CharacterState.Dashing);
                        }

                        // Jumping input
                        if (inputs.JumpDown)
                        {
                            _timeSinceJumpRequested = 0f;
                            _jumpRequested = true;
                        }

                        // Crouching input
                        if (inputs.CrouchDown)
                        {
                            _shouldBeCrouching = true;

                            if (!_isCrouching)
                            {
                                _isCrouching = true;
                                Motor.SetCapsuleDimensions(0.5f, CrouchedCapsuleHeight, CrouchedCapsuleHeight * 0.5f);
                                MeshRoot.localScale = new Vector3(1f, 0.5f, 1f);
                            }
                        }
                        else if (inputs.CrouchUp)
                        {
                            _shouldBeCrouching = false;
                        }

                        break;
                    }
                case CharacterState.Dashing:
                    {
                        // Dash input
                        if (inputs.DashDown && CanDash())
                        {
                            Debug.Log("trying");
                            _isDashing = true;
                            _dashTimeRemaining = Model.DashDuration;
                            UseDash();
                            
                            // Calculate dash direction based on input or camera
                            Vector3 inputDirection = Vector3.zero;
                            if (moveInputVector.sqrMagnitude > 0.01f)
                            {
                                // Use input direction if there is input
                                inputDirection = cameraPlanarRotation * moveInputVector;
                            }
                            else
                            {
                                // Fall back to camera direction if no input
                                inputDirection = cameraPlanarDirection;
                            }
                            
                            _dashDirection = inputDirection.normalized;
                            TransitionToState(CharacterState.Dashing);
                        }
                        else
                        {
                            // While dashing, we don't process other inputs
                            _moveInputVector = Vector3.zero;
                            _lookInputVector = _dashDirection;
                        }
                        
                    }
                    break;
            }
        }

        /// <summary>
        /// This is called every frame by the AI script in order to tell the character what its inputs are
        /// </summary>
        public void SetInputs(ref AICharacterInputs inputs)
        {
            _moveInputVector = inputs.MoveVector;
            _lookInputVector = inputs.LookVector;
        }

        private Quaternion _tmpTransientRot;

        /// <summary>
        /// (Called by KinematicCharacterMotor during its update cycle)
        /// This is called before the character begins its movement update
        /// </summary>
        public void BeforeCharacterUpdate(float deltaTime)
        {
            // Update dash cooldown
            if (_dashCooldownRemaining > 0f)
            {
                _dashCooldownRemaining -= deltaTime;
            }

            if (_isDashing)
            {
                _dashTimeRemaining -= deltaTime;
                if (_dashTimeRemaining <= 0f)
                {
                    _isDashing = false;
                    _dashCooldownRemaining = Model.DashCooldown;
                    // Reset velocity when dash ends
                    Motor.BaseVelocity = Vector3.zero;
                    TransitionToState(CharacterState.Default);
                }
            }

            // Update animation parameters
            UpdateAnimationParameters();
        }

        public void ResetDashCD()
        {
            _dashCooldownRemaining = 0f;
        }
        
        /// <summary>
        /// (Called by KinematicCharacterMotor during its update cycle)
        /// This is where you tell your character what its rotation should be right now. 
        /// This is the ONLY place where you should set the character's rotation
        /// </summary>
        public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            switch (CurrentCharacterState)
            {
                case CharacterState.Default:
                    {
                        if (_lookInputVector.sqrMagnitude > 0f && Model.OrientationSharpness > 0f)
                        {
                            // Smoothly interpolate from current to target look direction
                            Vector3 smoothedLookInputDirection = Vector3.Slerp(Motor.CharacterForward, _lookInputVector, 1 - Mathf.Exp(-Model.OrientationSharpness * deltaTime)).normalized;

                            // Set the current rotation (which will be used by the KinematicCharacterMotor)
                            currentRotation = Quaternion.LookRotation(smoothedLookInputDirection, Motor.CharacterUp);
                        }

                        Vector3 currentUp = (currentRotation * Vector3.up);
                        if (BonusOrientationMethod == BonusOrientationMethod.TowardsGravity)
                        {
                            // Rotate from current up to invert gravity
                            Vector3 smoothedGravityDir = Vector3.Slerp(currentUp, -Model.Gravity.normalized, 1 - Mathf.Exp(-BonusOrientationSharpness * deltaTime));
                            currentRotation = Quaternion.FromToRotation(currentUp, smoothedGravityDir) * currentRotation;
                        }
                        else if (BonusOrientationMethod == BonusOrientationMethod.TowardsGroundSlopeAndGravity)
                        {
                            if (Motor.GroundingStatus.IsStableOnGround)
                            {
                                Vector3 initialCharacterBottomHemiCenter = Motor.TransientPosition + (currentUp * Motor.Capsule.radius);

                                Vector3 smoothedGroundNormal = Vector3.Slerp(Motor.CharacterUp, Motor.GroundingStatus.GroundNormal, 1 - Mathf.Exp(-BonusOrientationSharpness * deltaTime));
                                currentRotation = Quaternion.FromToRotation(currentUp, smoothedGroundNormal) * currentRotation;

                                // Move the position to create a rotation around the bottom hemi center instead of around the pivot
                                Motor.SetTransientPosition(initialCharacterBottomHemiCenter + (currentRotation * Vector3.down * Motor.Capsule.radius));
                            }
                            else
                            {
                                Vector3 smoothedGravityDir = Vector3.Slerp(currentUp, -Model.Gravity.normalized, 1 - Mathf.Exp(-BonusOrientationSharpness * deltaTime));
                                currentRotation = Quaternion.FromToRotation(currentUp, smoothedGravityDir) * currentRotation;
                            }
                        }
                        else
                        {
                            Vector3 smoothedGravityDir = Vector3.Slerp(currentUp, Vector3.up, 1 - Mathf.Exp(-BonusOrientationSharpness * deltaTime));
                            currentRotation = Quaternion.FromToRotation(currentUp, smoothedGravityDir) * currentRotation;
                        }
                        break;
                    }
            }
        }

        /// <summary>
        /// (Called by KinematicCharacterMotor during its update cycle)
        /// This is where you tell your character what its velocity should be right now. 
        /// This is the ONLY place where you can set the character's velocity
        /// </summary>
        public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            switch (CurrentCharacterState)
            {
                case CharacterState.Default:
                    {
                        // Ground movement
                        if (Motor.GroundingStatus.IsStableOnGround)
                        {
                            float currentVelocityMagnitude = currentVelocity.magnitude;

                            Vector3 effectiveGroundNormal = Motor.GroundingStatus.GroundNormal;

                            // Reorient velocity on slope
                            currentVelocity = Motor.GetDirectionTangentToSurface(currentVelocity, effectiveGroundNormal) * currentVelocityMagnitude;

                            // Calculate target velocity
                            Vector3 inputRight = Vector3.Cross(_moveInputVector, Motor.CharacterUp);
                            Vector3 reorientedInput = Vector3.Cross(effectiveGroundNormal, inputRight).normalized * _moveInputVector.magnitude;
                            Vector3 targetMovementVelocity = reorientedInput * Model.MaxStableMoveSpeed;

                            // Smooth movement Velocity
                            currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity, 1f - Mathf.Exp(-Model.StableMovementSharpness * deltaTime));
                        }
                        // Air movement
                        else
                        {
                            // Add move input
                            if (_moveInputVector.sqrMagnitude > 0f)
                            {
                                Vector3 addedVelocity = _moveInputVector * (Model.AirAccelerationSpeed * deltaTime);

                                Vector3 currentVelocityOnInputsPlane = Vector3.ProjectOnPlane(currentVelocity, Motor.CharacterUp);

                                // Limit air velocity from inputs
                                if (currentVelocityOnInputsPlane.magnitude < Model.MaxAirMoveSpeed)
                                {
                                    // clamp addedVel to make total vel not exceed max vel on inputs plane
                                    Vector3 newTotal = Vector3.ClampMagnitude(currentVelocityOnInputsPlane + addedVelocity, Model.MaxAirMoveSpeed);
                                    addedVelocity = newTotal - currentVelocityOnInputsPlane;
                                }
                                else
                                {
                                    // Make sure added vel doesn't go in the direction of the already-exceeding velocity
                                    if (Vector3.Dot(currentVelocityOnInputsPlane, addedVelocity) > 0f)
                                    {
                                        addedVelocity = Vector3.ProjectOnPlane(addedVelocity, currentVelocityOnInputsPlane.normalized);
                                    }
                                }

                                // Prevent air-climbing sloped walls
                                if (Motor.GroundingStatus.FoundAnyGround)
                                {
                                    if (Vector3.Dot(currentVelocity + addedVelocity, addedVelocity) > 0f)
                                    {
                                        Vector3 perpenticularObstructionNormal = Vector3.Cross(Vector3.Cross(Motor.CharacterUp, Motor.GroundingStatus.GroundNormal), Motor.CharacterUp).normalized;
                                        addedVelocity = Vector3.ProjectOnPlane(addedVelocity, perpenticularObstructionNormal);
                                    }
                                }

                                // Apply added velocity
                                currentVelocity += addedVelocity;
                            }

                            // Gravity
                            currentVelocity += Model.Gravity * deltaTime;

                            // Drag
                            currentVelocity *= (1f / (1f + (Model.Drag * deltaTime)));
                        }

                        // Handle jumping
                        _jumpedThisFrame = false;
                        _timeSinceJumpRequested += deltaTime;

                        if (_jumpRequested)
                        {
                            if (_jumpsRemaining > 0)
                            {
                                Jump(ref currentVelocity);
                            }
                        }

                        // Take into account additive velocity
                        if (_internalVelocityAdd.sqrMagnitude > 0f)
                        {
                            currentVelocity += _internalVelocityAdd;
                            _internalVelocityAdd = Vector3.zero;
                        }
                        break;
                    }
                case CharacterState.Dashing:
                    {
                        // During dash, we set the velocity directly without any interpolation
                        currentVelocity = _dashDirection * Model.DashSpeed;
                        break;
                    }
            }
        }

        private void Jump(ref Vector3 currentVelocity)
        {
            // Calculate jump direction before ungrounding
            Vector3 jumpDirection = Motor.CharacterUp;
            if (Motor.GroundingStatus.FoundAnyGround && !Motor.GroundingStatus.IsStableOnGround)
            {
                jumpDirection = Motor.GroundingStatus.GroundNormal;
            }

            // Makes the character skip ground probing/snapping on its next update
            Motor.ForceUnground();

            // Add to the return velocity and reset jump state
            currentVelocity += (jumpDirection * Model.JumpUpSpeed) - Vector3.Project(currentVelocity, Motor.CharacterUp);
            currentVelocity += (_moveInputVector * Model.JumpScalableForwardSpeed);
            _jumpedThisFrame = true;
            _jumpsRemaining--; // Decrease remaining jumps
            _jumpRequested = false; // Reset jump request after executing
        }

        /// <summary>
        /// (Called by KinematicCharacterMotor during its update cycle)
        /// This is called after the character has finished its movement update
        /// </summary>
        public void AfterCharacterUpdate(float deltaTime)
        {
            switch (CurrentCharacterState)
            {
                case CharacterState.Default:
                    {
                        // Handle jump-related values
                        {
                            // Handle jumping pre-ground grace period
                            if (_jumpRequested && _timeSinceJumpRequested > Model.JumpPreGroundingGraceTime)
                            {
                                _jumpRequested = false;
                            }

                            if (Model.AllowJumpingWhenSliding ? Motor.GroundingStatus.FoundAnyGround : Motor.GroundingStatus.IsStableOnGround)
                            {
                                // If we're on a ground surface, reset jumping values
                                if (!_jumpedThisFrame)
                                {
                                    _jumpConsumed = false;
                                }
                                _timeSinceLastAbleToJump = 0f;
                            }
                            else
                            {
                                // Keep track of time since we were last able to jump (for grace period)
                                _timeSinceLastAbleToJump += deltaTime;
                            }
                        }

                        // Handle uncrouching
                        if (_isCrouching && !_shouldBeCrouching)
                        {
                            // Do an overlap test with the character's standing height to see if there are any obstructions
                            Motor.SetCapsuleDimensions(0.5f, 2f, 1f);
                            if (Motor.CharacterOverlap(
                                Motor.TransientPosition,
                                Motor.TransientRotation,
                                _probedColliders,
                                Motor.CollidableLayers,
                                QueryTriggerInteraction.Ignore) > 0)
                            {
                                // If obstructions, just stick to crouching dimensions
                                Motor.SetCapsuleDimensions(0.5f, CrouchedCapsuleHeight, CrouchedCapsuleHeight * 0.5f);
                            }
                            else
                            {
                                // If no obstructions, uncrouch
                                MeshRoot.localScale = new Vector3(1f, 1f, 1f);
                                _isCrouching = false;
                            }
                        }
                        break;
                    }
            }
        }

        public void PostGroundingUpdate(float deltaTime)
        {
            // Handle landing and leaving ground
            if (Motor.GroundingStatus.IsStableOnGround && !Motor.LastGroundingStatus.IsStableOnGround)
            {
                OnLanded();
                _jumpsRemaining = Model.MaxJumps; // Reset jumps when landing
            }
            else if (!Motor.GroundingStatus.IsStableOnGround && Motor.LastGroundingStatus.IsStableOnGround)
            {
                OnLeaveStableGround();
            }
        }

        public bool IsColliderValidForCollisions(Collider coll)
        {
            if (IgnoredColliders.Count == 0)
            {
                return true;
            }

            if (IgnoredColliders.Contains(coll))
            {
                return false;
            }

            return true;
        }

        public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
        {
        }

        public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
        {
        }

        public void AddVelocity(Vector3 velocity)
        {
            switch (CurrentCharacterState)
            {
                case CharacterState.Default:
                    {
                        _internalVelocityAdd += velocity;
                        break;
                    }
            }
        }

        public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
        {
        }

        protected void OnLanded()
        {
        }

        protected void OnLeaveStableGround()
        {
        }

        public void OnDiscreteCollisionDetected(Collider hitCollider)
        {
        }

        private void UpdateAnimationParameters()
        {
            if (animator == null) return;

            // Reset all animation parameters
            animator.SetBool(IsWalking, false);
            animator.SetBool(IsDashing, false);
            animator.SetBool(IsJumping, false);
            animator.SetBool(IsFalling, false);
            animator.SetBool(IsIdle, false);

            // Set the appropriate animation parameter based on state
            switch (CurrentCharacterState)
            {
                case CharacterState.Default:
                    if (Motor.GroundingStatus.IsStableOnGround)
                    {
                        if (_moveInputVector.sqrMagnitude > 0.01f)
                        {
                            animator.SetBool(IsWalking, true);
                        }
                        else
                        {
                            animator.SetBool(IsIdle, true);
                        }
                    }
                    else
                    {
                        if (Motor.Velocity.y > 0)
                        {
                            animator.SetBool(IsJumping, true);
                        }
                        else
                        {
                            animator.SetBool(IsFalling, true);
                        }
                    }
                    break;
                case CharacterState.Dashing:
                    animator.SetBool(IsDashing, true);
                    break;
            }
        }

        public void AddExtraDashCharge()
        {
            _extraDashCharges++;
            _hasExtraCharge = true;
        }

        private bool CanDash()
        {
            return _dashCooldownRemaining <= 0f || _hasExtraCharge;
        }

        private void UseDash()
        {
            if (_hasExtraCharge)
            {
                _hasExtraCharge = false;
                _extraDashCharges--;
            }
            else
            {
                _dashCooldownRemaining = Model.DashCooldown;
            }
        }
    }
}
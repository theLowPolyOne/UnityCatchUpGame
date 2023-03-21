using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System;

namespace Fallencake.CharacterController
{
    public class PlayerController : MonoBehaviour, IControllableCharacter
    {
        #region FIELDS

        [SerializeField] private ScriptableStats _stats;

        #region INTERNAL

        [Header("COMPONENTS:")]
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private CapsuleCollider _standingCollider;
        [SerializeField] private CapsuleCollider _crouchingCollider;
        [SerializeField] private PlayerTouchMovement _touchMovementInput;

        private CapsuleCollider _currentCollider; // current active collider
        private PlayerInput _input;
        private bool _cachedTriggerSetting;

        private InputData _frameInput;
        private Vector3 _speed;
        private Vector3 _currentExternalVelocity;
        private int _fixedFrame;
        private bool _hasControl = true;

        #endregion

        #region EXTERNAL

        public event Action<bool, float> GroundedChanged;
        public event Action<bool, Vector2> DashingChanged;
        public event Action<bool> WallGrabChanged;
        public event Action<bool> LedgeClimbChanged;
        public event Action<bool> Jumped;
        public event Action AirJumped;
        public event Action Attacked;
        public ScriptableStats PlayerStats => _stats;
        public Vector2 Input => _frameInput.Move;
        public Vector2 TouchInput => _touchMovementInput.MovementAmount;
        public Vector3 Speed => _speed;
        public Vector3 GroundNormal => _groundNormal;
        public int WallDirection => _wallDir;
        public bool Crouching => _crouching;
        public bool ClimbingLadder => _onLadder;
        public bool GrabbingLedge => _grabbingLedge;
        public bool ClimbingLedge => _climbingLedge;

        public virtual void ApplyVelocity(Vector3 vel, PlayerForce forceType)
        {
            if (forceType == PlayerForce.Burst) _speed += vel;
            else _currentExternalVelocity += vel;
        }

        public virtual void TakeAwayControl(bool resetVelocity = true)
        {
            if (resetVelocity) _rigidbody.velocity = Vector3.zero;
            _hasControl = false;
        }

        public virtual void ReturnControl()
        {
            _speed = Vector3.zero;
            _hasControl = true;
        }

        #endregion

        #endregion

        private void Awake()
        {
            _input = GetComponent<PlayerInput>();
            _cachedTriggerSetting = Physics.queriesHitTriggers;
            ToggleColliders(isStanding: true);
        }

        protected virtual void Update()
        {
            GetInput();
        }

        protected virtual void GetInput()
        {
            _frameInput = _input.InputData;

            if (_frameInput.JumpDown)
            {
                _jumpToConsume = true;
                _frameJumpWasPressed = _fixedFrame;
            }

            if (_frameInput.DashDown && _stats.AllowDash) _dashToConsume = true;
            if (_frameInput.AttackDown && _stats.AllowAttacks) _attackToConsume = true;
        }

        protected virtual void FixedUpdate()
        {
            _fixedFrame++;

            CheckCollisions();
            HandleCollisions();
            //HandleWalls();
            //HandleLedges();
            //HandleLadders();

            //HandleCrouching();
            HandleJump();
            //HandleDash();
            //HandleAttacking();

            HandleHorizontal();
            HandleVertical();
            ApplyMovementVelocity();
            ApplyRotationVelocity();

            //Debug.Log($"INPUT = {Input}");
            //Debug.Log($"_speed = {_speed}");

            //Debug.Log($"Speed = {_rigidbody.velocity.magnitude}");

            //Debug.Log($"_grounded = {_grounded}");
            //Debug.Log($"_groundHitCount = {_groundHitCount}");
        }

        #region Collisions

        private readonly RaycastHit[] _groundHits = new RaycastHit[2];
        private readonly RaycastHit[] _ceilingHits = new RaycastHit[2];
        private readonly Collider[] _wallHits = new Collider[5];
        private readonly Collider[] _ladderHits = new Collider[1];
        private Vector3 _groundNormal;
        private int _groundHitCount;
        private int _ceilingHitCount;
        private int _wallHitCount;
        private int _ladderHitCount;
        private int _frameLeftGrounded = int.MinValue;
        private bool _grounded;

        private Vector3 bottomSphereCenter
        {
            get => _currentCollider.transform.TransformPoint(_currentCollider.center) - Vector3.up * (_currentCollider.height / 2f - _currentCollider.radius);
        }
        private Vector3 topSphereCenter
        {
            get => _currentCollider.transform.TransformPoint(_currentCollider.center) + Vector3.up * (_currentCollider.height / 2f - _currentCollider.radius);
        }

        protected virtual void CheckCollisions()
        {
            // Ground and Ceiling
            _groundHitCount = Physics.CapsuleCastNonAlloc(bottomSphereCenter, topSphereCenter, 
               _currentCollider.radius, Vector3.down, _groundHits, _stats.GrounderDistance, ~_stats.PlayerLayer);
            _ceilingHitCount = Physics.CapsuleCastNonAlloc(bottomSphereCenter, topSphereCenter, 
                _currentCollider.radius, Vector3.up, _ceilingHits, _stats.GrounderDistance, ~_stats.PlayerLayer);

            // Walls and Ladders
            var bounds = GetWallDetectionBounds();
            _wallHitCount = Physics.OverlapBoxNonAlloc(bounds.center, bounds.size / 2f, _wallHits, Quaternion.identity, _stats.ClimbableLayer, QueryTriggerInteraction.UseGlobal);
            _ladderHitCount = Physics.OverlapBoxNonAlloc(bounds.center, bounds.size / 2f, _ladderHits, Quaternion.identity, _stats.LadderLayer, QueryTriggerInteraction.UseGlobal);
        }

        protected virtual bool TryGetGroundNormal(out Vector3 groundNormal)
        {
            Physics.queriesHitTriggers = false;
            var hit = new RaycastHit();
            Physics.Raycast(_rigidbody.position, Vector3.down, out hit, _stats.GrounderDistance * 2, ~_stats.PlayerLayer);
            Physics.queriesHitTriggers = _cachedTriggerSetting;
            groundNormal = hit.normal; // defaults to Vector3.zero if nothing was hit
            return hit.collider != null;
        }

        private Bounds GetWallDetectionBounds()
        {
            var colliderOrigin = _rigidbody.position + _standingCollider.center;
            return new Bounds(colliderOrigin, _stats.WallDetectorSize);
        }

        protected virtual void HandleCollisions()
        {
            // Hit a Ceiling
            if (_ceilingHitCount > 0) _speed.y = Mathf.Min(0, _speed.y);

            // Landed on the Ground
            if (!_grounded && _groundHitCount > 0)
            {
                _grounded = true;
                ResetDash();
                ResetJump();
                GroundedChanged?.Invoke(true, Mathf.Abs(_speed.y));
            }
            // Left the Ground
            else if (_grounded && _groundHitCount == 0)
            {
                _grounded = false;
                _frameLeftGrounded = _fixedFrame;
                GroundedChanged?.Invoke(false, 0);
            }
        }

        #endregion

        #region Walls

        private float _currentWallJumpMoveMultiplier = 1f; // aka "Horizontal input influence"
        private int _wallDir;
        private bool _isOnWall;
        private bool _isLeavingWall; // prevents immediate re-sticking to wall

        protected virtual void HandleWalls()
        {
            if (!_stats.AllowWalls) return;

            _currentWallJumpMoveMultiplier = Mathf.MoveTowards(_currentWallJumpMoveMultiplier, 1f, 1f / _stats.WallJumpInputLossFrames);

            // May need to prioritize the nearest wall here... But who is going to make a climbable wall that tight?
            // TODO: Fix here for TileMaps. Won't be able to use the wall's transform.position. Maybe use collider's center if that will be a thing
            _wallDir = _wallHitCount > 0 ? (int)Mathf.Sign(_wallHits[0].transform.position.x - transform.position.x) : 0;

            if (!_isOnWall && ShouldStickToWall()) SetOnWall(true);
            else if (_isOnWall && !ShouldStickToWall()) SetOnWall(false);

            bool ShouldStickToWall()
            {
                if (_wallDir == 0 || _grounded) return false;
                return _stats.RequireInputPush ? Mathf.Sign(_frameInput.Move.x) == _wallDir : true;
            }
        }

        private void SetOnWall(bool on)
        {
            _isOnWall = on;
            if (on) _speed = Vector2.zero;
            else
            {
                _isLeavingWall = false; // after we've left the wall
                ResetAirJumps(); // so that we can air jump even if we didn't leave via a wall jump
            }
            WallGrabChanged?.Invoke(on);
        }

        #endregion

        #region Ledges

        private Vector3 _ledgeCornerPos;
        private bool _grabbingLedge;
        private bool _climbingLedge;

        protected virtual void HandleLedges()
        {
            if (_climbingLedge || !_isOnWall) return;

            _grabbingLedge = TryGetLedgeCorner(out _ledgeCornerPos);

            if (_grabbingLedge) HandleLedgeGrabbing();
        }

        protected virtual bool TryGetLedgeCorner(out Vector3 cornerPos)
        {
            cornerPos = Vector3.zero;
            Vector3 grabHeight = _rigidbody.position + _stats.LedgeGrabPoint.y * Vector3.up;

            RaycastHit hitInfo1;
            Physics.Raycast(grabHeight + _stats.LedgeRaycastSpacing * Vector3.down, _wallDir * Vector3.right, out hitInfo1, 0.5f, _stats.ClimbableLayer);
            if (!hitInfo1.collider) return false; // Should hit below the ledge. Mainly used to determine xPos accurately

            RaycastHit hitInfo2;
            Physics.Raycast(grabHeight + _stats.LedgeRaycastSpacing * Vector3.up, _wallDir * Vector3.right, out hitInfo2, 0.5f, _stats.ClimbableLayer);
            if (!hitInfo2.collider) return false; // we only are within ledge-grab range when the first hits and second doesn't

            RaycastHit hitInfo3;
            Physics.Raycast(grabHeight + new Vector3(_wallDir * 0.5f, _stats.LedgeRaycastSpacing), Vector3.down, out hitInfo3, 0.5f, _stats.ClimbableLayer);
            if (!hitInfo3.collider) return false; // gets our yPos of the corner

            cornerPos = new(hitInfo1.point.x, hitInfo3.point.y);
            return true;
        }

        protected virtual void HandleLedgeGrabbing()
        {
            // Snap to ledge position
            var xInput = _frameInput.Move.x;
            var yInput = _frameInput.Move.y;
            if (yInput != 0 && (xInput == 0 || Mathf.Sign(xInput) == _wallDir) && _hasControl)
            {
                var pos = _rigidbody.position;
                var targetPos = _ledgeCornerPos - Vector3.Scale(_stats.LedgeGrabPoint, new(_wallDir, 1f));
                _rigidbody.position = Vector2.MoveTowards(pos, targetPos, _stats.LedgeGrabDeceleration * Time.fixedDeltaTime);
            }

            if (yInput > _stats.VerticalDeadzoneThreshold) StartCoroutine(ClimbLedge());
        }

        protected virtual IEnumerator ClimbLedge()
        {
            LedgeClimbChanged?.Invoke(true);
            _climbingLedge = true;

            TakeAwayControl();
            var targetPos = _ledgeCornerPos - Vector3.Scale(_stats.LedgeGrabPoint, new(_wallDir, 1f));
            transform.position = targetPos;

            float lockedUntil = Time.time + _stats.LedgeClimbDuration;
            while (Time.time < lockedUntil)
                yield return new WaitForFixedUpdate();

            _climbingLedge = false;
            _grabbingLedge = false;
            SetOnWall(false);

            targetPos = _ledgeCornerPos + Vector3.Scale(_stats.StandUpOffset, new(_wallDir, 1f));
            transform.position = targetPos;

            ReturnControl();
            LedgeClimbChanged?.Invoke(false);
        }

        #endregion

        #region Ladders

        private Vector2 _ladderSnapVel; // TODO: determine if we need to reset this when leaving a ladder, or use a different kind of Lerp/MoveTowards
        private int _frameLeftLadder = int.MinValue;
        private bool _onLadder;

        private bool CanEnterLadder => _ladderHitCount > 0 && _fixedFrame > _frameLeftLadder + _stats.LadderCooldownFrames;
        private bool MountLadderInputReached => _frameInput.Move.y > _stats.VerticalDeadzoneThreshold || (!_grounded && _frameInput.Move.y < -_stats.VerticalDeadzoneThreshold);
        private bool DismountLadderInputReached => _grounded && _frameInput.Move.y < -_stats.VerticalDeadzoneThreshold;

        protected virtual void HandleLadders()
        {
            if (!_stats.AllowLadders) return;

            if (!_onLadder && CanEnterLadder && MountLadderInputReached) ToggleClimbingLadder(true);
            else if (_onLadder && (_ladderHitCount == 0 || DismountLadderInputReached)) ToggleClimbingLadder(false);

            // Snap to center of ladder
            if (_onLadder && _frameInput.Move.x == 0 && _stats.SnapToLadders && _hasControl)
            {
                var pos = _rigidbody.position;
                // TODO: fix this for TileMap ladders. Cant use the ladder's transform.position
                _rigidbody.position = Vector2.SmoothDamp(pos, new Vector2(_ladderHits[0].transform.position.x, pos.y), ref _ladderSnapVel, _stats.LadderSnapSpeed);
            }
        }

        private void ToggleClimbingLadder(bool on)
        {
            if (_onLadder == on) return;
            if (on) _speed = Vector2.zero;
            else if (_ladderHitCount > 0) _frameLeftLadder = _fixedFrame; // for jumping to prevent immediately re-mounting ladder
            _onLadder = on;
            ResetAirJumps();
        }

        #endregion

        #region Crouching

        private readonly Collider2D[] _crouchHits = new Collider2D[5];
        private int _frameStartedCrouching;
        private bool _crouching;

        protected virtual bool CrouchPressed => _frameInput.Move.y < -_stats.VerticalDeadzoneThreshold;

        protected virtual void HandleCrouching()
        {
            if (!_stats.AllowCrouching) return;

            if (_crouching && _onLadder) ToggleCrouching(false); // use standing collider when on ladder
            if (_crouching != CrouchPressed) ToggleCrouching(!_crouching);
        }

        protected virtual void ToggleCrouching(bool shouldCrouch)
        {
            if (!_crouching && (_isOnWall || (_onLadder && !_grounded))) return; // Prevent crouching if climbing
            if (_crouching && !CanStandUp()) return; // Prevent standing into colliders

            _crouching = shouldCrouch;
            ToggleColliders(!shouldCrouch);
            if (_crouching) _frameStartedCrouching = _fixedFrame;
        }

        protected virtual void ToggleColliders(bool isStanding)
        {
            _currentCollider = isStanding ? _standingCollider : _crouchingCollider;
            _standingCollider.enabled = isStanding;
            _crouchingCollider.enabled = !isStanding;
        }

        protected virtual bool CanStandUp()
        {
            //var topOfHead = _rigidbody.position + _standingCollider.offset + new Vector2(0, 0.5f * _standingCollider.size.y);
            //var size = new Vector2(_standingCollider.size.x, _stats.CrouchBufferCheck);
            Physics2D.queriesHitTriggers = false;
            //var hits = Physics2D.OverlapBoxNonAlloc(topOfHead, size, 0, _crouchHits, ~_stats.PlayerLayer);
            Physics2D.queriesHitTriggers = _cachedTriggerSetting;
            //return hits == 0;
            return false;
        }

        #endregion

        #region Jumping

        private bool _jumpToConsume;
        private bool _endedJumpEarly;
        private bool _coyoteUsable;
        private bool _bufferedJumpUsable;
        private int _frameJumpWasPressed = int.MinValue;
        private int _airJumpsRemaining;

        private bool CanUseCoyote => _coyoteUsable && !_grounded && _fixedFrame < _frameLeftGrounded + _stats.CoyoteFrames;
        private bool HasBufferedJump => _bufferedJumpUsable && _fixedFrame < _frameJumpWasPressed + _stats.JumpBufferFrames;
        private bool CanAirJump => _airJumpsRemaining > 0;

        protected virtual void HandleJump()
        {
            if (_jumpToConsume || HasBufferedJump)
            {
                if (_isOnWall && !_isLeavingWall) WallJump();
                else if (_grounded || _onLadder || CanUseCoyote) NormalJump();
                else if (_jumpToConsume && CanAirJump) AirJump();
            }

            _jumpToConsume = false; // Always consume the flag

            if (!_endedJumpEarly && !_grounded && !_frameInput.JumpHeld && _rigidbody.velocity.y > 0) _endedJumpEarly = true; // Early end detection
        }

        protected virtual void NormalJump()
        { // includes ladder jumps
            _endedJumpEarly = false;
            _bufferedJumpUsable = false;
            _coyoteUsable = false;
            ToggleClimbingLadder(false);
            _speed.y = _stats.JumpPower;
            Jumped?.Invoke(false);
        }

        protected virtual void WallJump()
        {
            _endedJumpEarly = false;
            _bufferedJumpUsable = false;
            _isLeavingWall = true;
            _currentWallJumpMoveMultiplier = 0;
            _speed = Vector3.Scale(_stats.WallJumpPower, new(-_wallDir, 1));
            Jumped?.Invoke(true);
        }

        protected virtual void AirJump()
        {
            _endedJumpEarly = false;
            _airJumpsRemaining--;
            _speed.y = _stats.JumpPower;
            AirJumped?.Invoke();
        }

        protected virtual void ResetJump()
        {
            _coyoteUsable = true;
            _bufferedJumpUsable = true;
            _endedJumpEarly = false;
            ResetAirJumps();
        }

        protected virtual void ResetAirJumps() => _airJumpsRemaining = _stats.MaxAirJumps;

        #endregion

        #region Dashing

        private bool _dashToConsume;
        private bool _canDash;
        private Vector2 _dashVel;
        private bool _dashing;
        private int _startedDashing;

        protected virtual void HandleDash()
        {
            if (_dashToConsume && _canDash && !_crouching)
            {
                var dir = new Vector2(_frameInput.Move.x, Mathf.Max(_frameInput.Move.y, 0f)).normalized;
                if (dir == Vector2.zero)
                {
                    _dashToConsume = false;
                    return;
                }

                _dashVel = dir * _stats.DashVelocity;
                _dashing = true;
                _canDash = false;
                _startedDashing = _fixedFrame;
                DashingChanged?.Invoke(true, dir);

                _currentExternalVelocity = Vector2.zero; // Strip external buildup
            }

            if (_dashing)
            {
                _speed = _dashVel;
                // Cancel when the time is out or we've reached our max safety distance
                if (_fixedFrame > _startedDashing + _stats.DashDurationFrames)
                {
                    _dashing = false;
                    DashingChanged?.Invoke(false, Vector2.zero);
                    _speed.y = Mathf.Min(0, _speed.y);
                    _speed.x *= _stats.DashEndHorizontalMultiplier;
                    if (_grounded) ResetDash();
                }
            }

            _dashToConsume = false;
        }

        protected virtual void ResetDash()
        {
            _canDash = true;
        }

        #endregion

        #region Attacking

        private bool _attackToConsume;
        private int _frameLastAttacked = int.MinValue;

        protected virtual void HandleAttacking()
        {
            if (!_attackToConsume) return;
            // note: animation looks weird if we allow attacking while crouched. consider different attack animations or not allow it while crouched
            if (_fixedFrame > _frameLastAttacked + _stats.AttackFrameCooldown)
            {
                _frameLastAttacked = _fixedFrame;
                Attacked?.Invoke();
            }

            _attackToConsume = false;
        }

        #endregion

        #region Horizontal

        private Vector3 smoothInputVelocity;
        private Vector3 currentInputVector;

        protected virtual void HandleHorizontal()
        {
            if (_dashing) return;
            //var inputX = Input.x * (_onLadder ? _stats.LadderShimmySpeedMultiplier : 1);
            //var inputZ = Input.y * (_onLadder ? _stats.LadderShimmySpeedMultiplier : 1);
            var inputX = TouchInput.x;
            var inputZ = TouchInput.y;
            var inputVector = new Vector3(inputX, 0, inputZ);
            currentInputVector = Vector3.SmoothDamp(currentInputVector, inputVector, ref smoothInputVelocity, 0.1f);
            var moveVector = new Vector3(currentInputVector.x, 0, currentInputVector.z);

            // Deceleration
            if (currentInputVector.magnitude < _stats.HorizontalDeadzoneThreshold)
            {
                _speed = Vector3.MoveTowards(_speed, Vector3.zero,
                    (_grounded ? _stats.GroundDeceleration : _stats.AirDeceleration) * Time.fixedDeltaTime);
            }
            // Crouching
            else if (_crouching && _grounded)
            {
                var crouchPoint = Mathf.InverseLerp(0, _stats.CrouchSlowdownFrames, _fixedFrame - _frameStartedCrouching);
                var diminishedMaxSpeed = _stats.MaxSpeed * Mathf.Lerp(1, _stats.CrouchSpeedPenalty, crouchPoint);
                // TODO: not pressing down while crouched under an object makes you move faster because input is normalized.
                // do we take vertical input into account to re-normalize that along the horiztonal or just leave it be?
                _speed.x = Mathf.MoveTowards(_speed.x, diminishedMaxSpeed * Input.x, 
                    _stats.GroundDeceleration * Time.fixedDeltaTime);
            }
            // Regular Horizontal Movement
            else
            {
                // Prevent useless horizontal speed buildup when against a wall
                if (_wallHitCount > 0 && Mathf.Approximately(_rigidbody.velocity.x, 0) && Mathf.Sign(_frameInput.Move.x) == Mathf.Sign(_speed.x))
                    _speed.x = 0;

                //var inputX = Input.normalized.x * (_onLadder ? _stats.LadderShimmySpeedMultiplier : 1);
                //var inputZ = Input.normalized.z * (_onLadder ? _stats.LadderShimmySpeedMultiplier : 1);
                //var inputVector = new Vector3(inputX, _speed.y, inputZ);
                //Vector3.SmoothDamp(inputVector, Input, ref currentVelocity, 0.2f);
                //var targetSpeed = new Vector3(clampedSpeed.x * _stats.MaxSpeed, _speed.y, clampedSpeed.z * _stats.MaxSpeed);
                //_speed.x = Mathf.MoveTowards(_speed.x, inputX * _stats.MaxSpeed,
                //    _currentWallJumpMoveMultiplier * _stats.Acceleration * Time.fixedDeltaTime);
                //_speed.z = Mathf.MoveTowards(_speed.z, inputZ * _stats.MaxSpeed,
                //    _currentWallJumpMoveMultiplier * _stats.Acceleration * Time.fixedDeltaTime);

                _speed.x = Mathf.MoveTowards(_speed.x, moveVector.x * _stats.MaxSpeed,
                    _currentWallJumpMoveMultiplier * _stats.Acceleration * Time.fixedDeltaTime);
                _speed.z = Mathf.MoveTowards(_speed.z, moveVector.z * _stats.MaxSpeed,
                    _currentWallJumpMoveMultiplier * _stats.Acceleration * Time.fixedDeltaTime);
            }
        }

        #endregion

        #region Vertical

        protected virtual void HandleVertical()
        {
            if (_dashing) return;

            // Ladder
            if (_onLadder)
            {
                var inputY = _frameInput.Move.y;
                _speed.y = inputY * (inputY > 0 ? _stats.LadderClimbSpeed : _stats.LadderSlideSpeed);
            }
            // Grounded & Slopes
            else if (_grounded && _speed.y <= 0f)
            { // TODO: double check this velocity condition. If we're going up a slope, y-speed will be >0
                _speed.y = _stats.GroundingForce;

                if (TryGetGroundNormal(out _groundNormal))
                {
                    if (!Mathf.Approximately(_groundNormal.y, 1f))
                    { // on a slope
                        _speed.y = _speed.x * -_groundNormal.x / _groundNormal.y;
                        if (_speed.x != 0) _speed.y += _stats.GroundingForce;
                    }
                }
            }
            // Wall Climbing & Sliding
            else if (_isOnWall && !_isLeavingWall)
            {
                if (_frameInput.Move.y > 0) _speed.y = _stats.WallClimbSpeed;
                else if (_frameInput.Move.y < 0) _speed.y = -_stats.MaxWallFallSpeed;
                else if (_grabbingLedge) _speed.y = Mathf.MoveTowards(_speed.y, 0, _stats.LedgeGrabDeceleration * Time.fixedDeltaTime);
                else _speed.y = Mathf.MoveTowards(Mathf.Min(_speed.y, 0), -_stats.MaxWallFallSpeed, _stats.WallFallAcceleration * Time.fixedDeltaTime);
            }
            // In Air
            else
            {
                var inAirGravity = _stats.FallAcceleration;
                if (_endedJumpEarly && _speed.y > 0) inAirGravity *= _stats.JumpEndEarlyGravityModifier;
                _speed.y = Mathf.MoveTowards(_speed.y, -_stats.MaxFallSpeed, inAirGravity * Time.fixedDeltaTime);
            }
        }

        #endregion

        protected virtual void ApplyMovementVelocity()
        {
            if (!_hasControl) return;
            _rigidbody.velocity = _speed + _currentExternalVelocity;
            _currentExternalVelocity = Vector3.MoveTowards(_currentExternalVelocity, Vector3.zero,
                _stats.ExternalVelocityDecay * Time.fixedDeltaTime);
        }

        private void ApplyRotationVelocity()
        {
            if (!_hasControl) return;

            // Get the direction of movement
            Vector3 direction = _rigidbody.velocity.normalized;
            if (direction == Vector3.zero) return;

            // Calculate the target rotation
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            targetRotation.eulerAngles = new Vector3(0f, targetRotation.eulerAngles.y, 0f);

            // Check the angle between the current rotation and the target rotation
            float angle = Quaternion.Angle(transform.rotation, targetRotation);
            if (angle > _stats.RotationThreshold)
            {
                float angleDiff = Mathf.DeltaAngle(transform.rotation.eulerAngles.y, targetRotation.eulerAngles.y);
                float angularVelocity = angleDiff * _stats.RotationSpeed / Time.fixedDeltaTime;
                _rigidbody.angularVelocity = Vector3.up * angularVelocity;
            }
            else
            {
                _rigidbody.angularVelocity = Vector3.zero;
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_stats == null) return;

            if (_stats.ShowGroundDetection && _currentCollider != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(bottomSphereCenter, _currentCollider.radius);
                Gizmos.DrawWireSphere(topSphereCenter, _currentCollider.radius);
            }

            if (_stats.ShowWallDetection && _standingCollider != null)
            {
                Gizmos.color = Color.white;
                var bounds = GetWallDetectionBounds();
                Gizmos.DrawWireCube(bounds.center, bounds.size);
            }
            if (_stats.ShowLedgeDetection)
            {
                Gizmos.color = Color.red;
                var facingDir = Mathf.Sign(_wallDir);
                var grabHeight = transform.position + _stats.LedgeGrabPoint.y * Vector3.up;
                var grabPoint = grabHeight + facingDir * _stats.LedgeGrabPoint.x * Vector3.right;
                Gizmos.DrawWireSphere(grabPoint, 0.05f);
                Gizmos.DrawWireSphere(grabPoint + Vector3.Scale(_stats.StandUpOffset, new(facingDir, 1)), 0.05f);
                Gizmos.DrawRay(grabHeight + _stats.LedgeRaycastSpacing * Vector3.down, 0.5f * facingDir * Vector3.right);
                Gizmos.DrawRay(grabHeight + _stats.LedgeRaycastSpacing * Vector3.up, 0.5f * facingDir * Vector3.right);
            }
        }

        private void OnValidate()
        {
            if (_stats == null) Debug.LogWarning("Please assign a ScriptableStats asset to the Player Controller's Stats slot", this);
            if (_standingCollider == null) Debug.LogWarning("Please assign a Capsule Collider to the Standing Collider slot", this);
            if (_crouchingCollider == null) Debug.LogWarning("Please assign a Capsule Collider to the Crouching Collider slot", this);
            if (_rigidbody == null) _rigidbody = GetComponent<Rigidbody>(); // serialized but hidden in the inspector
        }
#endif
    }
}
using System.Collections;
using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(CharacterController))]
    public class Motor : MonoBehaviour
    {
        [System.Serializable]
        public struct States
        {
            public Vector3 Velocity;
            public Vector3 MoveDirection;

            public bool IsGrounded;
            public float MoveSpeed;
            public bool IsSprinting;
            public bool IsJumpDelay;

            public bool IsDodging;
            public int DodgeDirection;  // left = -1 | right = 1

            public bool IsWallSliding;
            public Vector3 WallHitNormal;

            public bool CanWallJump;
            public bool IsWallJumpFrozen;
            public bool IsWallJumping;
            public Vector3 WallJumpDirection;

            public bool IsKnockingBack;
            public bool IsKnockbackWallBounce;
            public Vector3 KnockbackDirection;

            public bool IsWallSlammed;

            public bool IsPushingBack;
            public Vector3 PushbackDirection;

            public bool IsFlinching;
            public bool IsStunned;

            public bool IsAttackMoving;
            public Vector3 AttackMoveDirection;

            public bool Collidable;
            public bool RotatePlayer;
        }
        public States State;

        [Header("References")]
        public CharacterController Controller;
        private Controller PlayerController;
        private Input Input;

        public BasicMovementAttributes B_Attr;      // This whole class can be changed out as a variable to another instance of the class specific to each weapon
        private BasicMovementAttributes original_B_Attr;
        public SpecialMovementAttributes S_Attr;    // We won't normally change this one

        public Stamina Stamina;
        public bool CancelJumpDelay;

        [Header("Private")]
        private float jumpDelayTimer;
        private int jumpAssistTimer;
        private bool jumpedEarly;

        private float dodgeCooldownTimer;
        private float dodgeTimer;

        private float knockbackTimer;
        private float knockbackLength;
        private float knockbackSpeed;
        private float wallSlamLength;

        private int wallBounceCount;

        private float attackMoveTimer;
        private float attackMoveLength;
        private bool attackMoveRotatePlayer;
        private bool attackMoveApplyGravity;
        private float attackMoveTaperPercent;
        private float attackMoveTaperSpeed;

        private float originalControllerRadius;
        private float originalControllerHeight;

        private LayerMask GroundedLayerMask = new LayerMask();

        Vector3 Sphere
        {
            get
            {
                Vector3 p;
                p = transform.position;
                p.y += Controller.radius;
                p.y -= (0.05f * 2);

                return p;
            }
        }
        private bool IsMoving
        {
            get
            {
                return Input.Direction.x != 0.0 || Input.Direction.y != 0.0f;
            }
        }
        private bool IsDodgingLeft
        {
            get
            {
                return State.IsDodging && State.DodgeDirection == -1;
            }
        }
        private bool IsDodgingRight
        {
            get
            {
                return State.IsDodging && State.DodgeDirection == 1;
            }
        }
        private bool IsMovementLocked
        {
            get
            {
                return State.IsKnockingBack || State.IsWallSlammed || State.IsPushingBack || State.IsFlinching || State.IsStunned;
            }
        }
        private bool IsWallJumpStates
        {
            get
            {
                return State.IsWallJumping || State.IsWallJumpFrozen;
            }
        }

        public void Initialize(Input input, Controller playerController)
        {
            // Get Components
            Controller = GetComponent<CharacterController>();

            // Set Components
            Input = input;
            PlayerController = playerController;

            // Set up variables
            original_B_Attr = B_Attr;

            originalControllerRadius = Controller.radius;
            originalControllerHeight = Controller.height;

            GroundedLayerMask = LayerMask.GetMask("Default", "Structure");
            StopAttackMove(0.0f);
        }

        public void HandleChecks()
        {
            // Dodging
            if (dodgeCooldownTimer <= 0.0f
                && !State.IsDodging && !State.IsWallJumpFrozen && !State.IsWallSlammed && !State.IsFlinching && !State.IsAttackMoving 
                && !PlayerController.State.Reloading && (!PlayerController.State.Attacking || (PlayerController.State.Attacking && PlayerController.ActiveWeapon.CanCancel)) && !PlayerController.State.Aiming && (Input.DodgeLeft || Input.DodgeRight))
            {
                /* Dodging Rulset:
                 * - Can when grounded
                 * - Not when dodging
                 * - Not when wall jump frozen
                 * - Can when wall jumping
                 * - Can when knocking back
                 * - Not when wall slammed
                 * - Not when pushing back
                 * - Not when flinching
                 * - Can when stunned
                 * - Not when attack moving
                */

                if (PlayerController.State.Stamina >= Stamina.DodgeStamina)
                {
                    State.MoveSpeed = S_Attr.DodgeSpeed;

                    State.IsDodging = true;
                    if (Input.DodgeLeft)
                        State.DodgeDirection = -1;
                    else if (Input.DodgeRight)
                        State.DodgeDirection = 1;

                    // we use the dodge timer to get a where we are in the dodge
                    // that we we can begin to taper off
                    dodgeTimer = S_Attr.DodgeLength;
                    // set the cooldown timer
                    dodgeCooldownTimer = S_Attr.DodgeCooldownLength;

                    // We do this so that we don't accidentally jump in the process
                    Input.Jump = false;

                    // Subtract stamina for sprinting
                    PlayerController.State.Stamina -= Stamina.DodgeStamina;

                    // Cancel states
                    State.IsStunned = false;
                    // But allow us to wall jump again
                    State.CanWallJump = true;
                }

                // Cancel states
                State.IsKnockingBack = false;
                // A special edge case for wall jumping, since usually we'd still add to the vertical velocity if we don't do this
                State.IsWallJumping = false;
            }
            else if (!State.IsDodging)
                dodgeCooldownTimer -= Time.deltaTime;
        }

        public void HandleMovement()
        {
            if (State.IsAttackMoving)
            {
                SetRadius(0.45f);
                SetCollidable(true);

                // If we've still got time in the attack move
                if (attackMoveTimer > 0)
                {
                    State.MoveSpeed = GameMath.TimerTaper(State.MoveSpeed, 0, attackMoveTimer, attackMoveLength, attackMoveTaperPercent, attackMoveTaperSpeed);
                    // Decrease the attack move timer
                    attackMoveTimer -= Time.deltaTime;

                    // Move the controller
                    State.MoveDirection = new Vector3(State.MoveSpeed * State.AttackMoveDirection.x, State.MoveDirection.y, State.MoveSpeed * State.AttackMoveDirection.z);
                    // Rotate the controller
                    if (attackMoveRotatePlayer)
                    {
                        State.RotatePlayer = true;
                        State.MoveDirection = transform.TransformDirection(State.MoveDirection);
                    }
                    // Apply gravity
                    if (attackMoveApplyGravity)
                        State.MoveDirection.y -= (S_Attr.Gravity) * Time.deltaTime;
                    // Clamp the vertical velocity speed
                    State.MoveDirection.y = Mathf.Clamp(State.MoveDirection.y, -S_Attr.Gravity, 32);
                    // Stop us from being able to wall jump
                    State.CanWallJump = false;
                }
                else
                    State.IsAttackMoving = false;
            }
            else if (State.IsWallJumpFrozen)
            {
                SetRadius(0.425f);
                SetCollidable(false);
                State.RotatePlayer = false;

                if (State.IsStunned || State.IsFlinching || State.IsKnockingBack)
                    State.IsWallJumpFrozen = false;
            }
            else if (State.IsWallJumping)
            {
                SetRadius(0.425f);
                SetCollidable(false);
                State.RotatePlayer = State.MoveDirection.y <= -4;

                /* Wall Jumping Ruleset:
                 * - Not when grounded
                 * - Not when dodging
                 * - Not when wall jump frozen
                 * - Can when wall jumping
                 * - Not when knocking back
                 * - Not when wall slammed
                 * - Not when pushing back
                 * - Not when flinching
                 * - Not when stunned
                 * - Not when attack moving
                 */
                if (State.IsGrounded || State.IsStunned || State.IsFlinching || State.IsKnockingBack || PlayerController.State.Attacking || PlayerController.State.Aiming || Input.Sprint)
                    State.IsWallJumping = false;

                // Reset the move direction to the wall jump direction
                State.MoveDirection = new Vector3(State.WallJumpDirection.x, State.MoveDirection.y, State.WallJumpDirection.z);
                // Rotate the controller
                if (State.MoveDirection.y > -4)
                    transform.rotation = Quaternion.LookRotation(-new Vector3(State.MoveDirection.x, 0, State.MoveDirection.z));

                // If we've begun to lose vertical velocity
                if (State.Velocity.y < 0)
                {
                    Vector3 wallJumpModify = new Vector3(S_Attr.WallJumpMoveDirectionSpeed * Input.DirectionModified.x, 0, S_Attr.WallJumpMoveDirectionSpeed * Input.DirectionModified.y);
                    wallJumpModify = Quaternion.Euler(0, Input.Yaw, 0) * wallJumpModify;
                    State.MoveDirection += wallJumpModify;

                    // Apply gravity
                    State.MoveDirection.y -= S_Attr.Gravity * Time.deltaTime;
                    // Clamp the vertical velocity speed
                    State.MoveDirection.y = Mathf.Clamp(State.MoveDirection.y, -S_Attr.Gravity, 32);
                }
                else
                {
                    // We apply gravity in 2 separate locations here
                    // because we want to decrease our vertically velocity
                    // faster when in a wall jump
                    // then go back to normal gravity when we're no longer going upward
                    // in our wall jump
                    // Apply gravity
                    State.MoveDirection.y -= S_Attr.WallJumpGravity * Time.deltaTime;
                    // Clamp the vertical velocity speed
                    State.MoveDirection.y = Mathf.Clamp(State.MoveDirection.y, -S_Attr.Gravity, 32);
                }
            }
            else if (State.IsDodging)
            {
                SetRadius(0.45f);
                SetCollidable(true);

                if (State.IsStunned || State.IsFlinching || State.IsKnockingBack || (PlayerController.State.Attacking && !PlayerController.ActiveWeapon.CanCancel) || PlayerController.State.Aiming || PlayerController.State.Reloading)
                    State.IsDodging = false;

                // cancel dodge if we press space and not anything else
                bool cancelOnlyJump = Input.Jump && !IsMoving;
                // cancel dodge if we press space and we're moving forward or backward
                bool cancelMoveZ = Input.Jump && Input.Direction.y != 0.0f;
                // Cancel dodge if we press space and we're moving left or right but not forward or back
                //bool cancelMoveZOnly = Input.Jump && Input.Horizontal != 0.0f && Input.Vertical == 0.0f;

                if (State.IsGrounded && (cancelOnlyJump || cancelMoveZ))
                {
                    // Cancel dodging on the ground
                    State.IsDodging = false;
                    // Cancel the jump input
                    Input.Jump = false;

                    // Do a regular jump
                    State.MoveDirection.y = S_Attr.JumpSpeed;
                    // reset the cooldown
                    dodgeCooldownTimer = 0.0f;

                    // This move should cancel jump delay
                    CancelJumpDelay = true;
                }
                else if (!State.IsGrounded && (cancelOnlyJump || cancelMoveZ))
                {
                    // Cancel dodging in the air
                    State.IsDodging = false;
                    // Cancel the jump input
                    Input.Jump = false;

                    // reset the cooldown
                    dodgeCooldownTimer = 0.0f;

                    // This move should cancel jump delay
                    CancelJumpDelay = true;
                }

                // If we've still got time in the dodge
                if (dodgeTimer > 0)
                {
                    // If our dodge time left is less than 30% of the total dodge length -> begin to taper off
                    State.MoveSpeed = GameMath.TimerTaper(State.MoveSpeed, 0, dodgeTimer, 0.3f, S_Attr.DodgeLength, 10);
                    // Decrease the dodge timer
                    dodgeTimer -= Time.deltaTime;

                    // Move the controller
                    State.MoveDirection = new Vector3(State.MoveSpeed * State.DodgeDirection, State.MoveDirection.y, 0);
                    // Rotate the controller
                    State.RotatePlayer = true;
                    State.MoveDirection = transform.TransformDirection(State.MoveDirection);
                    // Apply gravity
                    State.MoveDirection.y -= S_Attr.Gravity * Time.deltaTime;
                    // Clamp the vertical velocity speed
                    State.MoveDirection.y = Mathf.Clamp(State.MoveDirection.y, -S_Attr.Gravity, 32);
                }
                else
                    State.IsDodging = false;
            }
            else if (State.IsKnockingBack)
            {
                SetRadius(0.425f);
                SetCollidable(false);
                State.RotatePlayer = false;

                State.IsFlinching = false;
                State.IsStunned = false;
                State.IsPushingBack = false;
                State.IsWallSlammed = false;

                /* When we're knocking back, we want to keep in this state until
                * - We're grounded and the timer is greater than the length
                * - Or we've hit a wall and wall slammed
                * - Or we've dodged
                */

                // If we've still got time in the knockback
                if (knockbackTimer > 0)
                {
                    State.MoveSpeed = GameMath.TimerTaper(State.MoveSpeed, knockbackSpeed, knockbackTimer, 1.0f, knockbackLength, 15.0f);
                    // If our knockback time left is less than 30% of the total knockback length -> begin to taper off
                    //if (State.IsGrounded)
                    //    State.MoveSpeed = GameMath.TimerTaper(State.MoveSpeed, 0, knockbackTimer, 0.3f, knockbackLength, 20);
                    // Decrease the knockback timer
                    knockbackTimer -= Time.deltaTime;

                    // Move the controller
                    State.MoveDirection = new Vector3(State.MoveSpeed * State.KnockbackDirection.x, State.MoveDirection.y, State.MoveSpeed * State.KnockbackDirection.z);
                    // Apply gravity
                    State.MoveDirection.y -= (S_Attr.Gravity / 2) * Time.deltaTime;
                    // Clamp the vertical velocity speed
                    State.MoveDirection.y = Mathf.Clamp(State.MoveDirection.y, -S_Attr.Gravity, 32);
                }
                else if (!State.IsGrounded)
                {
                    // Apply gravity
                    State.MoveDirection.y -= S_Attr.Gravity * Time.deltaTime;
                }
                else if (State.IsGrounded)
                    State.IsKnockingBack = false;

                if (State.MoveSpeed < 3 && State.IsGrounded)
                    State.IsKnockingBack = false;
            }
            else if (State.IsWallSlammed)
            {
                SetRadius(0.425f);
                SetCollidable(true);
                State.RotatePlayer = false;
                // Stop us from being able to wall jump
                State.CanWallJump = false;

                State.IsFlinching = false;
                State.IsStunned = false;
                State.IsPushingBack = false;
                State.IsKnockingBack = false;
            }
            else if (State.IsFlinching)
            {
                SetRadius(0.35f);
                SetCollidable(true);
                State.RotatePlayer = false;

                State.IsStunned = false;
                State.IsPushingBack = false;
                State.IsWallSlammed = false;
                State.IsKnockingBack = false;

                // Stop the controller
                State.MoveDirection = new Vector3(0, State.MoveDirection.y, 0);
                // Apply gravity
                State.MoveDirection.y -= S_Attr.Gravity * Time.deltaTime;
                // Clamp the vertical velocity speed
                State.MoveDirection.y = Mathf.Clamp(State.MoveDirection.y, -S_Attr.Gravity, 32);
                // Stop us from being able to wall jump
                State.CanWallJump = false;
            }
            else if (State.IsStunned)
            {
                SetRadius(0.425f);
                SetCollidable(true);
                State.RotatePlayer = false;

                State.IsPushingBack = false;
                State.IsWallSlammed = false;
                State.IsKnockingBack = false;
                State.IsFlinching = false;

                // Stop the controller
                State.MoveDirection = new Vector3(0, State.MoveDirection.y, 0);
                // Apply gravity
                State.MoveDirection.y -= S_Attr.Gravity * Time.deltaTime;
                // Clamp the vertical velocity speed
                State.MoveDirection.y = Mathf.Clamp(State.MoveDirection.y, -S_Attr.Gravity, 32);
                // Stop us from being able to wall jump
                State.CanWallJump = false;
            }
            else if (State.IsGrounded)
            {
                SetRadius(0.425f);
                // Reset our coyote timer to the set number of frames
                jumpAssistTimer = S_Attr.JumpAssistFrames;

                // Set speeds
                if (Input.Sprint && !PlayerController.State.Reloading && !PlayerController.State.Attacking && !PlayerController.State.Aiming && PlayerController.State.Stamina >= Stamina.SprintStamina)
                {
                    State.MoveSpeed = B_Attr.SprintSpeed;

                    // Subtract stamina for sprinting
                    PlayerController.State.Stamina -= Stamina.SprintStamina * Time.deltaTime;
                }
                else
                {
                    if (PlayerController.State.Aiming)
                        State.MoveSpeed = B_Attr.AimSpeed;
                    else if (PlayerController.State.Attacking)
                        State.MoveSpeed = B_Attr.AttackSpeed;
                    else
                        State.MoveSpeed = B_Attr.WalkSpeed;

                    // Stop the input if our stamina is less than sprint stamina
                    // This stops a lock effect for when the stamina goes back up a bit
                    if (PlayerController.State.Stamina < Stamina.SprintStamina)
                        Input.Sprint = false;
                }

                // Set step offset
                Controller.stepOffset = 0.5f;

                if (IsMoving && !PlayerController.State.Attacking && !PlayerController.State.Aiming)
                {
                    // If we're on the ground and we're moving
                    // We want to set the radius, but not collide with people
                    //SetRadius(0.425f);
                    SetCollidable(false);
                }
                else
                {
                    //SetRadius(0.3f);
                    SetCollidable(true);
                }

                // Move the player
                State.MoveDirection = new Vector3(State.MoveSpeed * Input.DirectionModified.x, -State.MoveSpeed, State.MoveSpeed * Input.DirectionModified.y);
                // Rotate the controller
                State.RotatePlayer = true;
                State.MoveDirection = transform.TransformDirection(State.MoveDirection);

                // Check if we jump
                if ((Input.Jump || jumpedEarly) && (!State.IsJumpDelay || CancelJumpDelay)/*&& !(PlayerController.State.Attacking && !PlayerController.ActiveWeapon.IsRanged)*/)
                {
                    jumpedEarly = false;
                    CancelJumpDelay = false; // Reset the check so we can use it again at a later time
                    State.MoveDirection.y = S_Attr.JumpSpeed;
                }
                else
                    jumpDelayTimer -= 1 * Time.deltaTime;

                // Allow us to wall jump again
                State.CanWallJump = true;
            }
            else //if (!State.IsGrounded)
            {
                SetRadius(0.425f);

                ///////////////////////////////// Set speeds
                if (Input.Sprint)
                    if (State.MoveDirection.y > 0)
                        State.MoveDirection.y = 0;

                // Apply jump delay
                jumpDelayTimer = S_Attr.JumpDelayLength;

                // Jump assist basically allows the player to jump, even if they're off a ledge, but only for a small number of frames
                // Once it's greater than 0, even when we're off the edge, we want to be able to jump
                if (jumpAssistTimer > 0)
                {
                    jumpAssistTimer -= 1;

                    // Check if we jump
                    if (Input.Jump && (!State.IsJumpDelay || CancelJumpDelay))
                    {
                        jumpedEarly = false;
                        CancelJumpDelay = false; // Reset the check so we can use it again at a later time
                        State.MoveDirection.y = S_Attr.JumpSpeed;
                    }
                }

                // The jump early check allows us to check if a player is about to hit the ground in a short amount of time
                // but they hit the jump button a bit too early
                // If that happens, we want to give them the jump anyway
                if (Input.Jump && Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), (Controller.height / 2) + S_Attr.JumpEarlyDistance, GroundedLayerMask))
                    jumpedEarly = true;

                if (Input.Sprint && !PlayerController.State.Reloading && !PlayerController.State.Attacking && !PlayerController.State.Aiming && PlayerController.State.Stamina >= Stamina.SprintStamina)
                {
                    State.MoveSpeed = B_Attr.SprintSpeed;

                    // Subtract stamina for sprinting
                    PlayerController.State.Stamina -= Stamina.SprintStamina * Time.deltaTime;

                    // This move should cancel jump delay
                    CancelJumpDelay = true;
                }
                else
                {
                    if (State.IsWallSliding)
                        State.MoveSpeed = Mathf.Lerp(State.MoveSpeed, State.MoveSpeed * 0.7f, 5 * Time.deltaTime);
                    else if (PlayerController.State.Aiming)
                        State.MoveSpeed = B_Attr.AimSpeed;
                    else if (PlayerController.State.Attacking)
                        State.MoveSpeed = B_Attr.AttackSpeed;
                    else
                        State.MoveSpeed = B_Attr.WalkSpeed;

                    // Stop the input if our stamina is less than sprint stamina
                    // This stops a lock effect for when the stamina goes back up a bit
                    if (PlayerController.State.Stamina < Stamina.SprintStamina)
                        Input.Sprint = false;
                }
                //////////////////////////////////////////////////
                
                // Set step offset
                Controller.stepOffset = 0.0f;

                if (IsMoving && !PlayerController.State.Attacking && !PlayerController.State.Aiming)
                {
                    // If we're on the ground and we're moving
                    // We want to set the radius, but not collide with people
                    //SetRadius(0.425f);
                    SetCollidable(false);
                }
                else
                {
                    //SetRadius(0.3f);
                    SetCollidable(true);
                }

                // Move the controller
                State.MoveDirection = new Vector3(State.MoveSpeed * Input.DirectionModified.x, State.MoveDirection.y, State.MoveSpeed * Input.DirectionModified.y);
                // Rotate the controller
                State.RotatePlayer = true;
                State.MoveDirection = transform.TransformDirection(State.MoveDirection);
                // Apply gravity
                State.MoveDirection.y -= (State.IsWallSliding ? S_Attr.Gravity / 1.75f : S_Attr.Gravity) * Time.deltaTime;
                // Clamp the vertical velocity speed
                State.MoveDirection.y = Mathf.Clamp(State.MoveDirection.y, -S_Attr.Gravity, 32);
            }

            // The reason we do pushback here
            // is because we want to add it to our regular movement
            // rather than overwriting it
            if (State.IsPushingBack)
            {
                // Add the pushback to the regular movement
                State.MoveDirection += State.PushbackDirection;
            }

            //Controller.Move(State.MoveDirection * Time.deltaTime);
            State.Velocity = Controller.velocity;
            State.IsSprinting = Input.Sprint && State.MoveSpeed > B_Attr.WalkSpeed && !State.IsDodging && !State.IsWallJumping && !State.IsWallJumpFrozen && !State.IsWallSlammed && !State.IsKnockingBack && !State.IsPushingBack && !State.IsStunned && !State.IsFlinching && !State.IsAttackMoving;
            State.IsJumpDelay = jumpDelayTimer > 0.0f;

            //State.IsGrounded = Controller.isGrounded;
            State.IsWallSliding = false;
            if (!State.IsWallSlammed && !State.IsWallJumpFrozen)
                State.IsGrounded = (Controller.Move(State.MoveDirection * Time.deltaTime) & CollisionFlags.Below) != 0;
            //State.IsGrounded = State.IsGrounded || Controller.isGrounded;
            //State.IsGrounded = State.IsGrounded || Physics.CheckSphere(Sphere, Controller.radius, GroundedLayerMask);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), (Controller.height / 2) + S_Attr.JumpEarlyDistance, GroundedLayerMask) ? Color.green : Color.red;
            Gizmos.DrawLine(transform.position, transform.position + (transform.TransformDirection(Vector3.down) * ((Controller.height / 2) + S_Attr.JumpEarlyDistance)));
        }

        public void RotateController()
        {
            if (State.RotatePlayer && PlayerController.Networker.nv_Dead.Value == false)
            {
                //State.MoveDirection = Quaternion.Euler(0, Input.Yaw, 0) * State.MoveDirection;
                transform.localRotation = Quaternion.Euler(0, Input.Yaw, 0);
            }
        }

        public void ResetMotor()
        {
            State.MoveDirection = Vector3.zero;

            State.IsGrounded = false;
            State.IsSprinting = false;

            State.IsDodging = false;

            State.IsWallJumpFrozen = false;
            State.IsWallJumping = false;

            State.IsKnockingBack = false;
            State.IsWallSlammed = false;
            State.IsPushingBack = false;
            State.IsFlinching = false;
            State.IsStunned = false;
            State.IsAttackMoving = false;

            StopAttackMove(0.0f);
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (hit.gameObject.layer != 0 && hit.gameObject.layer != 16)
                return;

            if (State.IsKnockingBack && !State.IsGrounded)
            {
                // If we're grounded, we want to stick to the wall for a few seconds
                // If we're airborne, we want to bounce off
                if (State.IsKnockbackWallBounce && wallBounceCount < 2 && hit.normal.y > -0.1f && hit.normal.y < 0.1f)
                {
                    wallBounceCount++;

                    if (State.MoveSpeed > State.MoveSpeed - 2)
                        State.MoveSpeed -= 2;

                    State.KnockbackDirection = Vector3.Reflect(State.KnockbackDirection, hit.normal);
                }
                else if (hit.normal.y > -0.1f && hit.normal.y < 0.1f)
                {
                    State.IsKnockingBack = false;
                    StartCoroutine(WallSlam(hit.normal, wallSlamLength));
                }
            }
            else
            {
                // Do wall jump checks here
                /* Wall Jump Freeze Ruleset:
                 * - Not when grounded
                 * - Not when dodging
                 * - Not when wall jump frozen
                 * - Not when knocking back
                 * - Not when wall slammed
                 * - Not when pushing back
                 * - Not when flinching
                 * - Not when stunned
                 * - Not when attack moving
                 */

                // If we're falling, then wall slide
                if (State.MoveDirection.y <= -4 && !State.IsGrounded && !PlayerController.State.Attacking && !PlayerController.State.Aiming && !PlayerController.State.Reloading && !State.IsSprinting)
                {
                    State.IsWallSliding = true;
                    State.WallHitNormal = hit.normal;
                }

                if (State.IsGrounded || PlayerController.State.Attacking || PlayerController.State.Aiming || PlayerController.State.Reloading || PlayerController.State.Stamina < Stamina.WallJumpStamina || !State.CanWallJump)
                    return;
                if (IsMovementLocked || State.IsAttackMoving || State.IsWallJumpFrozen || State.IsDodging)
                    return;
                // If it's the roof or floor
                if (hit.normal.y < -0.1f || hit.normal.y > 0.1f)
                    return;
                // To make sure we're not trying to wall jump from above or below the collider
                if (Controller.collisionFlags == CollisionFlags.Above || Controller.collisionFlags == CollisionFlags.Below)
                    return;
                // If we're moving forward into the wall or we're already wall jumping
                //if (Input.Direction.y < 0 && !State.IsWallJumping)
                //    return;
                // Make sure we're not below a certain velocity while chaining
                //if (State.IsWallJumping && State.MoveDirection.y < -5)
                //    return;

                if (Input.Jump)
                    StartCoroutine(WallJumpFreeze(hit.point, hit.normal, new Vector3(State.MoveDirection.x, 0, State.MoveDirection.z)));
            }
        }

        private IEnumerator WallJumpFreeze(Vector3 hitPoint, Vector3 hitNormal, Vector3 dirOnImpact)
        {
            // Send the RPC to all the other players
            // And also execute it locally
            PlayerController.Networker.InvokeServerRpc("WallJump_Server", hitPoint, new Vector2(hitNormal.x, hitNormal.z), channel: "Reliable");
            PlayerController.Networker.WallJump_Client(hitPoint, new Vector2(hitNormal.x, hitNormal.z));

            Quaternion to = Quaternion.LookRotation(-hitNormal);
            transform.rotation = to;

            Vector3 newVel = new Vector3(dirOnImpact.x, 0, dirOnImpact.z);
            Vector3 vel = newVel / newVel.magnitude;
            State.WallJumpDirection = Vector3.Reflect(vel * original_B_Attr.WalkSpeed, hitNormal) * S_Attr.WallJumpSpeed;

            State.MoveDirection = State.WallJumpDirection;

            State.IsWallJumpFrozen = true;
            yield return new WaitForSeconds(S_Attr.WallJumpFrozenLength);
            State.IsWallJumpFrozen = false;

            if (PlayerController.State.Stamina >= Stamina.WallJumpStamina)
            {
                // Subtract stamina for wall jumping
                PlayerController.State.Stamina -= Stamina.WallJumpStamina;

                State.MoveDirection.y = S_Attr.WallJumpHeight;
                State.IsWallJumping = true;
            }
        }

        public void DoKnockback(Vector3 direction, float speed, float length, bool wallBounce = false, float slamLength = 0)
        {
            knockbackTimer = length;
            knockbackLength = length;
            State.KnockbackDirection = direction;

            knockbackSpeed = speed;
            State.MoveSpeed = speed * 2;
            State.MoveDirection = direction;
            State.MoveDirection.y = direction.y * speed;
            State.IsKnockingBack = true;

            State.IsKnockbackWallBounce = wallBounce;
            wallSlamLength = slamLength;

            wallBounceCount = 0;
        }
        private IEnumerator WallSlam(Vector3 hitNormal, float length)
        {
            State.MoveDirection = hitNormal;

            State.IsWallSlammed = true;
            yield return new WaitForSeconds(length);
            State.IsWallSlammed = false;
        }

        public void DoPushback(Vector3 direction, float speed, float length)
        {
            StartCoroutine(Pushback(direction, speed, length));
        }
        private IEnumerator Pushback(Vector3 direction, float speed, float length)
        {
            State.PushbackDirection = direction * speed;

            State.IsPushingBack = true;
            yield return new WaitForSeconds(length);
            State.IsPushingBack = false;
        }

        public void DoFlinch(float length)
        {
            StartCoroutine(Flinch(length));
        }
        private IEnumerator Flinch(float length)
        {
            State.IsFlinching = true;
            yield return new WaitForSeconds(length);
            State.IsFlinching = false;
        }

        public void DoStun(float length)
        {
            StartCoroutine(Stun(length));
        }
        private IEnumerator Stun(float length)
        {
            State.IsStunned = true;
            yield return new WaitForSeconds(length);
            State.IsStunned = false;
        }

        /// <summary>
        /// A special function created to move the player while they're attacking
        /// </summary>
        /// <param name="direction">The direction in which the player will move</param>
        /// <param name="speed">How fast the player will move</param>
        /// <param name="length">The length in seconds that the move will last</param>
        /// <param name="rotatePlayer">Should the player be able to rotate while moving?</param>
        /// <param name="taperStartPercent">What percentage in the move time will the player start to taper off to a stop - example 0.3f for 30%</param>
        /// <param name="taperSpeed">How fast will the lerp taper to 0 - default is 10.0f</param>
        public void DoAttackMove(Vector3 direction, float speed, float length, bool rotatePlayer = true, bool applyGravity = true, float taperStartPercent = 0.3f, float taperSpeed = 10.0f)
        {
            attackMoveTimer = length;
            attackMoveLength = length;
            attackMoveRotatePlayer = rotatePlayer;
            attackMoveApplyGravity = applyGravity;
            attackMoveTaperPercent = taperStartPercent;
            attackMoveTaperSpeed = taperSpeed;
            State.AttackMoveDirection = direction;

            State.MoveSpeed = speed;
            State.MoveDirection = direction;
            State.MoveDirection.y = direction.y * speed;
            State.IsAttackMoving = true;
        }
        /// <summary>
        /// Stop the attack movement that's in progress
        /// </summary>
        /// <param name="time">The time to set the attack move to (incase we don't want to end it right away)</param>
        public void StopAttackMove(float time)
        {
            attackMoveTimer = time;
        }

        /// <summary>
        /// Sets the radius of the character controller
        /// </summary>
        /// <param name="radius"></param>
        public void SetRadius(float radius = 0.0f)
        {
            Controller.radius = radius != 0 ? radius : originalControllerRadius;
        }

        /// <summary>
        /// Sets the layer of the player, whether they can be collided with or not
        /// </summary>
        /// <param name="collidable"></param>
        public void SetCollidable(bool collidable)
        {
            State.Collidable = collidable;
            if (collidable)
                gameObject.layer = 10;
            else
                gameObject.layer = 11;
        }
    }

    [System.Serializable]
    public partial class BasicMovementAttributes
    {
        [Header("Basic Movement")]
        public float WalkSpeed = 6.5f;
        public float SprintSpeed = 11.0f;
        public float AimSpeed = 2.0f;
        public float AttackSpeed = 4.0f;
    }

    [System.Serializable]
    public partial class SpecialMovementAttributes
    {
        [Header("Airborne")]
        public float JumpSpeed = 8.0f;
        public float Gravity = 9.0f;
        public float JumpDelayLength = 1.0f;
        public int JumpAssistFrames = 9;
        public float JumpEarlyDistance = 0.1f;

        [Header("Dodging")]
        public float DodgeSpeed = 11.0f;
        public float DodgeLength = 0.666f;
        public float DodgeCooldownLength = 0.25f;

        [Header("Wall Jumping")]
        public float WallJumpFrozenLength = 0.25f;
        public float WallJumpHeight = 5.0f;
        public float WallJumpGravity = 40.0f;
        public float WallJumpSpeed = 5.0f;
        public float WallJumpMoveDirectionSpeed = 3.0f;
    }

    [System.Serializable]
    public partial class Stamina
    {
        [Header("Special")]
        public float DodgeStamina = 15.0f;
        public float WallJumpStamina = 20.0f;
        public float SprintStamina = 9.25f;
    }
} 
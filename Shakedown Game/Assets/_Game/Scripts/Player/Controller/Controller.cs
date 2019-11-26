using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    public class Controller : MonoBehaviour
    {
        [HideInInspector] public bool isOwner = true;
        [HideInInspector] public bool moveMotor = true;

        [System.Serializable]
        public struct States
        {
            public float Stamina;

            public bool DrawingWeapon;
            public bool Aiming; // Also acts as charging
            public float ChargedAmount;
            public bool Attacking;
            public byte AttackNumber;
            public bool Reloading;

            public byte SelectedWeapon;
        }
        public States State;

        [Header("References")]
        public Input Input;
        public Motor Motor;
        public Camera Camera;
        public Animator Animator;
        public HUD HUD;
        public Networker Networker;

        [Header("Global Positions")]
        public Transform NameTagPosition;

        public Transform CameraPosition;
        public Transform AimTarget;
        public LayerMask AimTargetLayers;

        [Header("Weapons")]
        public byte[] EquippedWeaponsID = new byte[3];
        public List<Weapon> AllWeapons;

        private Weapon currentWeapon;
        public Weapon ActiveWeapon
        {
            get
            {
                if (currentWeapon == null || currentWeapon.WeaponID != EquippedWeaponsID[Networker.nv_SelectedWeapon.Value])
                {
                    for (int i = 0; i < AllWeapons.Count; i++)
                    {
                        if (AllWeapons[i].WeaponID == EquippedWeaponsID[Networker.nv_SelectedWeapon.Value])
                        {
                            currentWeapon = AllWeapons[i];
                            return AllWeapons[i];
                        }
                    }
                }
                else
                {
                    return currentWeapon;
                }

                Debug.LogError("No active weapon!");
                return null;
            }
        }
        private Weapon[] equippedWeapons;
        public Weapon[] EquippedWeapons
        {
            get
            {
                equippedWeapons = new Weapon[3];
                for (int i = 0; i < EquippedWeaponsID.Length; i++)
                    equippedWeapons[i] = GetWeaponByID(EquippedWeaponsID[i]);

                return equippedWeapons;
            }
        }

        [Header("Health/Stamina")]
        public short MaxHealth = 100; // -- THIS NEEDS TO BE REMOVED AND REPLACED WITH THE SKILLS... GOD FUCKING DAMNIT
        public float MaxStamina = 100;
        public float StaminaRegenSpeed = 8.5f;

        [Header("Game Mode Specific")]
        public Transform BallParentPosition;

        private void Start()
        {
            // Setup variables
            //State.Health = MaxHealth;
            State.Stamina = MaxStamina;

            // Subscribe to callbacks
            OnSelectedWeaponChanged += ChangeWeapon;

            // We do this here to avoid any weird reference stuff
            Animator.ModelRefrences.Initialize();
            // Initialize weapons
            for (int i = 0; i < AllWeapons.Count; i++)
                AllWeapons[i].Initialize(this);
            // Disable all of the other weapon game objects
            for (int i = 0; i < AllWeapons.Count; i++)
                AllWeapons[i].Disable();
            // And enable our weapon
            GetWeaponByID(EquippedWeaponsID[SelectedWeapon]).Enable();
            // Setup movement attributes
            Motor.B_Attr = GetWeaponByID(EquippedWeaponsID[SelectedWeapon]).MovementAttributes;

            // Initialize hitboxes
            foreach (Hitbox hitbox in GetComponentsInChildren<Hitbox>())
                hitbox.Controller = this;

            Input.Initialize();
            Motor.Initialize(Input, this);
            Camera.Initialize(Input, this);
            Animator.Initialize(this);
            HUD.Initialize(this);

            // We do this here so that we don't run into any null reference issue fucks
            // As much as I hate hacky code, I don't hate sleep
            Networker.Spawn(Vector4.zero, Networker.nv_SpawnPoint.Value);
        }

        private void Update()
        {
            if (!isOwner)
                return;

            Input.UpdateMouse();
            Camera.UpdateCamera();
            UpdateAimPosition();
            Motor.RotateController();
            HUD.UpdateHUD();
        }

        private void FixedUpdate()
        {
            if (!isOwner)
                return;

            Input.UpdateKeyboard();
            Input.UpdateCombat(ActiveWeapon.HasHeavyAttack);
            if (ActiveWeapon.HasHeavyAttack)
                Input.UpdateHeavyCombat();

            if (!Networker.nv_Dead.Value)
            {
                // If it's Touchdown and the game is in progress or we're the person who scored, then we can move, otherwise, we freeze
                bool td = (Game.Game.Instance.Mode == Game.Game.GameModes.Touchdown && Game.Game.Instance.nv_InProgress.Value) || (Game.Game.Instance.Mode == Game.Game.GameModes.Touchdown && Game.Objects.Ball.Instance.LocalOwner == this);

                if (td)
                {
                    DoLogic();

                    if (moveMotor)
                    {
                        Motor.HandleChecks();
                        Motor.HandleMovement();
                    }
                }
            }

            Animator.HandleAnimations();

            // Regen stamina
            if (!Motor.State.IsSprinting && Game.Objects.Ball.Instance.LocalOwner != this)
                State.Stamina += StaminaRegenSpeed * Time.deltaTime;
            State.Stamina = Mathf.Clamp(State.Stamina, 0, MaxStamina);
        }

        private void UpdateAimPosition()
        {
            RaycastHit hit;
            if (Physics.Raycast(Camera.Cam.transform.position, Camera.Cam.transform.forward, out hit, Mathf.Infinity, AimTargetLayers))
                AimTarget.position = hit.point;
            else
                AimTarget.position = Camera.Cam.transform.position + Camera.Cam.transform.forward * 100.0f;
        }

        private void DoLogic()
        {
            // Change the weapon
            if (!State.Attacking && !State.Aiming)
            {
                if (!Motor.State.IsDodging && !Motor.State.IsKnockingBack && !Motor.State.IsWallSlammed)
                {
                    if (!Motor.State.IsFlinching && !Motor.State.IsStunned && !Motor.State.IsWallJumpFrozen)
                    {
                        SelectedWeapon = Input.WeaponNumber;
                    }
                }
            }

            // Cancel reloading
            if (Motor.State.IsKnockingBack || Motor.State.IsWallSlammed || Motor.State.IsFlinching || Motor.State.IsStunned)
            {
                State.Reloading = false;
            }

            if (State.Attacking)
                // If we're attacking, we don't want to be able to wall jump again
                Motor.State.CanWallJump = false;

            // Update the weapon
            if (!State.DrawingWeapon && !Motor.State.IsKnockingBack && !Motor.State.IsWallSlammed)
            {
                if (!Motor.State.IsFlinching && !Motor.State.IsStunned && !Motor.State.IsWallJumpFrozen)
                    ActiveWeapon.UpdateWeapon();
                else
                {
                    State.Attacking = false;
                    State.Aiming = false;
                }
            }
            else
            {
                State.Attacking = false;
                State.Aiming = false;
            }
        }

        public Weapon GetWeaponByID(byte id)
        {
            for (int i = 0; i < AllWeapons.Count; i++)
            {
                if (AllWeapons[i].WeaponID == id)
                    return AllWeapons[i];
            }

            Debug.Log("Failed to get weapon by ID");
            return AllWeapons[0];
        }

        IEnumerator DrawingWeaponCoroutine;
        private void ChangeWeapon()
        {
            if (DrawingWeaponCoroutine != null)
                StopCoroutine(DrawingWeaponCoroutine);

            DrawingWeaponCoroutine = DrawWeapon();
            StartCoroutine(DrawingWeaponCoroutine);

            // Sound effects
            Animator.DoWeaponSwitch();
            // UI effect
            if (Networker.IsOwner)
                HUD.WeaponSlots.DoScript(SelectedWeapon);
        }

        private IEnumerator DrawWeapon()
        {
            // Disable all of these states
            State.Reloading = false;

            // Disable all of the other weapon game objects
            for (int i = 0; i < AllWeapons.Count; i++)
            {
                AllWeapons[i].Disable();
            }
            // And enable our weapon
            GetWeaponByID(EquippedWeaponsID[SelectedWeapon]).Enable();
            // Setup movement attributes
            Motor.B_Attr = GetWeaponByID(EquippedWeaponsID[SelectedWeapon]).MovementAttributes;

            State.DrawingWeapon = true;
            yield return new WaitForSeconds(0.2f);
            State.DrawingWeapon = false;

            Debug.Log("Changed weapon successfuly");
        }

        #region On Changed Callbacks
        public int SelectedWeapon
        {
            get
            {
                return State.SelectedWeapon;
            }
            set
            {
                if (State.SelectedWeapon == value)
                    return;

                State.SelectedWeapon = (byte)value;
                OnSelectedWeaponChanged();
            }
        }
        public delegate void OnSelectedWeaponChangeDelegate();
        public event OnSelectedWeaponChangeDelegate OnSelectedWeaponChanged;
        #endregion
    }
}
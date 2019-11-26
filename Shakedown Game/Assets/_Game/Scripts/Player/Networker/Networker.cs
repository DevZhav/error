using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkedVar;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Player
{
    public class Networker : NetworkedBehaviour
    {
        public static Networker Instance;

        [Header("References")]
        public Controller Controller;
        public Animator Animator;

        [Header("Game Objects")]
        public GameObject[] DisableObjects;

        [HideInInspector] public bool Initialized = false; // My hacky way of not running any code before the player has been initialized by the server
        [HideInInspector] public bool CanSeePlayer;

        // This is owned by the server
        static NetworkedVarSettings netSettingsReliableSlow = new NetworkedVarSettings() { ReadPermission = NetworkedVarPermission.Everyone, WritePermission = NetworkedVarPermission.ServerOnly, SendChannel = "Reliable", SendTickrate = 0f };
        static NetworkedVarSettings netSettingsHidden = new NetworkedVarSettings() { ReadPermission = NetworkedVarPermission.OwnerOnly, WritePermission = NetworkedVarPermission.ServerOnly, SendChannel = "Reliable", SendTickrate = 0f };
        // These are owned by the clients
        static NetworkedVarSettings netSettingsReliable = new NetworkedVarSettings() { ReadPermission = NetworkedVarPermission.Everyone, WritePermission = NetworkedVarPermission.OwnerOnly, SendChannel = "Reliable", SendTickrate = 0f };
        static NetworkedVarSettings netSettingsUnreliable = new NetworkedVarSettings() { ReadPermission = NetworkedVarPermission.Everyone, WritePermission = NetworkedVarPermission.OwnerOnly, SendChannel = "Unreliable", SendTickrate = 0f };

        // --- AVATAR NETWORKED VARS INFO --- //
        public NetworkedVarByte nv_UserID = new NetworkedVarByte(netSettingsHidden);
        public NetworkedVarByte nv_GameAccess = new NetworkedVarByte(netSettingsHidden);

        public NetworkedVarByte nv_Team = new NetworkedVarByte(netSettingsReliableSlow);
        public NetworkedVarString nv_Name = new NetworkedVarString(netSettingsReliableSlow);
        // --- AVATAR NETWORKED VARS INFO --- //

        // --- AVATAR NETWORKED VARS WEAPONS --- //
        public NetworkedVarByte nv_Weapon1 = new NetworkedVarByte(netSettingsReliableSlow);
        public NetworkedVarByte nv_Weapon2 = new NetworkedVarByte(netSettingsReliableSlow);
        public NetworkedVarByte nv_Weapon3 = new NetworkedVarByte(netSettingsReliableSlow);
        public byte SelectedWeaponID
        {
            get
            {
                byte s = nv_SelectedWeapon.Value;
                switch (s)
                {
                    default:
                        return nv_Weapon1.Value;
                    case 0:
                        return nv_Weapon1.Value;
                    case 1:
                        return nv_Weapon2.Value;
                    case 2:
                        return nv_Weapon3.Value;
                }
            }
        }
        // --- AVATAR NETWORKED VARS WEAPONS --- //

        // --- AVATAR NETWORKED VARS ITEMS --- //
        public NetworkedVarByte nv_Itm_Hair = new NetworkedVarByte(netSettingsReliableSlow);
        public NetworkedVarByte nv_Itm_Face = new NetworkedVarByte(netSettingsReliableSlow);
        public NetworkedVarByte nv_Itm_Top = new NetworkedVarByte(netSettingsReliableSlow);
        public NetworkedVarByte nv_Itm_Bottom = new NetworkedVarByte(netSettingsReliableSlow);
        public NetworkedVarByte nv_Itm_Gloves = new NetworkedVarByte(netSettingsReliableSlow);
        public NetworkedVarByte nv_Itm_Shoes = new NetworkedVarByte(netSettingsReliableSlow);
        public NetworkedVarByte nv_Itm_Pet = new NetworkedVarByte(netSettingsReliableSlow);
        public NetworkedVarByte nv_Itm_TopAccessory = new NetworkedVarByte(netSettingsReliableSlow);
        public NetworkedVarByte nv_Itm_BottomAccessory = new NetworkedVarByte(netSettingsReliableSlow);
        // --- AVATAR NETWORKED VARS ITEMS --- //

        // --- AVATAR NETWORKED VARS SCORE --- //
        public NetworkedVarUShort nv_Score_Eliminations = new NetworkedVarUShort(netSettingsReliableSlow);
        public NetworkedVarInt nv_Score_Damage = new NetworkedVarInt(netSettingsReliableSlow);
        public NetworkedVarByte nv_Score_StocksLeft = new NetworkedVarByte(netSettingsReliableSlow);
        public NetworkedVarUShort nv_Score_Deaths = new NetworkedVarUShort(netSettingsReliableSlow);
        public NetworkedVarUShort nv_Score_Score = new NetworkedVarUShort(netSettingsReliableSlow);
        // --- AVATAR NETWORKED VARS SCORE --- //

        // --- PLAYER NETWORKED VARS --- //
        public NetworkedVarVector4 nv_SpawnPoint = new NetworkedVarVector4(netSettingsReliable);

        public NetworkedVarShort nv_Health = new NetworkedVarShort(new NetworkedVarSettings() { ReadPermission = NetworkedVarPermission.Everyone, WritePermission = NetworkedVarPermission.ServerOnly, SendChannel = "Reliable", SendTickrate = 0 });
        public NetworkedVarShort nv_Stamina = new NetworkedVarShort(netSettingsUnreliable);
        public NetworkedVarBool nv_Dead = new NetworkedVarBool(netSettingsReliable);

        public NetworkedVarByte nv_AttackNumber = new NetworkedVarByte(netSettingsReliable);
        public NetworkedVarShort nv_ChargedAmount = new NetworkedVarShort(netSettingsUnreliable);
        public NetworkedVarByte nv_SelectedWeapon = new NetworkedVarByte(netSettingsReliable);

        public NetworkedVarShort nv_Pitch = new NetworkedVarShort(netSettingsUnreliable);
        public NetworkedVarShort nv_AimPositionX = new NetworkedVarShort(netSettingsUnreliable);
        public NetworkedVarShort nv_AimPositionY = new NetworkedVarShort(netSettingsUnreliable);
        public NetworkedVarShort nv_AimPositionZ = new NetworkedVarShort(netSettingsUnreliable);

        //public NetworkedVarShort nv_WallHitNormalX = new NetworkedVarShort(netSettingsUnreliable);
        //public NetworkedVarShort nv_WallHitNormalZ = new NetworkedVarShort(netSettingsUnreliable);

        public NetworkedVarShort nv_Radius = new NetworkedVarShort(netSettingsUnreliable);
        public NetworkedVarBool nv_Collidable = new NetworkedVarBool(netSettingsUnreliable);
        // --- PLAYER NETWORKED VARS --- //

        // --- BYTES --- //
        public struct Bool_1
        {
            public bool DrawingWeapon;
            public bool Aiming;
            public bool Attacking;
            public bool Reloading;
            public bool IsGrounded;
            public bool IsSprinting;
            public bool IsDodging;
            public bool DodgeDirection;
        }
        public Bool_1 b1;
        bool[] bArray1 = new bool[8];

        public struct Bool_2
        {
            public bool IsWallSliding;
            public bool IsWallJumpFrozen;
            public bool IsWallJumping;
            public bool IsKnockingBack;
            public bool IsWallSlammed;
            public bool IsPushingBack;
            public bool IsFlinching;
            public bool IsStunned;
        }
        public Bool_2 b2;
        bool[] bArray2 = new bool[8];

        public struct Bool_3
        {
            public bool IsJumpDelay;
            public bool fill_1;
            public bool fill_2;
            public bool fill_3;
            public bool fill_4;
            public bool fill_5;
            public bool fill_6;
            public bool fill_7;
        }
        public Bool_3 b3;
        bool[] bArray3 = new bool[8];
        // --- BYTES --- //

        private float PositionUpdatesPerSecond = 60f;
        private float lastSentTime;

        public override void NetworkStart()
        {
            base.NetworkStart();

            LoadPlayer();

            // Setup callbacks
            nv_SpawnPoint.OnValueChanged += Spawn;

            // -- HEALTH
            nv_Health.OnValueChanged += (previousValue, newValue) =>
            {
                if (!IsOwner)
                    return;
                Controller.Camera.SetDanger(previousValue - newValue);
            };
            // -- DEATH
            nv_Dead.OnValueChanged += (previousValue, newValue) =>
            {
                // If we're dead, disable all the hitboxes
                // Otherwise, enable all the hitboxes
                foreach (Hitbox hitbox in GetComponentsInChildren<Hitbox>())
                {
                    hitbox.GetComponent<Collider>().enabled = !newValue;
                    hitbox.enabled = !newValue;
                }
                // And disable the controller so our other players can't collide with it
                Controller.Motor.Controller.enabled = !newValue;

                //if (newValue == false)
                //    Controller.moveMotor = false;
            };
            // -- SELECTED WEAPON
            nv_SelectedWeapon.OnValueChanged += (previousValue, newValue) =>
            {
                // Disable all of the other weapon game objects
                for (int i = 0; i < Controller.AllWeapons.Count; i++)
                {
                    Controller.AllWeapons[i].Disable();
                }
                // And enable our weapon
                Controller.GetWeaponByID(Controller.EquippedWeaponsID[newValue]).Enable();
            };

            nv_Radius.OnValueChanged += (previousValue, newValue) => { Controller.Motor.SetRadius(newValue / 1000); };
            nv_Collidable.OnValueChanged += (previousValue, newValue) => { Controller.Motor.SetCollidable(newValue); };

            // On disconnect
            NetworkingManager.Singleton.OnClientDisconnectCallback += (clientID) =>
            {
                if (clientID == OwnerClientId)
                {
                    NetworkingManager.Singleton.StopClient();
                    SceneManager.LoadScene("Main Menu");
                }
            };
        }

        private void LoadPlayer()
        {
            // Setup variables
            Animator = Controller.Animator;
            Animator.Networker = this;
            Animator.Initialize(Controller);

            if (!IsOwner)
            {

            }
            else
            {
                Instance = this;

                foreach (Hitbox hitbox in GetComponentsInChildren<Hitbox>())
                    hitbox.gameObject.layer = 13;

                Enviro.TeamBasedColor.Instance.UpdateColors(nv_Team.Value);
                Settings.GameSettingsManager.Instance.ApplySettings();
            }

            // Disable / Enable
            Controller.isOwner = IsOwner;

            foreach (GameObject go in DisableObjects)
                go.SetActive(IsOwner);
        }

        private void Update()
        {
            if (!IsOwner)
            {
                Vector3 aimPos = new Vector3(nv_AimPositionX.Value / 10, nv_AimPositionY.Value / 10, nv_AimPositionZ.Value / 10);
                Controller.AimTarget.position = aimPos;

                Controller.transform.position = Vector3.MoveTowards(Controller.transform.position, ReceivedPosition, 15 * Time.deltaTime);
                //Controller.transform.position = Vector3.Lerp(Controller.transform.position, ReceivedPosition, 10 * Time.deltaTime);
                Controller.transform.localEulerAngles = new Vector3(0, ReceivedPosition.w, 0);//Vector3.Lerp(Controller.transform.localEulerAngles, new Vector3(0, ReceivedPosition.w, 0), 25 * Time.deltaTime);

                Controller.SelectedWeapon = nv_SelectedWeapon.Value;

                // Update our animator
                Animator.HandleAnimations();

                // Set a variable to see if our camera can see the active player
                // We do this here so that we don't do this multiple times in different locations (causing more unnecessary calls)
                CanSeePlayer = PlayerMethods.CanSeeObject(Controller.Motor.Controller.bounds);
            }
            else
            {
                // Set our network object variables
                nv_Stamina.Value = (short)(Controller.State.Stamina * 100); // Keep in mind this is divided by 100

                b1.DrawingWeapon = Controller.State.DrawingWeapon;
                b1.Aiming = Controller.State.Aiming;
                nv_ChargedAmount.Value = (short)(Controller.State.ChargedAmount * 100); // Keep in mind this is divided by 100

                b1.Attacking = Controller.State.Attacking;
                nv_AttackNumber.Value = Controller.State.AttackNumber;

                b1.Reloading = Controller.State.Reloading;
                nv_SelectedWeapon.Value = Controller.State.SelectedWeapon;

                nv_Pitch.Value = (short)(Controller.Input.Pitch * 100);
                nv_AimPositionX.Value = (short)(Controller.AimTarget.position.x * 10);
                nv_AimPositionY.Value = (short)(Controller.AimTarget.position.y * 10);
                nv_AimPositionZ.Value = (short)(Controller.AimTarget.position.z * 10);

                b1.IsGrounded = Controller.Motor.State.IsGrounded;
                b1.IsSprinting = Controller.Motor.State.IsSprinting;
                b3.IsJumpDelay = Controller.Motor.State.IsJumpDelay;

                b1.IsDodging = Controller.Motor.State.IsDodging;
                b1.DodgeDirection = Controller.Motor.State.DodgeDirection == 1;  // If our dodge direction is 1, we are dodging right ; else we are dodging left

                b2.IsWallSliding = Controller.Motor.State.IsWallSliding;
                //nv_WallHitNormalX.Value = (short)(Controller.Motor.State.WallHitNormal.x * 100);
                //nv_WallHitNormalZ.Value = (short)(Controller.Motor.State.WallHitNormal.z * 100);

                b2.IsWallJumpFrozen = Controller.Motor.State.IsWallJumpFrozen;
                b2.IsWallJumping = Controller.Motor.State.IsWallJumping;

                b2.IsKnockingBack = Controller.Motor.State.IsKnockingBack;
                b2.IsWallSlammed = Controller.Motor.State.IsWallSlammed;
                b2.IsPushingBack = Controller.Motor.State.IsPushingBack;
                b2.IsFlinching = Controller.Motor.State.IsFlinching;
                b2.IsStunned = Controller.Motor.State.IsStunned;

                nv_Radius.Value = (short)(Controller.Motor.Controller.radius * 1000); // Keep in mind this is divided by 1000
                nv_Collidable.Value = Controller.Motor.State.Collidable;

                if (Time.time - lastSentTime > (1f / PositionUpdatesPerSecond))
                {
                    Vector4 position = Controller.transform.position;
                    position.w = Controller.transform.localEulerAngles.y;

                    short moveDirX = (short)(Controller.Motor.State.MoveDirection.x * 1000);
                    short moveDirY = (short)(Controller.Motor.State.MoveDirection.y * 1000);
                    short moveDirZ = (short)(Controller.Motor.State.MoveDirection.z * 1000);

                    short moveSpeed = (short)(Controller.Motor.State.MoveSpeed * 1000);

                    // Set the bool arrays
                    bArray1 = new bool[8]
                    {
                        b1.DrawingWeapon,
                        b1.Aiming,
                        b1.Attacking,
                        b1.Reloading,
                        b1.IsGrounded,
                        b1.IsSprinting,
                        b1.IsDodging,
                        b1.DodgeDirection
                    };
                    bArray2 = new bool[8]
                    {
                        b2.IsWallSliding,
                        b2.IsWallJumpFrozen,
                        b2.IsWallJumping,
                        b2.IsKnockingBack,
                        b2.IsWallSlammed,
                        b2.IsPushingBack,
                        b2.IsFlinching,
                        b2.IsStunned
                    };
                    bArray3 = new bool[8]
                    {
                        b3.IsJumpDelay,
                        b3.fill_1,
                        b3.fill_2,
                        b3.fill_3,
                        b3.fill_4,
                        b3.fill_5,
                        b3.fill_6,
                        b3.fill_7,
                    };

                    InvokeServerRpc("Move_Server", position.x, position.y, position.z, position.w, moveDirX, moveDirY, moveDirZ, moveSpeed, ConvertBoolArrayToByte(bArray1), ConvertBoolArrayToByte(bArray2), ConvertBoolArrayToByte(bArray3), channel: "Unreliable");
                    lastSentTime = Time.time;
                }
            }
        }

        private static byte ConvertBoolArrayToByte(bool[] source)
        {
            byte result = 0;
            // This assumes the array never contains more than 8 elements!
            int index = 8 - source.Length;

            // Loop through the array
            foreach (bool b in source)
            {
                // if the element is 'true' set the bit at that position
                if (b)
                    result |= (byte)(1 << (7 - index));

                index++;
            }

            return result;
        }

        private static bool[] ConvertByteToBoolArray(byte b)
        {
            // prepare the return result
            bool[] result = new bool[8];

            // check each bit in the byte. if 1 set to true, if 0 set to false
            for (int i = 0; i < 8; i++)
                result[i] = (b & (1 << i)) == 0 ? false : true;

            // reverse the array
            Array.Reverse(result);

            return result;
        }

        Vector4 ReceivedPosition;
        Vector3 ReceivedMoveDirection;
        float MoveSpeed;
        [ClientRPC]
        public void Move_Client(float posX, float posY, float posZ, float rotation, short moveDirectionX, short moveDirectionY, short moveDirectionZ, short moveSpeed, byte byte1, byte byte2, byte byte3)
        {
            ReceivedPosition = new Vector4(posX, posY, posZ, rotation);
            ReceivedMoveDirection = new Vector3(moveDirectionX / 1000, moveDirectionY / 1000, moveDirectionZ / 1000);
            MoveSpeed = moveSpeed / 1000;

            bool[] bArray1 = ConvertByteToBoolArray((byte)byte1);
            b1.DrawingWeapon = bArray1[0];
            b1.Aiming = bArray1[1];
            b1.Attacking = bArray1[2];
            b1.Reloading = bArray1[3];
            b1.IsGrounded = bArray1[4];
            b1.IsSprinting = bArray1[5];
            b1.IsDodging = bArray1[6];
            b1.DodgeDirection = bArray1[7];

            bool[] bArray2 = ConvertByteToBoolArray((byte)byte2);
            b2.IsWallSliding = bArray2[0];
            b2.IsWallJumpFrozen = bArray2[1];
            b2.IsWallJumping = bArray2[2];
            b2.IsKnockingBack = bArray2[3];
            b2.IsWallSlammed = bArray2[4];
            b2.IsPushingBack = bArray2[5];
            b2.IsFlinching = bArray2[6];
            b2.IsStunned = bArray2[7];

            bool[] bArray3 = ConvertByteToBoolArray((byte)byte3);
            b3.IsJumpDelay = bArray3[0];
            b3.fill_1 = bArray3[1];
            b3.fill_2 = bArray3[2];
            b3.fill_3 = bArray3[3];
            b3.fill_4 = bArray3[4];
            b3.fill_5 = bArray3[5];
            b3.fill_6 = bArray3[6];
            b3.fill_7 = bArray3[7];
        }

        public Vector3 GetVelocity()
        {
            if (IsOwner)
                return Controller.Motor.State.MoveDirection;

            return ReceivedMoveDirection;
        }
        public float GetMoveSpeed()
        {
            if (IsOwner)
                return Controller.Motor.State.MoveSpeed;

            return MoveSpeed;
        }

        [ClientRPC]
        public void WallJump_Client(Vector3 point, Vector2 normal)
        {
            Animator.DoWallJump(point, new Vector3(normal.x, 0, normal.y));
        }

        [ClientRPC]
        public void Knockback_Client(Vector3 direction, float speed, float length, bool wallBounce, float slamLength)
        {
            Controller.Motor.DoKnockback(direction, speed, length, wallBounce, slamLength);
        }

        [ClientRPC]
        public void Pushback_Client(Vector3 direction, float speed, float length)
        {
            Controller.Motor.DoPushback(direction, speed, length);
        }

        [ClientRPC]
        public void Flinch_Client(float length)
        {
            Controller.Motor.DoFlinch(length);
        }

        [ClientRPC]
        public void Stun_Client(float length)
        {
            Controller.Motor.DoStun(length);
        }

        public void Spawn(Vector4 oldPoint, Vector4 spawnPoint)
        {
            Debug.Log("Received spawn info");
            // Move the player to the spawn point
            Controller.Motor.Controller.enabled = false;
            Controller.transform.position = new Vector3(spawnPoint.x, spawnPoint.y, spawnPoint.z);
            Controller.Input.Yaw = spawnPoint.w;
            Controller.Motor.Controller.enabled = true;

            Initialized = true;

            ResetPlayer();
        }

        private void ResetPlayer()
        {
            // Reset the motor so the player doesnt move on spawn
            Controller.Motor.ResetMotor();

            // Give the player back his health and stamina :thumbsdown:
            Controller.State.Stamina = Controller.MaxStamina;

            // Allow the player to move again
            Controller.moveMotor = true;

            // Reset all weapona ammo
            for (int i = 0; i < Controller.AllWeapons.Count; i++)
                Controller.AllWeapons[i].CurrentAmmo = Controller.AllWeapons[i].AmmoPerClip;

            // Change the camera target back
            Controller.Camera.FollowPosition = Controller.CameraPosition;
        }

        [ClientRPC]
        public void CastEffects_Client(Vector3 hitPoint, Vector3 hitNormal, byte attackNumber)
        {
            Controller.ActiveWeapon.CastEffects(hitPoint, hitNormal, attackNumber);
        }

        [ClientRPC]
        public void ChatMessage_Client(byte gameAccess = 0, string senderName = "", byte senderTeam = 0, string senderMessage = "")
        {
            ChatFeed.Instance.AddMessageToChat(gameAccess, senderName, senderTeam, senderMessage);
        }
    }
}
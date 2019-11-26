using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkedVar;
using MLAPI.Spawning;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : NetworkedBehaviour
{
    public Transform PlayerViewer;
    private List<Player> DamageDealers = new List<Player>();

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

    public NetworkedVarShort nv_WallHitNormalX = new NetworkedVarShort(netSettingsUnreliable);
    public NetworkedVarShort nv_WallHitNormalZ = new NetworkedVarShort(netSettingsUnreliable);

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
    // --- BYTES --- //

    public byte MaxHealth = 100;
    [HideInInspector] public Vector4 ReceivedPosition;

    private void Start()
    {
        // -- CALLBACKS -- //

        // On player death changed
        nv_Dead.OnValueChanged += (previousValue, newValue) =>
        {
            if (newValue == true)
            {
                Debug.Log("Player has died...");
                nv_Score_Deaths.Value += 1;

                // Award the damage dealers an elimination, then reset it
                for (int i = 0; i < DamageDealers.Count; i++)
                    DamageDealers[i].nv_Score_Eliminations.Value += 1;
                DamageDealers.Clear();

                if (Game.Game.Instance.GameMode == Game.Game.GameModes.TeamDeathMatch)
                {

                }
                else if (Game.Game.Instance.GameMode == Game.Game.GameModes.Touchdown)
                {
                    Transform spawn = Game.Game.Instance.GetSpawn(nv_Team.Value);
                    StartCoroutine(RespawnPlayer(Game.Game.Instance.RespawnTime, spawn.position, spawn.eulerAngles.y));
                }
                else if (Game.Game.Instance.GameMode == Game.Game.GameModes.TeamSmash)
                {

                }
            }
            else
                Debug.Log("Player has respawned");
        };
    }

    public override void NetworkStart()
    {
        base.NetworkStart();
        GetPlayerInfo();
        Debug.Log("<color=yellow>Prefab Hash: " + NetworkedObject.PrefabHash + "</color>");
        Debug.Log("<color=green>Is Scene Object: " + NetworkedObject.NetworkedInstanceId + "</color>");
    }

    private void Update()
    {
        if (!nv_Dead.Value && nv_Health.Value <= 0 && (!b2.IsKnockingBack || b1.IsGrounded || !b2.IsWallSlammed))
        {
            nv_Dead.Value = true;

            if (Game.Game.Instance.GameMode == Game.Game.GameModes.TeamDeathMatch)
            {

            }
            else if (Game.Game.Instance.GameMode == Game.Game.GameModes.Touchdown)
            {
                if (Game.Objects.Ball.Instance.LocalBallOwner == this)
                {
                    Game.Objects.Ball.Instance.LocalDropBall();
                }
            }
            else if (Game.Game.Instance.GameMode == Game.Game.GameModes.TeamSmash)
            {

            }
        }
    }

    [ServerRPC]
    public void Move_Server(float posX, float posY, float posZ, float rotation, short moveDirectionX, short moveDirectionY, short moveDirectionZ, short moveSpeed, byte byte1, byte byte2, byte byte3)
    {
        ReceivedPosition = new Vector4(posX, posY, posZ, rotation);

        bool[] bArray1 = ConvertByteToBoolArray(byte1);
        b1.DrawingWeapon = bArray1[0];
        b1.Aiming = bArray1[1];
        b1.Attacking = bArray1[2];
        b1.Reloading = bArray1[3];
        b1.IsGrounded = bArray1[4];
        b1.IsSprinting = bArray1[5];
        b1.IsDodging = bArray1[6];
        b1.DodgeDirection = bArray1[7];

        bool[] bArray2 = ConvertByteToBoolArray(byte2);
        b2.IsWallSliding = bArray2[0];
        b2.IsWallJumpFrozen = bArray2[1];
        b2.IsWallJumping = bArray2[2];
        b2.IsKnockingBack = bArray2[3];
        b2.IsWallSlammed = bArray2[4];
        b2.IsPushingBack = bArray2[5];
        b2.IsFlinching = bArray2[6];
        b2.IsStunned = bArray2[7];

        bool[] bArray3 = ConvertByteToBoolArray(byte3);
        b3.IsJumpDelay = bArray3[0];
        b3.fill_1 = bArray3[1];
        b3.fill_2 = bArray3[2];
        b3.fill_3 = bArray3[3];
        b3.fill_4 = bArray3[4];
        b3.fill_5 = bArray3[5];
        b3.fill_6 = bArray3[6];
        b3.fill_7 = bArray3[7];

        InvokeClientRpcOnEveryoneExcept("Move_Client", ExecutingRpcSender, posX, posY, posZ, rotation, moveDirectionX, moveDirectionY, moveDirectionZ, moveSpeed, byte1, byte2, byte3, channel: "Unreliable");

        if (Application.isEditor)
            PlayerViewer.position = ReceivedPosition;
    }

    private void GetPlayerInfo()
    {
        // Set user info
        nv_UserID.Value = (byte)Random.Range(0, 9999);
        nv_GameAccess.Value = 1;

        nv_Name.Value = "test " + Random.Range(0, 9999);
        nv_Team.Value = (byte)Random.Range(1, 3);
        //StartCoroutine(DB_API.Server_Match_User_Authorized(DB_API.MatchID, nv_UserID.Value, callback =>
        //{
        //
        //}));

        nv_Health.Value = MaxHealth;
        Spawn();
    }

    private IEnumerator RespawnPlayer(float time, Vector3 spawnPosition, float spawnRotation)
    {
        yield return new WaitForSeconds(time);
        nv_Health.Value = MaxHealth;
        nv_Dead.Value = false;
        Spawn();
        Debug.Log("Player has respawned...");
    }

    public void Spawn()
    {
        // Spawn the player initially
        if (Game.Game.Instance.GameMode == Game.Game.GameModes.TeamDeathMatch)
        {

        }
        else if (Game.Game.Instance.GameMode == Game.Game.GameModes.Touchdown)
        {
            Debug.Log("Spawning Player");
            Transform spawn = Game.Game.Instance.GetSpawn(nv_Team.Value);

            // You must be wondering why I add a random.range below
            // Well let me tell you about my hacky fucks
            // If I didn't do it, I wouldn't get to sleep at night
            // Oh will all the programmers hate my soul if they ever got a hold of this
            nv_SpawnPoint.Value = new Vector4(spawn.position.x, spawn.position.y + Random.Range(0.01f, 0.02f), spawn.position.z, spawn.eulerAngles.y);
        }
        else if (Game.Game.Instance.GameMode == Game.Game.GameModes.TeamSmash)
        {

        }
    }

    [ServerRPC]
    public void WallJump_Server(Vector3 point, Vector2 normal)
    {
        InvokeClientRpcOnEveryoneExcept("WallJump_Client", ExecutingRpcSender, point, normal, channel: "Reliable");
    }

    [ServerRPC(RequireOwnership = false)]
    public void DoDamage(short damage)
    {
        if (Game.Game.Instance.nv_InProgress.Value != true)
            return;

        var p = SpawnManager.GetPlayerObject(ExecutingRpcSender);
        Player player = p.GetComponent<Player>();

        // Add him to the damage dealers
        if (!DamageDealers.Contains(player))
            DamageDealers.Add(player);

        if (nv_Health.Value - damage <= 0 && nv_Health.Value > 0)
        {
            // Award him damage
            // But only if it's not the same person
            if (player != this)
                player.nv_Score_Damage.Value += damage > 0 ? (damage - nv_Health.Value) : 0;
            
            // Send an elimination kill feed
            Game.Game.Instance.SendKillfeedMessage(player.nv_Name.Value, player.nv_Team.Value, nv_Name.Value, nv_Team.Value, player.SelectedWeaponID);
        }
        else if (nv_Health.Value > 0)
        {
            // Award him damage
            // But only if it's not the same person
            if (player != this)
                player.nv_Score_Damage.Value += damage > 0 ? damage : 0;
        }
        
        // Subtract our health
        nv_Health.Value -= damage;
        nv_Health.Value = (short)Mathf.Clamp(nv_Health.Value, 0, MaxHealth);
    }

    [ServerRPC(RequireOwnership = false)]
    public void CastEffects_Server(Vector3 hitPoint, Vector3 hitNormal, byte attackNumber)
    {
        InvokeClientRpcOnEveryoneExcept("CastEffects_Client", ExecutingRpcSender, hitPoint, hitNormal, attackNumber, channel: "Reliable");
    }

    [ServerRPC(RequireOwnership = false)]
    public void Knockback_Server(Vector3 direction, float speed, float length, bool wallBounce, float slamLength)
    {
        InvokeClientRpcOnOwner("Knockback_Client", direction, speed, length, wallBounce, slamLength, channel: "Reliable");
    }

    [ServerRPC(RequireOwnership = false)]
    public void Pushback_Server(Vector3 direction, float speed, float length)
    {
        InvokeClientRpcOnOwner("Pushback_Client", direction, speed, length, channel: "Reliable");
    }

    [ServerRPC(RequireOwnership = false)]
    public void Flinch_Server(float length)
    {
        InvokeClientRpcOnOwner("Flinch_Client", length, channel: "Reliable");
    }

    [ServerRPC(RequireOwnership = false)]
    public void Stun_Server(float length)
    {
        InvokeClientRpcOnOwner("Stun_Client", length, channel: "Reliable");
    }

    [ServerRPC(RequireOwnership = false)]
    public void ChatMessage_Server(string message)
    {
        InvokeClientRpcOnEveryone("ChatMessage_Client", nv_GameAccess.Value, nv_Name.Value, nv_Team.Value, message, channel: "Reliable");
    }

    private static bool[] ConvertByteToBoolArray(byte b)
    {
        // prepare the return result
        bool[] result = new bool[8];

        // check each bit in the byte. if 1 set to true, if 0 set to false
        for (int i = 0; i < 8; i++)
            result[i] = (b & (1 << i)) == 0 ? false : true;

        // reverse the array
        System.Array.Reverse(result);

        return result;
    }
}

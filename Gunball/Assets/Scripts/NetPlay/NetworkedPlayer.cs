using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Gunball.MapObject;

namespace Gunball
{
    public class NetworkedPlayer : NetworkBehaviour
    {
        [SerializeField] GameObject playerCamera;
        [SerializeField] GameObject playerGuide;
        [SerializeField] GameObject playerCineTarget;
        [SerializeField] float NetLerpTime = 0.05f;

        Player _player;
        Vector3 lerpVel;
        Vector3 lerpTarget;
        float lastGroundRotation;

        Vector3 respawnStart;
        Vector3 respawnEnd;
        float respawnTime;
        
        NetworkVariable<NetworkedPlayerState> _netState = new NetworkVariable<NetworkedPlayerState>(readPerm: NetworkVariableReadPermission.Everyone, writePerm: NetworkVariableWritePermission.Owner);
        NetworkedPlayerState PlayerState { get => _netState.Value; set => _netState.Value = value; }
        void Start()
        {
            if (_player != null)
            {
                if (!IsOwner)
                {
                    _player.Velocity = PlayerState.Velocity;
                    lerpVel = _player.Velocity;
                    lerpTarget = PlayerState.Position;
                    transform.position = Vector3.SmoothDamp(transform.position, lerpTarget, ref lerpVel, NetLerpTime);
                    lastGroundRotation = PlayerState.LastGroundRotation;
                }
            }
        }

        public override void OnNetworkSpawn()
        {
            _player = GetComponent<Player>();
            if (!IsOwner)
            {
                Destroy(playerCamera);
                Destroy(playerGuide);
                Destroy(playerCineTarget);
                _player.IsPlayer = false;

                _netState.OnValueChanged += SyncPlayerState;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (IsOwner)
            {
                PlayerState = new NetworkedPlayerState
                {
                    Position = transform.position,
                    Velocity = _player.Velocity,
                    AimingAngle = _player.AimingAngle,
                    LastGroundRotation = _player.VisualModel.transform.rotation.eulerAngles.y,
                    Health = _player.Health,
                    RespawnStartPos = _player.RespawnStart,
                    RespawnEndPos = _player.RespawnEnd,
                    RespawnTime = _player.RespawnTime,
                };
            }
            else
            {
                if (respawnTime > 0)
                {
                    lerpTarget = Vector3.Lerp(respawnStart, respawnEnd, 1f - Mathf.Clamp01(respawnTime));
                    transform.position = Vector3.SmoothDamp(transform.position, lerpTarget, ref lerpVel, NetLerpTime);
                }
                else
                {
                    transform.position = Vector3.SmoothDamp(transform.position, lerpTarget, ref lerpVel, NetLerpTime);
                    Quaternion targetRot = Quaternion.Euler(0, lastGroundRotation, 0);
                    if (_player.IsOnGround)
                    {
                        _player.VisualModel.transform.rotation = Quaternion.Slerp(_player.VisualModel.transform.rotation, targetRot, 90f * Time.deltaTime);
                    }
                    else
                    {
                        targetRot = Player.TiltRotationTowardsVelocity(targetRot, Vector3.up, _player.Velocity, _player.SpeedStat * 32f);
                        _player.VisualModel.transform.rotation = Quaternion.Slerp(_player.VisualModel.transform.rotation, targetRot, 90f * Time.deltaTime);
                    }
                }
            }
        }

        void SyncPlayerState(NetworkedPlayerState lastState, NetworkedPlayerState newState)
        {
            _player.Velocity = newState.Velocity;
            lerpVel = _player.Velocity;
            lerpTarget = newState.Position;
            transform.position = Vector3.SmoothDamp(transform.position, lerpTarget, ref lerpVel, NetLerpTime);
            lastGroundRotation = newState.LastGroundRotation;
            _player.Health = newState.Health;
            respawnStart = newState.RespawnStartPos;
            respawnEnd = newState.RespawnEndPos;
            respawnTime = newState.RespawnTime;

            if (_player.IsDead)
            {
                _player.VisualModel.SetActive(false);
            }
            else
            {
                _player.VisualModel.SetActive(true);
            }
            _player.SetNoClip(_player.IsDead && respawnTime <= 0);
            if (lastState.Health <= 0 && newState.Health > 0)
            {
                transform.position = respawnStart;
                _player.VisualModel.transform.LookAt(respawnEnd);
            }
        }

#region Remote Procedure Calls
        [ServerRpc(RequireOwnership = false)]
        public void SetupKitWeaponsServerRpc(ServerRpcParams serverRpcParams = default)
        {
            var clientId = serverRpcParams.Receive.SenderClientId;
            for (int i = 0; i < _player.WeaponNames.Length; i++)
            {
                GameObject WpGO = GameCoordinator.instance.CreatePlayerWeapon(_player.WeaponNames[i]);
                WpGO.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);
                RegisterKitWeaponClientRpc(i, WpGO.GetComponent<NetworkObject>().NetworkObjectId);
            }
        }

        [ClientRpc]
        public void RegisterKitWeaponClientRpc(int wpNameIndex, ulong wpNetId)
        {
            GameObject WpGO = NetworkManager.SpawnManager.SpawnedObjects[wpNetId].gameObject;
            _player.RegisterKitWeapon(_player.WeaponNames[wpNameIndex], WpGO);
        }

        public void NetInflictDamage(float damage, IShootableObject target)
        {
            InflictDamageServerRpc(damage, target.gameObject.GetComponent<NetworkObject>().NetworkObjectId, gameObject.GetComponent<NetworkObject>().NetworkObjectId);
        }
        [ServerRpc]
        public void InflictDamageServerRpc(float damage, ulong targetNetId, ulong sourceNetId)
        {
            IShootableObject target = NetworkManager.SpawnManager.SpawnedObjects[targetNetId].GetComponent<IShootableObject>();
            if (target != null && !target.IsDead)
                InflictDamageClientRpc(damage, targetNetId, sourceNetId);
        }
        [ClientRpc]
        public void InflictDamageClientRpc(float damage, ulong targetNetId, ulong sourceNetId)
        {
            if (IsOwner) return;
            IShootableObject target = NetworkManager.SpawnManager.SpawnedObjects[targetNetId].GetComponent<IShootableObject>();
            IDamageSource source = NetworkManager.SpawnManager.SpawnedObjects[sourceNetId].GetComponent<IDamageSource>();
            if (target == null || source == null)
            {
                Debug.LogError("Player NetInflictDamage Client RPC: Target or source is null!");
                return;
            }
            target.DoDamage(damage, source);
        }

        public void NetInflictKnockback(Vector3 force, Vector3 pos, float knockbackTimer, IShootableObject target)
        {
            InflictKnockbackServerRpc(force, pos, knockbackTimer, target.gameObject.GetComponent<NetworkObject>().NetworkObjectId, gameObject.GetComponent<NetworkObject>().NetworkObjectId);
        }
        [ServerRpc]
        public void InflictKnockbackServerRpc(Vector3 force, Vector3 pos, float knockbackTimer, ulong targetNetId, ulong sourceNetId)
        {
            InflictKnockbackClientRpc(force, pos, knockbackTimer, targetNetId, sourceNetId);
        }
        [ClientRpc]
        public void InflictKnockbackClientRpc(Vector3 force, Vector3 pos, float knockbackTimer, ulong targetNetId, ulong sourceNetId)
        {
            if (IsOwner) return;
            IShootableObject target = NetworkManager.SpawnManager.SpawnedObjects[targetNetId].GetComponent<IShootableObject>();
            if (target == null)
            {
                Debug.LogError("Player NetInflictKnockback Client RPC: Target or null!");
                return;
            }
            target.SetKnockbackTimer(knockbackTimer);
            target.Knockback(force, pos);
        }

        public void NetInflictHealing(float healing, IShootableObject target)
        {
            InflictHealingServerRpc(healing, target.gameObject.GetComponent<NetworkObject>().NetworkObjectId, gameObject.GetComponent<NetworkObject>().NetworkObjectId);
        }
        [ServerRpc]
        public void InflictHealingServerRpc(float healing, ulong targetNetId, ulong sourceNetId)
        {
            IShootableObject target = NetworkManager.SpawnManager.SpawnedObjects[targetNetId].GetComponent<IShootableObject>();
            if (target != null && !target.IsDead)
                InflictHealingClientRpc(healing, targetNetId, sourceNetId);
        }
        [ClientRpc]
        public void InflictHealingClientRpc(float healing, ulong targetNetId, ulong sourceNetId)
        {
            if (IsOwner) return;
            IShootableObject target = NetworkManager.SpawnManager.SpawnedObjects[targetNetId].GetComponent<IShootableObject>();
            IDamageSource source = NetworkManager.SpawnManager.SpawnedObjects[sourceNetId].GetComponent<IDamageSource>();
            if (target == null || source == null)
            {
                Debug.LogError("Player NetInflictHealing Client RPC: Target or source is null!");
                return;
            }
            target.RecoverDamage(healing, source);
        }
#endregion

        public struct NetworkedPlayerState : INetworkSerializable
        {
            Vector3 position;
            Vector3 velocity;

            short aimingHorizontal;
            short aimingVertical;
            short groundRotationOrAirBasis;

            float health;

            Vector3 respawnStart;
            Vector3 respawnEnd;
            short respawnTime;

            public Vector3 Position { get => position; set => position = value; }
            public Vector3 Velocity { get => velocity; set => velocity = value; }
            public Vector3 AimingAngle { 
                get {
                    return new Vector3(aimingHorizontal, aimingVertical, 0);
                }
                set {
                    aimingHorizontal = (short)value.x;
                    aimingVertical = (short)value.y;
                }
            }

            public float LastGroundRotation { get => groundRotationOrAirBasis; set => groundRotationOrAirBasis = (short)value; }

            public float Health { get => health; set => health = value; }

            public Vector3 RespawnStartPos { get => respawnStart; set => respawnStart = value; }
            public Vector3 RespawnEndPos { get => respawnEnd; set => respawnEnd = value; }
            public float RespawnTime { get => respawnTime; set => respawnTime = (short)value; }

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
            {
                serializer.SerializeValue(ref position);
                serializer.SerializeValue(ref velocity);

                serializer.SerializeValue(ref aimingHorizontal);
                serializer.SerializeValue(ref aimingVertical);
                serializer.SerializeValue(ref groundRotationOrAirBasis);

                serializer.SerializeValue(ref health);

                serializer.SerializeValue(ref respawnStart);
                serializer.SerializeValue(ref respawnEnd);
                serializer.SerializeValue(ref respawnTime);
            }
        }
    }
}
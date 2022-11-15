using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Gunball.MapObject;
using Gunball.WeaponSystem;

namespace Gunball
{
    public class NetworkedWeapon : NetworkBehaviour
    {
        WeaponBase _weapon;
        NetworkVariable<ulong> _ownerId = new NetworkVariable<ulong>(readPerm: NetworkVariableReadPermission.Everyone, writePerm: NetworkVariableWritePermission.Owner);
        public ulong OwnerId { get => _ownerId.Value; set => _ownerId.Value = value; }

        public override void OnNetworkSpawn() {
            _weapon = GetComponent<WeaponBase>();
            _ownerId.OnValueChanged += SyncOwner;

            if (NetworkManager.SpawnManager.SpawnedObjects.ContainsKey(OwnerId))
            {
                GameObject newOwner = NetworkManager.SpawnManager.SpawnedObjects[OwnerId].gameObject;
                if (newOwner != null)
                {
                    _weapon.SetOwner(newOwner);
                }
            }
        }

        public void SetOwner(GameObject owner)
        {
            if (IsOwner)
            {
                OwnerId = owner.GetComponent<NetworkObject>().NetworkObjectId;
            }
        }

        public void SyncOwner(ulong oldOwner, ulong newOwner)
        {
            _weapon.SetOwner(NetworkManager.SpawnManager.SpawnedObjects[newOwner].gameObject);
        }

        public void NetCreateWeaponBullet(Vector3 rootPos, Vector3 spawnPos, Vector3 facing)
        {
            CreateWeaponBulletServerRpc(rootPos, spawnPos, facing, NetworkManager.LocalTime.Time);
        }

        [ServerRpc]
        public void CreateWeaponBulletServerRpc(Vector3 rootPos, Vector3 spawnPos, Vector3 facing, double clientTime)
        {
            CreateWeaponBulletClientRpc(rootPos, spawnPos, facing, clientTime);
        }

        [ClientRpc]
        public void CreateWeaponBulletClientRpc(Vector3 rootPos, Vector3 spawnPos, Vector3 facing, double serverTime)
        {
            if (!IsOwner)
            {
                var delay = serverTime - NetworkManager.ServerTime.Time;
                delay = Mathf.Min((float)delay, 0);
                _weapon.CreateWeaponBullet(rootPos, spawnPos, facing, _weapon.Owner, (float)delay, true);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void SetOwnerServerRpc(ServerRpcParams serverRpcParams = default)
        {
            var clientId = serverRpcParams.Receive.SenderClientId;
            GetComponent<NetworkObject>().ChangeOwnership(clientId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RemoveOwnerServerRpc(ServerRpcParams serverRpcParams = default)
        {
            GetComponent<NetworkObject>().RemoveOwnership();
        }
    }
}
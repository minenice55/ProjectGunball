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

        public override void OnNetworkSpawn() {
            _weapon = GetComponent<WeaponBase>();
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
                _weapon.CreateWeaponBullet(rootPos, spawnPos, facing, _weapon.Owner, (float)delay);
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
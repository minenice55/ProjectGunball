using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Gunball.MapObject;
using Gunball.WeaponSystem;
using Cinemachine;

namespace Gunball
{
    public class NetworkCoordinator : NetworkBehaviour
    {GameCoordinator _gameCoordinator;

        void Start() {
        }
        public override void OnNetworkSpawn()
        {
            _gameCoordinator = GameCoordinator.instance;
            _gameCoordinator.SetNetCoordinator(this);
        }

        [ServerRpc(RequireOwnership = false)]
        public void AssignRespawnPointServerRpc(ServerRpcParams serverRpcParams = default)
        {
            var clientId = serverRpcParams.Receive.SenderClientId;
            if (NetworkManager.ConnectedClients.ContainsKey(clientId))
            {
                int pointNum = _gameCoordinator.assignedRespawnPoints % _gameCoordinator.respawnPoints.Length;
                Vector3 pos = _gameCoordinator.respawnPoints[pointNum].Position;
                Quaternion facing = _gameCoordinator.respawnPoints[pointNum].Facing;

                GameObject rammer = Instantiate(_gameCoordinator.rammerPrefab, pos, facing);
                rammer.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);

                _gameCoordinator.assignedRespawnPoints++;
            }
        }
    }
}

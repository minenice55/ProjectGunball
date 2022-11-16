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
                RespawnPoint point = _gameCoordinator.respawnPoints[pointNum];
                if (pointNum % 2 == 0)
                    point = _gameCoordinator.GetRespawnPointForTeam(ITeamObject.Teams.Alpha, _gameCoordinator.assignedRespawnPoints);
                else
                    point = _gameCoordinator.GetRespawnPointForTeam(ITeamObject.Teams.Bravo, _gameCoordinator.assignedRespawnPoints);

                Vector3 pos = point.Position;
                Quaternion facing = point.Facing;

                GameObject rammer = Instantiate(_gameCoordinator.rammerPrefab, pos, facing);
                rammer.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);
                rammer.GetComponent<NetworkedRespawnRammer>().SetTeamServerRpc((int)point.ObjectTeam);

                _gameCoordinator.assignedRespawnPoints++;
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Gunball.MapObject;
using Gunball.Interface;
using Cinemachine;

namespace Gunball
{
    public class NetworkCoordinator : NetworkBehaviour
    {
        [SerializeField] NetworkObject netScore;
        GameCoordinator _gameCoordinator;

        void Start() {
        }
        public override void OnNetworkSpawn()
        {
            _gameCoordinator = GameCoordinator.instance;
            _gameCoordinator.SetNetCoordinator(this);
            if (IsServer)
            {
                netScore.Spawn();
            }
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

        [ServerRpc]
        public void StartGameServerRpc()
        {
            StartGameClientRpc(NetworkManager.ServerTime.Time);
            _gameCoordinator.ballSpawn.GetComponent<GunBall>().DoDeath();
        }

        [ClientRpc]
        public void StartGameClientRpc(double time)
        {
            _gameCoordinator.SetPlayLayout();
            double timeToWait = time - NetworkManager.ServerTime.Time;
            Player player = NetworkManager.SpawnManager.GetLocalPlayerObject().GetComponent<Player>();
            player.StartGame((float) timeToWait + Time.time + 5f);
            _gameCoordinator.StartBGM();
        }

        [ServerRpc(RequireOwnership = false)]
        public void ConfirmJoinedServerRpc(ServerRpcParams serverRpcParams = default)
        {
            ulong clientId = serverRpcParams.Receive.SenderClientId;
            _gameCoordinator.JoinedPlayers++;
            ConfirmJoinedClientRpc(_gameCoordinator.JoinedPlayers);
            _gameCoordinator.AddRequiredReady(clientId);
        }

        [ClientRpc]
        public void ConfirmJoinedClientRpc(int num)
        {
            _gameCoordinator.JoinedPlayers = num;
        }

        [ServerRpc(RequireOwnership = false)]
        public void ConfirmReadyServerRpc(bool ready, ServerRpcParams serverRpcParams = default)
        {
            ulong clientId = serverRpcParams.Receive.SenderClientId;
            _gameCoordinator.ConfirmReady(ready, clientId);
        }

        [ServerRpc]
        public void EndGameServerRpc()
        {
            EndGameClientRpc();
        }

        [ClientRpc]
        public void EndGameClientRpc()
        {
            _gameCoordinator.EndGame();
        }

        [ServerRpc]
        public void SyncScoreServerRpc(int scoreAlpha, int scoreBravo, int scoringSide)
        {
            SyncScoreClientRpc(scoreAlpha, scoreBravo, scoringSide);
        }
        
        [ClientRpc]
        public void SyncScoreClientRpc(int scoreAlpha, int scoreBravo, int scoringSide)
        {
            var player = NetworkManager.SpawnManager.GetLocalPlayerObject().GetComponent<Player>();
            player.GoalUiSequence((ITeamObject.Teams)scoringSide);
            if (!IsOwner)
            {
                ScoringSystem.instance.SetScores(scoreAlpha, scoreBravo, false);
            }
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Gunball.Interface;
using Gunball.MapObject;
using Cinemachine;

namespace Gunball
{
    public class NetworkedScoringSystem : NetworkBehaviour
    {
        NetworkVariable<NetworkedScoringState> _netState = new NetworkVariable<NetworkedScoringState>(
            readPerm: NetworkVariableReadPermission.Everyone, 
            writePerm: NetworkVariableWritePermission.Server);
        
        NetworkedScoringState scoringState { get => _netState.Value; set => _netState.Value = value; }
        int _alphaScore, _bravoScore;
        public override void OnNetworkSpawn()
        {
            if (!IsServer)
            {
                _netState.OnValueChanged += SyncScoreState;
            }
        }

        public void Update()
        {
            if (IsServer)
            {
                scoringState = new NetworkedScoringState
                {
                    AlphaScore = ScoringSystem.instance.SideAScore,
                    BravoScore = ScoringSystem.instance.SideBScore,
                };
            }
        }

        public void SyncScoreState(NetworkedScoringState oldState, NetworkedScoringState newState)
        {
            _alphaScore = newState.AlphaScore;
            _bravoScore = newState.BravoScore;
            ScoringSystem.instance.SetScores(newState.AlphaScore, newState.BravoScore, false);
        }

        // [ServerRpc]
        // public void AddScoreServerRpc(int side)
        // {
        //     AddScoreClientRpc(side);
        // }

        // [ClientRpc]
        // public void AddScoreClientRpc(int side)
        // {
        //     //get player object
        //     var player = NetworkManager.SpawnManager.GetLocalPlayerObject().GetComponent<Player>();
        //     player.GoalUiSequence((ITeamObject.Teams)side);
        // }

        public struct NetworkedScoringState : INetworkSerializable
        {
            int alphaScore;
            int bravoScore;

            public int AlphaScore { get => alphaScore; set => alphaScore = value; }
            public int BravoScore { get => bravoScore; set => bravoScore = value; }

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
            {
                serializer.SerializeValue(ref alphaScore);
                serializer.SerializeValue(ref bravoScore);
            }
        }
    }
}
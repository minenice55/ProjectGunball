using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Gunball.MapObject;

namespace Gunball
{
    public class NetworkedRespawnRammer : NetworkBehaviour
    {
        [SerializeField] GameObject cutInCam;
        [SerializeField] GameObject aimingCam;
        RespawnRammer _rammer;

        ulong _ownerId;
        Vector3 _aimingAt;
        ITeamObject.Teams _team;

        NetworkVariable<NetworkedRammerState> _netState = new NetworkVariable<NetworkedRammerState>(readPerm: NetworkVariableReadPermission.Everyone, writePerm: NetworkVariableWritePermission.Owner);
        NetworkedRammerState RammerState { get => _netState.Value; set => _netState.Value = value; }

        // Start is called before the first frame update
        void Start()
        {

        }

        public override void OnNetworkSpawn()
        {
            _rammer = GetComponent<RespawnRammer>();
            if (IsOwner)
            {
                GameObject play = NetworkManager.LocalClient.PlayerObject.gameObject;
                _rammer.SetOwner(play);
            }
            else
            {
                _rammer.RemoveCameras();
                _netState.OnValueChanged += SyncState;
                if (NetworkManager.SpawnManager.SpawnedObjects.ContainsKey(RammerState.OwnerId))
                {
                    GameObject newOwner = NetworkManager.SpawnManager.SpawnedObjects[RammerState.OwnerId].gameObject;
                    if (newOwner != null)
                    {
                        _rammer.SetOwner(newOwner);
                        _rammer.SetTeam(_team);
                    }
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (IsOwner)
            {
                RammerState = new NetworkedRammerState
                {
                    AimingAt = _rammer.AimingAt,
                    OwnerId = _ownerId,
                    Team = _rammer.ObjectTeam
                };
            }
            else
            {
                _rammer.VisualModel.transform.LookAt(_aimingAt);
            }
        }

        void SyncState(NetworkedRammerState oldState, NetworkedRammerState newState)
        {
            _aimingAt = newState.AimingAt;
            _ownerId = newState.OwnerId;
            _team = newState.Team;
            if (newState.OwnerId != oldState.OwnerId)
            {
                if (NetworkManager.SpawnManager.SpawnedObjects.ContainsKey(newState.OwnerId))
                {
                    GameObject newOwner = NetworkManager.SpawnManager.SpawnedObjects[newState.OwnerId].gameObject;
                    if (newOwner != null)
                    {
                        _rammer.SetOwner(newOwner);
                        _rammer.SetTeam(_team);
                    }
                }
            }
        }

        public void SetOwner(GameObject owner)
        {
            if (IsOwner)
            {
                _ownerId = owner.GetComponent<NetworkObject>().NetworkObjectId;
            }
        }

        [ServerRpc]
        public void DoRammerPrepareServerRpc()
        {
            DoRammerPrepareClientRpc();
        }

        [ClientRpc]
        public void DoRammerPrepareClientRpc()
        {
            if (!IsOwner)
                _rammer.PlayRammerPrepare();
        }

        [ServerRpc]
        public void DoRammerFireServerRpc()
        {
            DoRammerFireClientRpc();
        }

        [ClientRpc]
        public void DoRammerFireClientRpc()
        {
            if (!IsOwner)
                _rammer.PlayRammerFire();
        }

        [ServerRpc(RequireOwnership = false)]
        public void SetTeamServerRpc(int team, ServerRpcParams serverRpcParams = default)
        {
            if (IsServer)
                SetTeamClientRpc(team);
        }

        [ClientRpc]
        public void SetTeamClientRpc(int team)
        {
            _rammer.SetTeam((ITeamObject.Teams)team);
        }

        public struct NetworkedRammerState : INetworkSerializable
        {
            Vector3 aimingAt;
            ulong ownerId;
            byte team;

            public Vector3 AimingAt { get => aimingAt; set => aimingAt = value; }
            public ulong OwnerId { get => ownerId; set => ownerId = value; }
            public ITeamObject.Teams Team { get => (ITeamObject.Teams)team; set => team = (byte)value; }

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
            {
                serializer.SerializeValue(ref aimingAt);
                serializer.SerializeValue(ref ownerId);
                serializer.SerializeValue(ref team);
            }
        }
    }
}
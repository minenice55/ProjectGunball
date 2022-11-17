using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Gunball.MapObject;

namespace Gunball
{
    public class NetworkedGunball : NetworkBehaviour
    {
        [SerializeField] float NetLerpTime = 0.05f;

        GunBall _gunball;
        bool _isHeld = false;
        ulong _owner, _ownerObject;
        Vector3 lerpVel;
        Vector3 lerpTarget;

        NetworkVariable<NetworkedGunballState> _netState = new NetworkVariable<NetworkedGunballState>(
            readPerm: NetworkVariableReadPermission.Everyone, 
            writePerm: NetworkVariableWritePermission.Server);
        
        NetworkedGunballState BallState { get => _netState.Value; set => _netState.Value = value; }

        public override void OnNetworkSpawn()
        {
            _gunball = GetComponent<GunBall>();
            if (!IsOwner)
            {
                _netState.OnValueChanged += SyncBallState;

                _owner = BallState.OwnerId;
                _isHeld = BallState.IsHeld;
                _ownerObject = BallState.OwnerObject;
            }
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (IsServer)
            {
                if (!NetworkManager.ConnectedClients.ContainsKey(_owner))
                {
                    _isHeld = false;
                    _gunball.DeathDrop();
                    DeathDropClientRpc(_owner);
                }
                BallState = new NetworkedGunballState
                {
                    Position = transform.position,
                    Rotation = transform.rotation,
                    Velocity = _gunball.Velocity,
                    OwnerId = _owner,
                    IsHeld = _isHeld,
                    OwnerObject = NetworkManager.SpawnManager.GetPlayerNetworkObject(_owner).NetworkObjectId
                };
            }
            else
            {
                if (_isHeld)
                {
                    if (NetworkManager.SpawnManager.SpawnedObjects.ContainsKey(_ownerObject))
                    {
                        Player play = NetworkManager.SpawnManager.SpawnedObjects[_ownerObject].gameObject.GetComponent<Player>();
                        transform.position = play.BallPickupPos.position;
                        transform.rotation = play.BallPickupPos.rotation;
                        transform.localScale = play.BallPickupPos.localScale;
                    }
                }
                else
                {
                    transform.position = Vector3.SmoothDamp(transform.position, lerpTarget, ref lerpVel, NetLerpTime);
                }
            }
        }

        void SyncBallState(NetworkedGunballState oldState, NetworkedGunballState newState)
        {
            _gunball.Velocity = newState.Velocity;
            lerpVel = newState.Velocity;
            lerpTarget = newState.Position;
            transform.position = Vector3.SmoothDamp(transform.position, lerpTarget, ref lerpVel, NetLerpTime);
            transform.rotation = newState.Rotation;

            _owner = newState.OwnerId;
            _isHeld = newState.IsHeld;
            _ownerObject = newState.OwnerObject;
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestSetOwnerServerRpc(double pickupTime, ServerRpcParams serverRpcParams = default)
        {
            if (_isHeld) return;
            var clientId = serverRpcParams.Receive.SenderClientId;
            if (NetworkManager.ConnectedClients.ContainsKey(clientId))
            {
                Player play = NetworkManager.SpawnManager.GetPlayerNetworkObject(clientId).GetComponent<Player>();
                if (play.InAction) return;
                _isHeld = true;
                _owner = clientId;
                _gunball.Pickup(play);
                SetOwnerClientRpc(clientId);
            }
        }

        [ClientRpc]
        public void SetOwnerClientRpc(ulong clientId)
        {
            if (!IsServer)
            {
                if (clientId == NetworkManager.LocalClientId)
                {
                    Player play = NetworkManager.SpawnManager.GetLocalPlayerObject().GetComponent<Player>();
                    _gunball.Pickup(play);
                }
            }
        }
        
        [ServerRpc(RequireOwnership = false)]
        public void RequestThrowServerRpc(Vector3 rootPos, Vector3 spawnPos, Vector3 facing, ServerRpcParams serverRpcParams = default)
        {
            if (!_isHeld) return;
            var clientId = serverRpcParams.Receive.SenderClientId;
            if (clientId != _owner) return;
            if (NetworkManager.ConnectedClients.ContainsKey(clientId))
            {
                _isHeld = false;
                _gunball.DoBallThrow(rootPos, spawnPos, facing);
                ThrowClientRpc(rootPos, spawnPos, facing, clientId);
            }
        }

        [ClientRpc]
        public void ThrowClientRpc(Vector3 rootPos, Vector3 spawnPos, Vector3 facing, ulong clientId)
        {
            if (!IsServer)
                _gunball.DoBallThrow(rootPos, spawnPos, facing);
        }


        [ServerRpc(RequireOwnership = false)]
        public void RequestDeathDropServerRpc(ServerRpcParams serverRpcParams = default)
        {
            if (!_isHeld) return;
            var clientId = serverRpcParams.Receive.SenderClientId;
            if (clientId != _owner) return;
            if (NetworkManager.ConnectedClients.ContainsKey(clientId))
            {
                _isHeld = false;
                _gunball.DeathDrop();
                DeathDropClientRpc(clientId);
            }
        }

        [ClientRpc]
        public void DeathDropClientRpc(ulong clientId)
        {
            if (!IsServer)
            {
                if (clientId == NetworkManager.LocalClientId)
                {
                    _gunball.DeathDrop();
                }
            }
        }

        public struct NetworkedGunballState : INetworkSerializable
        {
            Vector3 position;
            Vector3 rotation;
            Vector3 velocity;

            ulong ownerId;
            bool isHeld;

            ulong ownerObject;

            public Vector3 Position { get => position; set => position = value; }
            public Quaternion Rotation { get => Quaternion.Euler(rotation.x, rotation.y, rotation.z); set => rotation = value.eulerAngles; }
            public Vector3 Velocity { get => velocity; set => velocity = value; }

            public ulong OwnerId { get => ownerId; set => ownerId = value; }
            public bool IsHeld { get => isHeld; set => isHeld = value; }
            public ulong OwnerObject { get => ownerObject; set => ownerObject = value; }

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
            {
                serializer.SerializeValue(ref position);
                serializer.SerializeValue(ref rotation);
                serializer.SerializeValue(ref velocity);
                serializer.SerializeValue(ref ownerId);
                serializer.SerializeValue(ref isHeld);
                serializer.SerializeValue(ref ownerObject);
            }
        }
    }
}
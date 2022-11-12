using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Gunball.MapObject;
using Gunball.WeaponSystem;
using Cinemachine;

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
        NetworkVariable<NetworkedPlayerState> _netState = new NetworkVariable<NetworkedPlayerState>(readPerm: NetworkVariableReadPermission.Everyone, writePerm: NetworkVariableWritePermission.Owner);
        NetworkedPlayerState PlayerState { get => _netState.Value; set => _netState.Value = value; }

        // Start is called before the first frame update
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
                };
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

        void SyncPlayerState(NetworkedPlayerState lastState, NetworkedPlayerState newState)
        {
            _player.Velocity = newState.Velocity;
            lerpVel = _player.Velocity;
            lerpTarget = newState.Position;
            transform.position = Vector3.SmoothDamp(transform.position, lerpTarget, ref lerpVel, NetLerpTime);
            lastGroundRotation = newState.LastGroundRotation;
            _player.Health = newState.Health;
            if (_player.IsDead)
            {
                _player.VisualModel.SetActive(false);
            }
            else
            {
                _player.VisualModel.SetActive(true);
            }
        }

        public struct NetworkedPlayerState : INetworkSerializable
        {
            Vector3 position;
            Vector3 velocity;

            short aimingHorizontal;
            short aimingVertical;
            short groundRotationOrAirBasis;

            float health;

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

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
            {
                serializer.SerializeValue(ref position);
                serializer.SerializeValue(ref velocity);

                serializer.SerializeValue(ref aimingHorizontal);
                serializer.SerializeValue(ref aimingVertical);
                serializer.SerializeValue(ref groundRotationOrAirBasis);

                serializer.SerializeValue(ref health);
            }
        }
    }
}
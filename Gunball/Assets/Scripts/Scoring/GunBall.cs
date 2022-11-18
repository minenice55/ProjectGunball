using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gunball.MapObject;
using Gunball.Interface;
using Gunball.WeaponSystem;

namespace Gunball.MapObject
{
    public class GunBall : MonoBehaviour, IShootableObject, IPickup
    {
        static int numSegments = 0;
        static readonly int maxIterations = 10000;
        static readonly int maxSegmentCount = 300;
        static readonly float segmentStepModulo = 10f;

        float _health = 1000f;
        [SerializeField] Vector3 SpawnPos;
        [SerializeField] Rigidbody _rigidbody;
        [SerializeField] TrailRenderer trail;
        [SerializeField] string ballWeaponName = "Wpo_VsBall";

        #region Properties
        public float MaxHealth => 10000f;
        public float Health { get => _health; set { _health = value; } }
        public bool IsDead { get => false; }
        public Transform Transform { get => transform; }
        public IShootableObject.ShootableType Type { get { return _owner == null ? IShootableObject.ShootableType.VsBall : IShootableObject.ShootableType.None; } }

        public Vector3 Velocity { get => _rigidbody.velocity; set => _rigidbody.velocity = value; }
        public float LastThrowTime {set => lastThrowTime = value; }
        public Player Owner { get => _owner; }

        float lastThrowTime = Single.MinValue;
        #endregion

        #region Private Variables
        Player _owner = null;
        Vector3 origScale;

        NetworkedGunball _networkedGunball;
        #endregion

        #region Methods
        void Start()
        {
            origScale = transform.localScale;
            _networkedGunball = GetComponent<NetworkedGunball>();
            GameCoordinator.instance.CreateGlobalWeapon(ballWeaponName);
        }

        void Update()
        {
            if (_networkedGunball != null && !_networkedGunball.IsOwner) return;
            if (_owner != null)
            {
                DoEffect();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_owner != null) return;
            if (Time.time - lastThrowTime < 0.5f) return;
            if (other.tag == "Player")
            {
                if (_networkedGunball == null)
                {
                    Player play = other.gameObject.GetComponent<Player>();
                    DoBallPickup(play);
                }
                else
                {
                    NetworkedPlayer play = other.gameObject.GetComponent<NetworkedPlayer>();
                    if (!play.IsLocalPlayer) return;
                    ulong pid = other.gameObject.GetComponent<NetworkedPlayer>().OwnerClientId;
                    _networkedGunball.RequestSetOwnerServerRpc(pid);
                }
            }
        }

        public void Knockback(Vector3 force, Vector3 pos)
        {
            _rigidbody.AddForceAtPosition(force, pos, ForceMode.Impulse);
        }

        public void SetKnockbackTimer(float time){return;}

        public void DoDamage(float damage, IDamageSource source = null){return;}
        public void RecoverDamage(float healing, IDamageSource source = null){return;}

        public void DoDeath(IDamageSource cause = null)
        {
            if (_owner != null)
            {
                if (_owner != null) { _owner.VsBall = null; _owner.ResetWeapon(); }
                _owner = null;
            }

            transform.rotation = Quaternion.identity;
            transform.localScale = origScale;
            transform.position = SpawnPos;
            _rigidbody.velocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            trail.Clear();
        }

        public void Pickup(Player player)
        {
            _owner = player;
            _owner.VsBall = this;

            _owner.ChangeWeapon(ballWeaponName);
            ((WeaponVsBall)_owner.Weapon).SetBall(this);

            transform.position = _owner.BallPickupPos.position;
            transform.rotation = _owner.BallPickupPos.rotation;
            transform.localScale = _owner.BallPickupPos.localScale;
            _rigidbody.isKinematic = true;
        }

        public void DoEffect()
        {
            transform.position = _owner.BallPickupPos.position;
            transform.rotation = _owner.BallPickupPos.rotation;
            transform.localScale = _owner.BallPickupPos.localScale;
        }

        public void EndEffect()
        {}

        public void CallBallThrow(Vector3 rootPos, Vector3 spawnPos, Vector3 facing)
        {
            if (_networkedGunball != null)
            {
                _networkedGunball.RequestThrowServerRpc(rootPos, spawnPos, facing);
            }
            else
            {
                DoBallThrow(rootPos, spawnPos, facing);
                _owner.ResetWeapon();
                ResetOwner();
            }
        }

        public void DoBallThrow(Vector3 rootPos, Vector3 spawnPos, Vector3 facing)
        {
            if (_owner == null || _owner.Weapon == null) return;
            WeaponVsBall wp = (WeaponVsBall)_owner.Weapon;
            transform.position = spawnPos;
            transform.localScale = origScale;
            _rigidbody.isKinematic = false;

            lastThrowTime = Time.time;

            _rigidbody.velocity = facing * wp.GetSpawnSpeed() + _owner.Velocity;
            _owner.ResetWeapon();
            ResetOwner();
        }

        public void DoBallPickup(Player play)
        {
            if (!play.InAction)
                Pickup(play);
        }

        public void ResetOwner()
        {
            if (_owner != null) _owner.VsBall = null;
            _owner = null;
            _rigidbody.isKinematic = false;
            transform.localScale = origScale;
        }

        public void CallDeathDrop()
        {
            if (_networkedGunball != null)
            {
                _networkedGunball.RequestDeathDropServerRpc();
            }
            else
            {
                DeathDrop();
            }
        }

        public void DeathDrop()
        {
            transform.localScale = origScale;
            _rigidbody.isKinematic = false;

            lastThrowTime = Time.time;

            _rigidbody.velocity = Vector3.up * 8f;
            if (_owner != null) _owner.ResetWeapon();
            ResetOwner();
        }

        public static Vector3 TryBulletMove(Vector3 bulletSpawnPos, Vector3 force, float drag, out Vector3[] segments)
        {
            float timestep = Time.fixedDeltaTime;

            float stepDrag = 1 - drag * timestep;
            Vector3 velocity = force * timestep;
            Vector3 gravity = Physics.gravity * timestep * timestep;
            Vector3 position = bulletSpawnPos;

            segments = new Vector3[maxSegmentCount];

            segments[0] = position;
            numSegments = 1;

            for (int i = 0; i < maxIterations && numSegments < maxSegmentCount; i++)
            {
                velocity += gravity;
                velocity *= stepDrag;

                position += velocity;

                if (i % segmentStepModulo == 0)
                {
                    segments[numSegments] = position;
                    numSegments++;
                }
            }

            return position;
        }
        #endregion
    }
}

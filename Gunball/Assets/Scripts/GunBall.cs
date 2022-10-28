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
        static int maxIterations = 10000;
        static int maxSegmentCount = 300;
        static float segmentStepModulo = 10f;

        float _health = 1000f;
        [SerializeField] Rigidbody _rigidbody;
        [SerializeField] GameObject ballBulletPrefab;

        #region Properties
        public float Health { get => _health; set { _health = 1000f; } }
        public bool IsDead { get => false; }
        public Transform Transform { get => transform; }
        public IShootableObject.ShootableType Type { get { return _owner == null ? IShootableObject.ShootableType.VsBall : IShootableObject.ShootableType.None; } }

        public Player Owner { get => _owner; }

        float lastThrowTime = Single.MinValue;
        #endregion

        #region Private Variables
        Player _owner = null;
        Vector3 origScale;
        #endregion

        #region Methods
        void Start()
        {
            origScale = transform.localScale;
        }

        void Update()
        {
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
                Player play = other.gameObject.GetComponent<Player>();
                if (!play.InAction)
                    Pickup(play);
            }
        }

        public void Knockback(Vector3 force, Vector3 pos)
        {
            _rigidbody.AddForceAtPosition(force, pos, ForceMode.Impulse);
        }

        public void DoDamage(float damage, Player source = null)
        {
            return;
        }
        public void RecoverDamage(float healing, Player source = null)
        {
            return;
        }

        public void Pickup(Player player)
        {
            _owner = player;

            _owner.ChangeWeapon(ballBulletPrefab);
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
        {
            WeaponVsBall wp = (WeaponVsBall)_owner.Weapon;
            transform.position = wp.OverrideSpawnPos();
            transform.localScale = origScale;
            _rigidbody.isKinematic = false;

            lastThrowTime = Time.time;

            _rigidbody.velocity = wp.FacingDirection * wp.GetSpawnSpeed() + _owner.Velocity;
            _owner = null;
        }

        public void ResetOwner()
        {
            _owner = null;
            _rigidbody.isKinematic = false;
            transform.localScale = origScale;
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
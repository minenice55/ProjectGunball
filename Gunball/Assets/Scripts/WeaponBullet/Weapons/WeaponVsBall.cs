using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gunball.WeaponSystem;
using Gunball.MapObject;
namespace Gunball.WeaponSystem
{
    public class WeaponVsBall : WeaponBase
    {
        [SerializeField] GuideParam GuidePrm;
        [SerializeField] CollisionParam ColPrm;
        [SerializeField] MoveSimpleParam MovePrm;
        [SerializeField] ChargeParam ChgPrm;

        public GunBall ball;

        public void SetBall(GunBall b)
        {
            ball = b;
        }

        public float GetSpawnSpeed()
        {
            return MovePrm.SpawnSpeed;
        }

        public override bool RefireCheck(float heldDuration, float relaxDuration, WeaponParam wpPrm)
        {
            if (heldDuration <= 0 && relaxDuration > 0)
            {
                nextFireTime = 0;
                return false;
            }
            lastActionTime = Time.time;
            if (heldDuration >= nextFireTime)
            {
                nextFireTime = heldDuration + wpPrm.RepeatTime;
                return true;
            }
            return false;
        }

        public override GuideType GetGuideType()
        {
            return GuidePrm.ShotGuideType;
        }

        public override Vector3[] GetGuideCastPoints()
        {
            Vector3[] castPoints;
            Vector3 currPos = GunBall.TryBulletMove(OverrideSpawnPos(), facingDirection * MovePrm.SpawnSpeed + owner.Velocity, BulletObject.GetComponent<Rigidbody>().drag, out castPoints);

            return castPoints;
        }

        public override LayerMask GetGuideCollisionMask()
        {
            return ColPrm.CollisionMask;
        }

        public override float GetGuideRadius() { return GuidePrm.GuideRadius; }
        public override float GetGuideWidth() { return GuidePrm.GuideWidth; }

        public override Vector3 OverrideSpawnPos() { return owner.BallSpawnPos.position; }

        public override void CreateWeaponBullet(Vector3 rootPos, Vector3 spawnPos, Vector3 facing, Player player, float postDelay = 0, bool visualOnly = false)
        {
            //todo: make ball compensate for net delay?
            ball.EndEffect();
        }

        public override IEnumerator DoFireSequence(float delay, Player player)
        {
            player.InFireCoroutine = true;
            if (delay > 0)
                yield return new WaitForSeconds(delay);
            
            if (_netWeapon != null)
            {
                if (_netWeapon.IsOwner)
                    _netWeapon.NetCreateWeaponBullet(RootSpawnPos.position, BulletSpawnPos.position, facingDirection);
            }
            else
                CreateWeaponBullet(RootSpawnPos.position, BulletSpawnPos.position, facingDirection, player);
            player.VsBall = null;
            ball.ResetOwner();
            player.ResetWeapon();
            player.InFireCoroutine = false;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gunball.WeaponSystem;
using Gunball.MapObject;
namespace Gunball.WeaponSystem
{
    public class WeaponVsBall : WeaponBulletMgr
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

        public override void CreateWeaponBullet(Player player)
        {
            ball.EndEffect();
        }

        public override IEnumerator DoFireSequence(Player player)
        {
            player.InFireCoroutine = true;
            yield return new WaitForSeconds(WpPrm.PreDelayTime);
            CreateWeaponBullet(player);

            ball.ResetOwner();
            player.ResetWeapon();
            player.InFireCoroutine = false;
        }
    }
}
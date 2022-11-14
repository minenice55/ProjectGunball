using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gunball.WeaponSystem;
using Gunball.MapObject;
namespace Gunball.WeaponSystem
{
    public class WeaponBlastDebug : WeaponBase
    {
        [SerializeField] GuideParam GuidePrm;
        [SerializeField] CollisionParam ColPrm;
        [SerializeField] MoveBlastParam BlastPrm;
        [SerializeField] DamageParam DmgPrm;

        MoveSimpleParam MovePrm { get { return BlastPrm.MoveSimpleParam; } }

        public override GuideType GetGuideType()
        {
            return GuidePrm.ShotGuideType;
        }

        public override Vector3[] GetGuideCastPoints()
        {
            float counter = Mathf.Min(MovePrm.ToGravityTime, GuidePrm.ShotGuideSecs);
            List<Vector3> points = new List<Vector3>();
            Vector3 currPos = BulletSpawnPos.position;
            Vector3[] castPoints;
            currPos = BulletBase.TryBulletMove(BulletSpawnPos.position, RootSpawnPos.position, currPos, facingDirection, 0, counter, MovePrm, out castPoints);
            foreach (Vector3 p in castPoints)
            {
                points.Add(p);
            }

            while (counter < GuidePrm.ShotGuideSecs)
            {
                currPos = BulletBase.TryBulletMove(BulletSpawnPos.position, RootSpawnPos.position, currPos, facingDirection, counter, STEP_TIME, MovePrm, out castPoints);
                foreach (Vector3 p in castPoints)
                {
                    points.Add(p);
                }
                counter += STEP_TIME;
            }
            return points.ToArray();
        }

        public override LayerMask GetGuideCollisionMask()
        {
            return ColPrm.CollisionMask;
        }

        public override float GetGuideRadius() { return GuidePrm.GuideRadius; }

        public override void CreateWeaponBullet(Vector3 rootPos, Vector3 spawnPos, Vector3 facing, Player player, float postDelay = 0, bool visualOnly = false)
        {
            GameObject bullet = Instantiate(BulletObject, spawnPos, Quaternion.identity);
            BulletRocket bulletBase = bullet.GetComponent<BulletRocket>();
            bulletBase.SetupBullet(spawnPos, rootPos, facing, player, ColPrm, BlastPrm, DmgPrm, postDelay, visualOnly);
            bullet.SetActive(true);
        }
    }
}
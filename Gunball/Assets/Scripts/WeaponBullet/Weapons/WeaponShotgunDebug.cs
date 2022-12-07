using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gunball.MapObject;

namespace Gunball.WeaponSystem
{
    public class WeaponShotgunDebug : WeaponBase
    {
        [SerializeField] ShotgunParam ShotgunPrm;
        [SerializeField] GuideParam GuidePrm;
        [SerializeField] MoveSimpleParam GuideMovePrm;
        [SerializeField] CollisionParam GuideColParam;

        public override GuideType GetGuideType()
        {
            return GuidePrm.ShotGuideType;
        }

        public override Vector3[] GetGuideCastPoints()
        {
            float counter = Mathf.Min(GuideMovePrm.ToGravityTime, GuidePrm.ShotGuideSecs);
            List<Vector3> points = new List<Vector3>();
            Vector3 currPos = BulletSpawnPos.position;
            Vector3[] castPoints;
            currPos = BulletBase.TryBulletMove(BulletSpawnPos.position, RootSpawnPos.position, currPos, facingDirection, 0, counter, GuideMovePrm, out castPoints);
            foreach (Vector3 p in castPoints)
            {
                points.Add(p);
            }

            while (counter < GuidePrm.ShotGuideSecs)
            {
                currPos = BulletBase.TryBulletMove(BulletSpawnPos.position, RootSpawnPos.position, currPos, facingDirection, counter, STEP_TIME, GuideMovePrm, out castPoints);
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
            return GuideColParam.CollisionMask;
        }

        public override float GetGuideRadius() { return GuidePrm.GuideRadius; }

        public override void CreateWeaponBullet(Vector3 rootPos, Vector3 spawnPos, Vector3 facing, Player player, float postDelay = 0, bool visualOnly = false)
        {
            player.PlayFireSound();
            Vector3 xzFacing = facing;
            xzFacing.y = 0;
            foreach (var groups in ShotgunPrm.BulletGroups)
            {
                float horizontalDegStep = groups.HorizontalDegree / Mathf.Max(groups.BulletNum-1, 1);
                float verticalDegStep = groups.VerticalDegree / Mathf.Max(groups.BulletNum-1, 1);
                for (int i = 0; i < groups.BulletNum; i++)
                {
                    Vector3 bulletFacing = facing;
                    float verticalSpread = verticalDegStep * i - groups.VerticalOffset;
                    Vector3 facingCross = Vector3.Cross(xzFacing.normalized, Vector3.up);
                    bulletFacing = Quaternion.AngleAxis(verticalSpread, facingCross) * 
                        Quaternion.AngleAxis(horizontalDegStep*i - groups.HorizontalOffset, Vector3.Cross(facing, facingCross)) * 
                        facing;
                    GameObject bullet = Instantiate(BulletObject, spawnPos, Quaternion.identity);
                    BulletBase bulletBase = bullet.GetComponent<BulletBase>();
                    bulletBase.SetupBullet(spawnPos, rootPos, bulletFacing.normalized, player, groups.CollisionParam, groups.MoveParam, groups.DamageParam, postDelay, visualOnly);
                    bullet.SetActive(true);
                }
            }
        }
    }
}
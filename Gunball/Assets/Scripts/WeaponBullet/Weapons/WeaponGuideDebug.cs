using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponGuideDebug : WeaponBulletMgr
{
    [SerializeField] GuideParam GuidePrm;
    [SerializeField] CollisionParam ColPrm;
    [SerializeField] MoveSimpleParam MovePrm;
    [SerializeField] DamageParam DmgPrm;

    public override GuideType GetGuideType()
    {
        return GuidePrm.ShotGuideType;
    }

    public override Vector3[] GetGuideCastPoints()
    {
        float counter = Mathf.Min(MovePrm.ToGravityTime, GuidePrm.ShotGuideSecs);
        List<Vector3> points = new List<Vector3>();
        points.Add(RootSpawnPos.position);
        Vector3 currPos = BulletSpawnPos.position;
        Vector3[] castPoints;
        currPos = BulletBase.TryBulletMove(BulletSpawnPos.position, currPos, facingDirection, 0, counter, MovePrm, out castPoints);
        foreach (Vector3 p in castPoints)
        {
            points.Add(p);
        }

        while (counter < GuidePrm.ShotGuideSecs)
        {
            currPos = BulletBase.TryBulletMove(BulletSpawnPos.position, currPos, facingDirection, counter, STEP_TIME, MovePrm, out castPoints);
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

    public override void FireWeaponBullet(Player player)
    {
        GameObject bullet = Instantiate(BulletObject, BulletSpawnPos.position, Quaternion.identity);
        BulletBase bulletBase = bullet.GetComponent<BulletBase>();
        bulletBase.SetupBullet(BulletSpawnPos, facingDirection, player, IgnoreColliders, ColPrm, MovePrm, DmgPrm);
        bullet.SetActive(true);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
}

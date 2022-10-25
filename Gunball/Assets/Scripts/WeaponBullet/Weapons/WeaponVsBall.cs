using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponVsBall : WeaponBulletMgr
{
    [SerializeField] GuideParam GuidePrm;
    [SerializeField] CollisionParam ColPrm;
    [SerializeField] MoveSimpleParam MovePrm;
    [SerializeField] ChargeParam ChgPrm;

    public override GuideType GetGuideType()
    {
        return GuidePrm.ShotGuideType;
    }

    
}

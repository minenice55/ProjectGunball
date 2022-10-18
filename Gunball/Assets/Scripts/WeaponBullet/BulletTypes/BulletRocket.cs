using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletRocket : BulletBase
{
    [SerializeField] GameObject BlastPrefab;
    WeaponBulletMgr.MoveBlastParam BlastParam;
    public void SetupBullet(Transform weaponPos, Transform playRootPos, Vector3 facing, Player owner, Collider[] ignoreColliders,
        WeaponBulletMgr.CollisionParam colPrm, 
        WeaponBulletMgr.MoveBlastParam movePrm, 
        WeaponBulletMgr.DamageParam dmgPrm )
    {
        base.SetupBullet(weaponPos, playRootPos, facing, owner, ignoreColliders, colPrm, movePrm.MoveSimpleParam, dmgPrm);
        BlastParam = movePrm;
        transform.forward = facing;
    }

    public new void Update() {
        float _dt = Time.deltaTime;
        Vector3 nextPos;
        Vector3 colPos;
        castPoints = DoBulletMove(_dt, out nextPos);
        if (BlastParam.DieOnGravityState && lifeTime >= BlastParam.MoveSimpleParam.ToGravityTime + BlastParam.DieOnGravityTime)
        {
            transform.forward = (nextPos - transform.position).normalized;
            transform.position = nextPos;
            DoOnCollisionKill(nextPos);
            Destroy(gameObject);
        }
        else if (castPoints != null)
        {
            if (CheckBulletCollision(castPoints, out colPos))
            {
                DoOnCollisionKill(colPos);
                Destroy(gameObject);
            }
            else
            {
                // face in movement direction
                transform.forward = (nextPos - transform.position).normalized;
                transform.position = nextPos;
            }
        }
    }

    protected override void DoOnCollisionKill(Vector3 pos)
    {
        GameObject.Instantiate(BlastPrefab, pos, Quaternion.identity);
        BulletBlast blast = BlastPrefab.GetComponent<BulletBlast>();
        blast.DoBlast(BlastParam.BlastSimpleParam, pos);
    }
}

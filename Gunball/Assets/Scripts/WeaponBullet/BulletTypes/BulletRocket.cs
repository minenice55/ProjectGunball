using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gunball.WeaponSystem;
using Gunball.MapObject;
namespace Gunball.WeaponSystem
{
    public class BulletRocket : BulletBase
    {
        [SerializeField] GameObject BlastPrefab;
        WeaponBase.MoveBlastParam BlastParam;
        public void SetupBullet(Vector3 weaponPos, Vector3 playRootPos, Vector3 facing, Player owner, Collider[] ignoreColliders,
            WeaponBase.CollisionParam colPrm,
            WeaponBase.MoveBlastParam movePrm,
            WeaponBase.DamageParam dmgPrm,
            float postDelay = 0)
        {
            base.SetupBullet(weaponPos, playRootPos, facing, owner, ignoreColliders, colPrm, movePrm.MoveSimpleParam, dmgPrm);
            BlastParam = movePrm;
            transform.forward = facing;
        }

        public new void Update()
        {
            float _dt = Time.deltaTime;
            Vector3 nextPos;
            Vector3 colPos;
            RaycastHit hit;
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
                if (CheckBulletCollision(castPoints, out colPos, out hit))
                {
                    DoOnCollisionKill(colPos, hit);
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

        protected override void DoOnCollisionKill(Vector3 pos, RaycastHit hit)
        {
            base.DoOnCollisionKill(pos, hit);
            GameObject.Instantiate(BlastPrefab, pos, Quaternion.identity);
            BulletBlast blast = BlastPrefab.GetComponent<BulletBlast>();
            blast.DoBlast(BlastParam.BlastSimpleParam, pos, owner);
        }

        protected override void DoOnCollisionKill(Vector3 pos)
        {
            base.DoOnCollisionKill(pos);
            GameObject.Instantiate(BlastPrefab, pos, Quaternion.identity);
            BulletBlast blast = BlastPrefab.GetComponent<BulletBlast>();
            blast.DoBlast(BlastParam.BlastSimpleParam, pos, owner);
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gunball.WeaponSystem;
using Gunball.MapObject;
namespace Gunball.WeaponSystem
{
    public class BulletBlast : MonoBehaviour
    {
        WeaponBase.BlastSimpleParam BlastPrm;

        public void DoBlast(WeaponBase.BlastSimpleParam blastPrm, Vector3 pos)
        {
            LayerMask playerMask = LayerMask.GetMask("Player", "Ball");
            gameObject.SetActive(true);
            GetComponent<Animator>()?.Play("Blast");
            transform.position = pos;
            BlastPrm = blastPrm;

            if (blastPrm.DistanceDamage.Length != 0)
            {
                Collider[] cols = Physics.OverlapSphere(pos, blastPrm.DistanceDamage[blastPrm.DistanceDamage.Length - 1].BlastRadius, playerMask);
                foreach (Collider col in cols)
                {
                    bool wasHit = false;
                    IShootableObject target = col.GetComponent<IShootableObject>();
                    if (target != null)
                    {
                        Vector3 cPoint = target.Transform.position;
                        float dist = Vector3.Distance(pos, cPoint);
                        for (int i = 0; i < blastPrm.DistanceDamage.Length; i++)
                        {
                            if (dist <= blastPrm.DistanceDamage[i].BlastRadius)
                            {
                                Vector3 direction = (cPoint - transform.position).normalized;
                                target.DoDamage(blastPrm.DistanceDamage[i].BlastDamage);
                                float bias = 1f;
                                WeaponBase.KnockbackParam kbPrm = blastPrm.DistanceDamage[i].Knockback;
                                switch (target.Type)
                                {
                                    case IShootableObject.ShootableType.Player:
                                        bias = kbPrm.PlayerBias;
                                        break;
                                    case IShootableObject.ShootableType.VsBall:
                                        bias = kbPrm.VsBallBias;
                                        break;
                                    case IShootableObject.ShootableType.MapObject:
                                        bias = kbPrm.MapObjectBias;
                                        break;
                                    case IShootableObject.ShootableType.None:
                                        continue;
                                }
                                target.Knockback(direction * kbPrm.Force * bias, pos);
                                target.SetKnockbackTimer(kbPrm.TimeBias);
                                wasHit = true;
                                break; 
                            }
                        }
                    }
                    if (wasHit)
                    {
                        break;
                    }
                }
            }
        }

        // use in animations
        public void KillBlastObject()
        {
            Destroy(gameObject);
        }
    }
}
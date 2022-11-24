using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gunball.MapObject;
namespace Gunball.WeaponSystem
{
    public class BulletBlast : MonoBehaviour
    {
        public void DoBlast(WeaponBase.BlastSimpleParam blastPrm, Vector3 pos, IDamageSource source, bool visualOnly = false)
        {
            gameObject.SetActive(true);
            transform.position = pos;

            if (visualOnly) return;

            List<GameObject> hitTargets = new List<GameObject>();
            LayerMask targetMask = LayerMask.GetMask("Player", "Ball");
            if (blastPrm.DistanceDamage.Length != 0)
            {
                Collider[] cols = Physics.OverlapSphere(pos, blastPrm.DistanceDamage[blastPrm.DistanceDamage.Length - 1].BlastRadius, targetMask);
                foreach (Collider col in cols)
                {
                    IShootableObject target = col.GetComponent<IShootableObject>();
                    if (!hitTargets.Contains(col.gameObject))
                    {
                        if (target != null)
                        {
                            Vector3 cPoint = target.Transform.position;
                            float dist = Vector3.Distance(pos, cPoint);
                            for (int i = 0; i < blastPrm.DistanceDamage.Length; i++)
                            {
                                if (dist <= blastPrm.DistanceDamage[i].BlastRadius)
                                {
                                    Vector3 direction = (cPoint - transform.position).normalized;
                                    source.InflictDamage(blastPrm.DistanceDamage[i].BlastDamage, target);
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
                                    source.InflictKnockback(direction * kbPrm.Force * bias, cPoint, kbPrm.TimeBias, target);
                                    hitTargets.Add(col.gameObject);
                                    break; 
                                }
                            }
                        }
                    }
                }
            }
        }

        void Start()
        {
            GetComponent<Animator>()?.Play("Blast");
        }

        // use in animations
        public void KillBlastObject()
        {
            Destroy(gameObject);
        }

        public void PlayBlastSound()
        {
            GetComponent<AudioSource>()?.Play();
        }
    }
}
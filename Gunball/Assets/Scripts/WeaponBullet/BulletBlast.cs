using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBlast : MonoBehaviour
{
    WeaponBulletMgr.BlastSimpleParam BlastPrm;

    public void DoBlast(WeaponBulletMgr.BlastSimpleParam blastPrm, Vector3 pos)
    {
        LayerMask playerMask = LayerMask.GetMask("Player");
        gameObject.SetActive(true);
        GetComponent<Animator>()?.Play("Blast");
        transform.position = pos;
        BlastPrm = blastPrm;

        if (blastPrm.DistanceDamage.Length != 0)
        {
            Collider[] cols = Physics.OverlapSphere(pos, blastPrm.DistanceDamage[blastPrm.DistanceDamage.Length - 1].BlastRadius, playerMask);
            foreach (Collider col in cols)
            {
                Player player = col.GetComponent<Player>();
                if (player != null)
                {
                    Vector3 cPoint = col.ClosestPoint(pos);
                    float dist = Vector3.Distance(pos, cPoint);
                    for (int i = 0; i < blastPrm.DistanceDamage.Length; i++)
                    {
                        if (dist <= blastPrm.DistanceDamage[i].BlastRadius)
                        {
                            Debug.Log("Hit player with damage: " + blastPrm.DistanceDamage[i].BlastDamage);
                            Vector3 direction = (cPoint - transform.position).normalized;
                            player.Knockback(direction * blastPrm.DistanceDamage[i].Knockback.Force, pos);
                            break;
                        }
                    }
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

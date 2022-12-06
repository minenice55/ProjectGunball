using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gunball.MapObject
{
    public class DeathPlane : MonoBehaviour, IDamageSource
    {
        private void OnTriggerEnter(Collider other)
        {
            IShootableObject shootable = other.GetComponent<IShootableObject>();
            if (shootable != null)
            {
                InflictDamage(0, shootable);
            }
        }

        public void InflictKnockback(Vector3 force, Vector3 pos, float knockbackTimer, IShootableObject target)
        {
            return;
        }
        public void InflictDamage(float damage, IShootableObject target, bool doCharge = true)
        {
            target.DoDamage(target.Health);
            target.DoDeath(this);
        }
        public void InflictHealing(float damage, IShootableObject target)
        {
            return;
        }
    }
}
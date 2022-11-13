using System;
using UnityEngine;
namespace Gunball.MapObject
{
    public interface IDamageSource
    {
        GameObject gameObject { get; }
        public void InflictKnockback(Vector3 force, Vector3 pos, float knockbackTimer, IShootableObject target);
        public void InflictDamage(float damage, IShootableObject target);
        public void InflictHealing(float damage, IShootableObject target);
    }
}

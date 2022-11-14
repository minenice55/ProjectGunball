using System;
using UnityEngine;
namespace Gunball.MapObject
{
    public interface IShootableObject
    {
        enum ShootableType
        {
            Player,
            VsBall,
            MapObject,
            None
        }
        GameObject gameObject { get; }
        float Health { get; set; }
        float MaxHealth { get; }
        bool IsDead { get; }
        Transform Transform { get; }
        ShootableType Type { get; }
        public void Knockback(Vector3 force, Vector3 pos);
        public void SetKnockbackTimer(float timeBias);
        public void DoDamage(float damage, IDamageSource source = null);
        public void RecoverDamage(float healing, IDamageSource source = null);

        public void DoDeath(IDamageSource cause = null);
    }
}
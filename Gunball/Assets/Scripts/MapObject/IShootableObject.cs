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
        float Health { get; set; }
        float MaxHealth { get; }
        bool IsDead { get; }
        Transform Transform { get; }
        ShootableType Type { get; }
        public void Knockback(Vector3 force, Vector3 pos);
        public void SetKnockbackTimer(float timeBias);
        public void DoDamage(float damage, Player source = null);
        public void RecoverDamage(float healing, Player source = null);

        public void DoDeath(Player cause = null);
    }
}
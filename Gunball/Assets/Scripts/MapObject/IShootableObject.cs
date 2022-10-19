using System;
using UnityEngine;
public interface IShootableObject {
    float Health { get; set; }
    bool IsDead { get; }
    public void Knockback(Vector3 force, Vector3 pos);
    public void DoDamage(float damage, Player source = null);
    public void RecoverDamage(float healing, Player source = null);
}

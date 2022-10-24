using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunBall : MonoBehaviour, IShootableObject
{
    float _health = 1000f;
    [SerializeField] Rigidbody _rigidbody;

    #region Properties
    public float Health { get => _health; set { _health = 1000f; } }
    public bool IsDead { get => false; }
    public Transform Transform { get => transform; }
    public IShootableObject.ShootableType Type { get => IShootableObject.ShootableType.VsBall; }
    #endregion

    #region Methods
    void Start() {
        
    }

    public void Knockback(Vector3 force, Vector3 pos)
    {
        _rigidbody.AddForceAtPosition(force, pos, ForceMode.Impulse);
    }

    public void DoDamage(float damage, Player source = null)
    {
        return;
    }
    public void RecoverDamage(float healing, Player source = null)
    {
        return;
    }
    #endregion
}

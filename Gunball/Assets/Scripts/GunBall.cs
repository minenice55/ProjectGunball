using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunBall : MonoBehaviour, IShootableObject, IPickup
{
    float _health = 1000f;
    [SerializeField] Rigidbody _rigidbody;

    #region Properties
    public float Health { get => _health; set { _health = 1000f; } }
    public bool IsDead { get => false; }
    public Transform Transform { get => transform; }
    public IShootableObject.ShootableType Type { get {return _owner == null ? IShootableObject.ShootableType.VsBall : IShootableObject.ShootableType.None;} }

    public Player Owner {get => _owner;}
    #endregion

    #region Private Variables
    Player _owner = null;
    Vector3 origScale;
    #endregion

    #region Methods
    void Start() {
        origScale = transform.localScale;
    }

    void Update() {
        if (_owner != null)
        {
            DoEffect();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_owner != null) return;
        if (other.tag == "Player")
        {
            Player play = other.gameObject.GetComponent<Player>();
            if (!play.InAction)
                Pickup(play);
        }
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

    public void Pickup(Player player)
    {
        _owner = player;
        transform.position = _owner.BallPickupPos.position;
        transform.rotation = _owner.BallPickupPos.rotation;
        transform.localScale = _owner.BallPickupPos.localScale;
        _rigidbody.isKinematic = true;
    }

    public void DoEffect()
    {
        transform.position = _owner.BallPickupPos.position;
        transform.rotation = _owner.BallPickupPos.rotation;
        transform.localScale = _owner.BallPickupPos.localScale;
    }

    public void EndEffect()
    {
        transform.localScale = origScale;
        _rigidbody.isKinematic = false;
    }
    #endregion
}

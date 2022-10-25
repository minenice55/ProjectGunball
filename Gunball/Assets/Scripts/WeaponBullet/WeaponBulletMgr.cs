using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class WeaponBulletMgr : MonoBehaviour
{
    [SerializeField] public WeaponParam WpPrm;
    public const float STEP_TIME = 1/30f;
    [SerializeField] protected Transform RootSpawnPos;
    [SerializeField] protected Transform BulletSpawnPos;

    [SerializeField] protected GameObject BulletObject;

    public Vector3 FacingDirection {get {return facingDirection;}}
    
    public Collider[] IgnoreColliders;
    protected Vector3 facingDirection;
    protected float nextFireTime = 0, lastActionTime = Single.MinValue;
    protected Player owner;

    public enum GuideType {
        None,
        Shot,
        Direction,
        Trajectory
    }

    #region Methods
    public void SetFacingDirection(Vector3 dir)
    {
        facingDirection = dir;
    }
    public void SetOwner(GameObject owner, Transform weaponPos)
    {
        this.owner = owner.GetComponent<Player>();
        RootSpawnPos = owner.transform;
        BulletSpawnPos = weaponPos;
        IgnoreColliders = owner.GetComponentsInChildren<Collider>();
    }
    #endregion

    #region Virtual Methods
    public virtual void StartFireSequence(Player player) {
        owner.FireCoroutine = DoFireSequence(owner);
        owner.StartCoroutine(owner.FireCoroutine);
    }
    public virtual IEnumerator DoFireSequence(Player player)
    {
        player.InFireCoroutine = true;
        yield return new WaitForSeconds(WpPrm.PreDelayTime);
        CreateWeaponBullet(player);
        player.InFireCoroutine = false;
    }
    public virtual void CreateWeaponBullet(Player player) {}
    public virtual bool RefireCheck(float heldDuration, float relaxDuration, WeaponParam wpPrm) {
        if (heldDuration <= 0 && relaxDuration > 0)
        {
            nextFireTime = 0;
            return false;
        }
        lastActionTime = Time.time;
        if (heldDuration < Mathf.Max(0, wpPrm.RepeatTime - relaxDuration))
            return false;
        if (heldDuration >= nextFireTime)
        {
            nextFireTime = heldDuration + wpPrm.RepeatTime;
            return true;
        }
        return false;
    }
    public virtual bool GetPlayerInAction() {return (Time.time - lastActionTime <= WpPrm.PreDelayTime);}

    public virtual Vector3[] GetGuideCastPoints() { return new Vector3[]{RootSpawnPos.position, BulletSpawnPos.position}; }
    public virtual float GetGuideRadius() { return 0f; }
    public virtual float GetGuideWidth() { return 0f; }
    public virtual GuideType GetGuideType() { return GuideType.None; }
    public virtual LayerMask GetGuideCollisionMask() { return new LayerMask(); }
    public virtual Vector3 OverrideSpawnPos() { return BulletSpawnPos.position; }
    #endregion
    
    #region Parameter Structs
    [Serializable] public struct WeaponParam
    {
        [Tooltip("Maximum reserve ammo")]
        public int MaxAmmo;
        [Tooltip("Clip size (-1: no clip, draw directly from reserves)")]
        public int AmmoClip;
        [Tooltip("Ammo consumed per shot")]
        public int AmmoConsume;

        [Tooltip("Weapon startup time")]
        public float PreDelayTime;
        [Tooltip("Time between shots")]
        public float RepeatTime;

        [Tooltip("Override player run speed while firing (-1: disabled)")]
        public float RunSpeedOverride;
        [Tooltip("Override player air speed while firing (-1: disabled)")]
        public float AirSpeedOverride;
    }

    [Serializable] public struct ChargeParam
    {
        [Tooltip("Charge time for full power")]
        public float FullChargeTime;
        [Tooltip("Need to charge for at least this time to fire")]
        public float MinChargeTime;
        [Tooltip("Ammo consumed on min charge (-1: use same ammo as normal)")]
        public int MinChargeAmmo;
        [Tooltip("Speed on min charge")]
        public float MinChargeSpeed;
        [Tooltip("Speed to lerp to for max charge (a full charge will override this)")]
        public float MaxChargeSpeed;
    }

    [Serializable] public struct SpreadParam
    {
        [Tooltip("Spread in degrees while on ground")]
        public float GroundDegSpread;
        [Tooltip("Spread in degrees while jumping")]
        public float JumpDegSpread;

        [Tooltip("Starting spread bias on spawn")]
        public float DegBiasStart;
        [Tooltip("Maximum spread bias while on ground")]
        public float GroundDegBiasMax;

        [Tooltip("Set spread bias to this value when jumping")]
        public float JumpDegBiasSetting;

        [Tooltip("Spread bias increase while holding down the trigger")]
        public float DegBiasIncrease;
        [Tooltip("Spread bias decrease while letting go of the trigger")]
        public float DegBiasDecrease;

        [Tooltip("Time to start returning spread value to grounded values after jumping")]
        public float JumpSpreadLerpStart;
        [Tooltip("Time to finish returning spread value to grounded values after jumping")]
        public float JumpSpreadLerpEnd;
    }

    [Serializable] public struct CollisionParam
    {
        [Tooltip("Starting radius of the bullet for terrain collision detection")]
        public float InitRadiusField;
        [Tooltip("Starting radius of the bullet for player collision detection")]
        public float InitRadiusPlayer;

        [Tooltip("Final radius of the bullet for terrain collision detection")]
        public float EndRadiusField;
        [Tooltip("Final radius of the bullet for player collision detection")]
        public float EndRadiusPlayer;

        [Tooltip("Collision detection radius for the reticle")]
        public float GuideRadius;
        [Tooltip("What this bullet collides with")]
        public LayerMask CollisionMask;
        [Tooltip("Time for which the bullet ignores collisions with teammates")]
        public float FriendlyFireIgnoreTime;
    }

    [Serializable] public struct GuideParam
    {
        [Tooltip("How far ahead to redict the bullet position for the guide / reticle")]
        public float ShotGuideSecs;
        [Tooltip("Radius of the guide spherecast")]
        public float GuideRadius;
        [Tooltip("A generic width value for the guide / reticle")]
        public float GuideWidth;
        [Tooltip("The type of guide / reticle")]
        public GuideType ShotGuideType;
    }

    [Serializable] public struct MoveSimpleParam
    {
        [Tooltip("How fast the bullet moves when spawned")]
        public float SpawnSpeed;
        [Tooltip("Time until the bullet is affected by gravity")]
        public float ToGravityTime;
        [Tooltip("Speed the bullet gets set to once gravity is applied")]
        public float GravitySpeed;
        [Tooltip("The bullet's gravity")]
        public float Gravity;
        [Tooltip("The bullet's drag when affected by gravity")]
        public float GravityDragRatio;
    }

    [Serializable] public struct MoveBlastParam
    {
        [Tooltip("Remove the bullet once gravity is applied?")]
        public bool DieOnGravityState;
        [Tooltip("If bullet should die on gravity state, how long until it dies")]
        public float DieOnGravityTime;
        public MoveSimpleParam MoveSimpleParam;
        public BlastSimpleParam BlastSimpleParam;
    }

    [Serializable] public struct BlastSimpleParam
    {
        [Tooltip("Linearly interpolate damage and knockback based on distance from radius")]
        public bool DamageLinear;
        public DistanceDamageParam[] DistanceDamage;
        [Serializable] public struct DistanceDamageParam
        {
            [Tooltip("Radius of the blast")]
            public float BlastRadius;
            [Tooltip("Damage of the blast")]
            public float BlastDamage;
            public KnockbackParam Knockback;
        }
    }

    [Serializable] public struct KnockbackParam
    {
        [Tooltip("Knockback force")]
        public float Force;
        [Tooltip("Radius to apply knockback in")]
        public float Radius;
        [Tooltip("Distance to multiply force by at the edge of the radius")]
        public float DistanceBias;
        [Tooltip("Distance to multiply force by when hitting a player")]
        [DefaultValue(1f)]
        public float PlayerBias;
        [Tooltip("Distance to multiply force by when hitting the ball")]
        [DefaultValue(1f)]
        public float VsBallBias;
        [Tooltip("Distance to multiply force by when hitting any other map object")]
        [DefaultValue(1f)]
        public float MapObjectBias;
    }

    [Serializable] public struct DamageParam
    {
        [Tooltip("Maximum damage before falloff")]
        public float DamageMax;
        [Tooltip("Minimum damage after falloff")]
        public float DamageMin;
        [Tooltip("Time before falloff starts")]
        public float ReduceStartTime;
        [Tooltip("Time until falloff is complete")]
        public float ReduceEndTime;
        public KnockbackParam Knockback;
    }

    [Serializable] public struct HealingParam
    {
        [Tooltip("Maximum healing before falloff")]
        public float HealingMax;
        [Tooltip("Minimum healing after falloff")]
        public float HealingMin;
        [Tooltip("Time before falloff starts")]
        public float ReduceStartTime;
        [Tooltip("Time until falloff is complete")]
        public float ReduceEndTime;
        [Tooltip("On impact, time over which the healing is applied")]
        public float HealingApplicationTime;
    }

    #endregion
}

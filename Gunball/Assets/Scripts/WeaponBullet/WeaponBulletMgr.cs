using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBulletMgr : MonoBehaviour
{
    public const float STEP_TIME = 1/20f;
    [SerializeField] protected Transform RootSpawnPos;
    [SerializeField] protected Transform BulletSpawnPos;

    [SerializeField] protected GameObject BulletObject;
    
    public Collider[] IgnoreColliders;
    protected Vector3 facingDirection;

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
        RootSpawnPos = owner.transform;
        BulletSpawnPos = weaponPos;
        IgnoreColliders = owner.GetComponentsInChildren<Collider>();
    }
    #endregion

    #region Virtual Methods
    public virtual void FireWeaponBullet(Player player) {}
    public virtual Vector3[] GetGuideCastPoints() { return new Vector3[]{RootSpawnPos.position, BulletSpawnPos.position}; }
    public virtual GuideType GetGuideType() { return GuideType.None; }
    public virtual LayerMask GetGuideCollisionMask() { return new LayerMask(); }
    #endregion
    
    #region Parameter Structs
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
        [Serializable] public class DistanceDamageParam
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

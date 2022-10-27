using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBase : MonoBehaviour
{
    protected float lifeTime;
    protected Player owner;
    protected Vector3 startPos;
    protected Vector3 rootPos;
    protected Collider[] ignoreColliders;
    protected Vector3 facingDirection;
    protected Vector3[] castPoints;
    protected WeaponBulletMgr.CollisionParam ColPrm;
    protected WeaponBulletMgr.MoveSimpleParam MovePrm;
    protected WeaponBulletMgr.DamageParam DmgPrm;
    Vector3 gravityStateSpeed;
    RaycastHit[] hitsBuffer = new RaycastHit[16];

    LayerMask TerrainMask;

    public virtual void SetupBullet(Transform weaponPos, Transform playRootPos, Vector3 facing, Player owner, Collider[] ignoreColliders,
        WeaponBulletMgr.CollisionParam colPrm, 
        WeaponBulletMgr.MoveSimpleParam movePrm, 
        WeaponBulletMgr.DamageParam dmgPrm )
    {
        ColPrm = colPrm;
        MovePrm = movePrm;
        DmgPrm = dmgPrm;

        startPos = weaponPos.position;
        rootPos = playRootPos.position;
        transform.position = startPos;
        facingDirection = facing;
        this.ignoreColliders = ignoreColliders;
        this.owner = owner;

        gravityStateSpeed = MovePrm.GravitySpeed * facing;
        TerrainMask = LayerMask.GetMask("Ground", "Wall", "MapObjectSolid");
    }

    public void Update() {
        float _dt = Time.deltaTime;
        Vector3 nextPos;
        Vector3 colPos;
        RaycastHit hit;
        castPoints = DoBulletMove(_dt, out nextPos);
        if (castPoints != null)
        {
            if (CheckBulletCollision(castPoints, out colPos, out hit))
            {
                DoOnCollisionKill(colPos, hit);
                Destroy(gameObject);
            }
            else
            {
                transform.position = nextPos;
            }
        }
    }

    protected virtual void DoOnCollisionKill(Vector3 pos, RaycastHit hit)
    {
        IShootableObject hitObj = hit.collider.GetComponent<IShootableObject>();
        if (hitObj != null)
        {
            float damage = DmgPrm.DamageMax;
            if (lifeTime >= DmgPrm.ReduceStartTime)
            {
                damage = Mathf.Max( 
                    DmgPrm.DamageMin,
                    Mathf.Lerp(DmgPrm.DamageMax, DmgPrm.DamageMin, (lifeTime - DmgPrm.ReduceStartTime) / (DmgPrm.ReduceEndTime - DmgPrm.ReduceStartTime))
                );
            }
            hitObj.DoDamage(damage, owner);
            float bias = 1f;
            switch (hitObj.Type)
            {
                case IShootableObject.ShootableType.Player:
                    bias = DmgPrm.Knockback.PlayerBias;
                    break;
                case IShootableObject.ShootableType.VsBall:
                    bias = DmgPrm.Knockback.VsBallBias;
                    break;
                case IShootableObject.ShootableType.MapObject:
                    bias = DmgPrm.Knockback.MapObjectBias;
                    break;
                case IShootableObject.ShootableType.None:
                    return;
            }
            hitObj.Knockback(facingDirection * DmgPrm.Knockback.Force * bias, pos);
        }
    }

    protected virtual void DoOnCollisionKill(Vector3 pos) {}

    protected virtual Vector3[] DoBulletMove(float _dt, out Vector3 nextPos)
    {
        Vector3[] castPoints;
        nextPos = TryBulletMove(startPos, rootPos, transform.position, facingDirection, lifeTime, _dt, MovePrm, out castPoints);
        lifeTime += _dt;
        return castPoints;
    }
    protected virtual bool CheckBulletCollision(Vector3[] castPoints, out Vector3 colPosition, out RaycastHit hit)
    {
        //spherecast between each point
        for (int i = 0; i < castPoints.Length - 1; i++)
        {
            Vector3 dir = castPoints[i + 1] - castPoints[i];
            float dist = dir.magnitude;
            dir.Normalize();

            //first players and other objects
            //lerp between radius
            //clear buffer
            for (int b = 0; b < hitsBuffer.Length; b++)
            {
                hitsBuffer[b] = new RaycastHit();
            }
            float radius = Mathf.Lerp(ColPrm.InitRadiusPlayer, ColPrm.EndRadiusPlayer, Mathf.Min(lifeTime/MovePrm.ToGravityTime, 1f));
            int hitCount = Physics.SphereCastNonAlloc(castPoints[i], radius, dir, hitsBuffer, dist, ColPrm.CollisionMask &~ TerrainMask);
            System.Array.Sort(hitsBuffer, (a, b) => a.distance.CompareTo(b.distance));
            if (hitCount > 0)
            {
                for (int j = 0; j < hitsBuffer.Length; j++)
                {
                    if (hitsBuffer[j].collider != null)
                    {
                         
                        //first check if is none type of IShootableObject
                        IShootableObject shootable = hitsBuffer[j].collider.gameObject.GetComponent<IShootableObject>();
                        if (shootable != null)
                        {
                            if (shootable.Type == IShootableObject.ShootableType.None) continue;
                        }

                        //then check ignore list
                        bool ignore = false;
                        for (int k = 0; k < ignoreColliders.Length; k++)
                        {
                            if (hitsBuffer[j].collider == ignoreColliders[k])
                            {
                                ignore = true;
                                break;
                            }
                        }
                        if (ignore) continue;
                        colPosition = hitsBuffer[j].point;
                        hit = hitsBuffer[j];
                        return true;
                    }
                }
            }

            //then check against terrain flags
            //clear buffer
            for (int b = 0; b < hitsBuffer.Length; b++)
            {
                hitsBuffer[b] = new RaycastHit();
            }
            radius = Mathf.Lerp(ColPrm.InitRadiusField, ColPrm.EndRadiusField, Mathf.Min(lifeTime/MovePrm.ToGravityTime, 1f));
            hitCount = Physics.SphereCastNonAlloc(castPoints[i], radius, dir, hitsBuffer, dist, ColPrm.CollisionMask & TerrainMask);
            // sort by distance
            System.Array.Sort(hitsBuffer, (a, b) => a.distance.CompareTo(b.distance));
            if (hitCount > 0)
            {
                for (int j = 0; j < hitsBuffer.Length; j++)
                {
                    if (hitsBuffer[j].collider == null) continue;
                    colPosition = hitsBuffer[j].point;
                    hit = hitsBuffer[j];
                    return true;
                }
            }
        }
        colPosition = Vector3.zero;
        hit = new RaycastHit();
        return false;
    }
    public static Vector3 TryBulletMove(Vector3 startPos, Vector3 rootPos, Vector3 currPos, Vector3 facingDirection, float lifeTime, float _dt, WeaponBulletMgr.MoveSimpleParam movePrm, out Vector3[] castPos)
    {
        List<Vector3> castPoints = new List<Vector3>();
        Vector3 pos = currPos;
        Vector3 gravityStateSpeed = movePrm.GravitySpeed * facingDirection;
        float gravity = 0;
        float currTime = lifeTime + _dt;
        if (lifeTime <= 0f)
        {
            castPoints.Add(rootPos);
            castPoints.Add(startPos);
        }
        else
        {
            castPoints.Add(pos);
        }
        //move bullet
        if (currTime > movePrm.ToGravityTime)
        {
            if (lifeTime < movePrm.ToGravityTime)
            {
                // go to the straight state end position
                lifeTime = movePrm.ToGravityTime;
                pos = startPos + movePrm.SpawnSpeed * facingDirection * movePrm.ToGravityTime;
                castPoints.Add(pos);
                // see if we need to exit early due to no gravity state
                if (movePrm.GravitySpeed <= 0 && movePrm.Gravity <= 0)
                {
                    castPos = castPoints.ToArray();
                    return pos;
                }

                //gravity step
                float lastStepTime = movePrm.ToGravityTime - lifeTime;
                gravityStateSpeed /= (Mathf.Max(movePrm.GravityDragRatio, 1) * (lifeTime + lastStepTime));
                gravity = movePrm.Gravity * (lifeTime + lastStepTime - movePrm.ToGravityTime);
                pos += gravityStateSpeed * lastStepTime + gravity * Vector3.down * lastStepTime;
                castPoints.Add(pos);
            }
            else
            {
                //gravity step
                gravityStateSpeed /= (Mathf.Max(movePrm.GravityDragRatio, 1) * currTime);
                gravity = movePrm.Gravity * (currTime - movePrm.ToGravityTime);
                pos += gravityStateSpeed * _dt + gravity * Vector3.down * _dt;
                castPoints.Add(pos);
            }
        }
        else
        {
            pos = startPos + movePrm.SpawnSpeed * facingDirection * movePrm.ToGravityTime * (currTime / movePrm.ToGravityTime);
            castPoints.Add(pos);
        }
        castPos = castPoints.ToArray();
        return pos;
    }
}

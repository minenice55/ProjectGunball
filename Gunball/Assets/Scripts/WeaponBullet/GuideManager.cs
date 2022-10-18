using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GuideManager : MonoBehaviour
{

    [SerializeField] RectTransform CollidingGuideRect;
    [SerializeField] RectTransform PredictingGuideRect;

    Camera mainCam;
    WeaponBulletMgr Wpn;
    Vector3 PredictionPos;
    RaycastHit[] hitsBuffer = new RaycastHit[16];

    public void UpdateGuide()
    {
        if (Wpn == null) 
        {
            CollidingGuideRect.gameObject.SetActive(false);
            PredictingGuideRect.gameObject.SetActive(false);
            return;
        }
        PredictingGuideRect.gameObject.SetActive(true);
        CollidingGuideRect.gameObject.SetActive(true);

        // clear the buffer
        for (int i = 0; i < hitsBuffer.Length; i++)
        {
            hitsBuffer[i] = new RaycastHit();
        }

        Vector3[] guideCastPoints = Wpn.GetGuideCastPoints();
        //set position of guide rects
        WorldToScreenPoint(CastGuide(out PredictionPos, guideCastPoints), CollidingGuideRect);
        WorldToScreenPoint(PredictionPos, PredictingGuideRect);
        DrawDebugGuidePath(guideCastPoints);
    }

    public void SetCamera(Camera cam)
    {
        mainCam = cam;
    }

    public void SetWeapon(WeaponBulletMgr wpn)
    {
        Wpn = wpn;
    }

    Vector3 CastGuide(out Vector3 PredictionPos, Vector3[] CastPoints)
    {
        PredictionPos = CastPoints[CastPoints.Length - 1];
        // iterate through each cast point checking for a collision in between
        for (int i = 0; i < CastPoints.Length - 1; i++)
        {
            Vector3 startPos = CastPoints[i];
            Vector3 endPos = CastPoints[i + 1];
            Vector3 dir = endPos - startPos;
            float dist = dir.magnitude;
            dir.Normalize();

            // check for collision
            // todo: use the non-alloc version of this
            Physics.RaycastNonAlloc(startPos, dir, hitsBuffer, dist, Wpn.GetGuideCollisionMask() , QueryTriggerInteraction.Ignore);
            // sort by distance
            System.Array.Sort(hitsBuffer, (a, b) => a.distance.CompareTo(b.distance));
            // iterate through hits and check if they are in our ignore list
            for (int j = 0; j < hitsBuffer.Length; j++)
            {
                if (hitsBuffer[j].collider == null) continue;
                bool ignore = false;
                for (int k = 0; k < Wpn.IgnoreColliders.Length; k++)
                {
                    if (hitsBuffer[j].collider == Wpn.IgnoreColliders[k])
                    {
                        ignore = true;
                        break;
                    }
                }
                if (ignore) continue;
                // we hit something, return the position
                return hitsBuffer[j].point;
            }
        }
        return PredictionPos;
    }

    void DrawDebugGuidePath(Vector3[] CastPoints)
    {
        for (int i = 0; i < CastPoints.Length - 1; i++)
        {
            Debug.DrawLine(CastPoints[i], CastPoints[i + 1], Color.magenta);
        }
    }

    void WorldToScreenPoint(Vector3 worldPos, RectTransform rect)
    {
        Vector3 screenPos = mainCam.WorldToScreenPoint(worldPos);
        rect.position = screenPos;
    }  
}

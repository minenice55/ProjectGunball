using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GuideManager : MonoBehaviour
{

    [SerializeField] RectTransform CollidingGuideRect;
    [SerializeField] RectTransform PredictingGuideRect;
    [SerializeField] Transform GuideTarget;
    [SerializeField] LineRenderer trajectoryRenderer;

    Camera mainCam;
    WeaponBulletMgr Wpn;
    Vector3 PredictionPos;
    RaycastHit[] hitsBuffer = new RaycastHit[16];

    public void UpdateGuide()
    {
        trajectoryRenderer.positionCount = 0;
        if (Wpn == null || Wpn.GetGuideType() == WeaponBulletMgr.GuideType.None)
        {
            CollidingGuideRect.gameObject.SetActive(false);
            PredictingGuideRect.gameObject.SetActive(false);
            return;
        }

        // clear the buffer
        for (int i = 0; i < hitsBuffer.Length; i++)
        {
            hitsBuffer[i] = new RaycastHit();
        }

        Vector3[] points = Wpn.GetGuideCastPoints();
        switch (Wpn.GetGuideType())
        {
            case WeaponBulletMgr.GuideType.Shot:
                PredictingGuideRect.gameObject.SetActive(true);
                CollidingGuideRect.gameObject.SetActive(true);
                DrawShotGuide(points);
                break;
            case WeaponBulletMgr.GuideType.Trajectory:
                CollidingGuideRect.gameObject.SetActive(false);
                PredictingGuideRect.gameObject.SetActive(false);
                DrawTrajectoryGuide(points);
                break;
            default:
                break;
        }
    }

    public void SetCamera(Camera cam)
    {
        mainCam = cam;
    }

    public void SetWeapon(WeaponBulletMgr wpn)
    {
        Wpn = wpn;
    }

    Vector3 CastGuide(out Vector3 PredictionPos, out int lastPt, Vector3[] CastPoints)
    {
        lastPt = CastPoints.Length - 1;
        PredictionPos = CastPoints[lastPt];
        // iterate through each cast point checking for a collision in between
        for (int i = 0; i < CastPoints.Length - 1; i++)
        {
            Vector3 startPos = CastPoints[i];
            Vector3 endPos = CastPoints[i + 1];
            Vector3 dir = endPos - startPos;
            float dist = dir.magnitude;
            dir.Normalize();

            // check for collision
            Physics.SphereCastNonAlloc(startPos, Wpn.GetGuideRadius(), dir, hitsBuffer, dist, Wpn.GetGuideCollisionMask() , QueryTriggerInteraction.Ignore);
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
                lastPt = i;
                return hitsBuffer[j].point;
            }
        }
        return PredictionPos;
    }

    void DrawShotGuide(Vector3[] points)
    {
        WorldToScreenPoint(CastGuide(out PredictionPos, out int LastPoint, points), CollidingGuideRect);
        WorldToScreenPoint(PredictionPos, PredictingGuideRect);
        GuideTarget.position = PredictionPos;
        DrawDebugGuidePath(points);
    }

    void DrawTrajectoryGuide(Vector3[] points)
    {
        if (Wpn.GetGuideRadius() <= 0) return;
        List<Vector3> trajectoryPoints = new List<Vector3>();
        Vector3 endPos = CastGuide(out PredictionPos, out int LastPoint, points);
        trajectoryRenderer.startWidth = Wpn.GetGuideRadius();
        trajectoryRenderer.endWidth = Wpn.GetGuideRadius();
        //add points in reverse
        trajectoryPoints.Add(endPos);
        trajectoryPoints.Add(Vector3.MoveTowards(endPos, points[LastPoint], 0.01f));
        for (int i = LastPoint; i > 0; i--)
        {
            trajectoryPoints.Add(points[i]);
            trajectoryPoints.Add(Vector3.MoveTowards(points[i], points[i-1], 0.01f));
        }
        trajectoryRenderer.positionCount = trajectoryPoints.Count;
        trajectoryRenderer.SetPositions(trajectoryPoints.ToArray());
        DrawDebugGuidePath(points);
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

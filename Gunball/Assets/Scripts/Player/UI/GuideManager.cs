using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Gunball.MapObject;
using Gunball.WeaponSystem;

namespace Gunball.WeaponSystem
{
    public class GuideManager : MonoBehaviour
    {

        [SerializeField] RectTransform CollidingGuideRect;
        [SerializeField] RectTransform CollidingGuideOnRect;
        [SerializeField] RectTransform CollidingGuideEffectiveRect;
        [SerializeField] RectTransform PredictingGuideRect;
        [SerializeField] RectTransform RespawnGuideRect;
        [SerializeField] RectTransform GoalGuideRect;
        [SerializeField] Transform GuideTarget;
        [SerializeField] LineRenderer trajectoryRenderer;

        Camera mainCam;
        WeaponBase Wpn;
        Vector3 PredictionPos;
        RaycastHit[] hitsBuffer = new RaycastHit[16];

        public void UpdateGuide()
        {
            if (CollidingGuideRect == null || PredictingGuideRect == null || RespawnGuideRect == null || trajectoryRenderer == null) return;
            trajectoryRenderer.positionCount = 0;
            if (Wpn == null || Wpn.GetGuideType() == WeaponBase.GuideType.None)
            {
                CollidingGuideRect.gameObject.SetActive(false);
                CollidingGuideOnRect.gameObject.SetActive(false);
                CollidingGuideEffectiveRect.gameObject.SetActive(false);
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
                case WeaponBase.GuideType.Shot:
                    PredictingGuideRect.gameObject.SetActive(true);
                    CollidingGuideRect.gameObject.SetActive(true);
                    DrawShotGuide(points);
                    break;
                case WeaponBase.GuideType.Trajectory:
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

        public void SetWeapon(WeaponBase wpn)
        {
            Wpn = wpn;
        }

        public void SetIsRespawnGuide(bool isRespawnGuide, Vector3 respawnPos, ITeamObject.Teams team)
        {
            if (RespawnGuideRect != null)
            {
                RespawnGuideRect.gameObject.SetActive(isRespawnGuide);
                if (isRespawnGuide)
                {
                    RespawnGuideRect.gameObject.GetComponent<Image>().color = GameCoordinator.instance.GetTeamColor(team);
                    float time = Mathf.Sin(Time.time * 3f) * 0.5f + 0.5f;
                    WorldToScreenPoint(respawnPos + Vector3.up * time * 0.5f, RespawnGuideRect);
                }
            }
        }

        public void SetIsBallGoalGuide(bool showGuide, bool isGoalGuide, ITeamObject.Teams team)
        {
            if (GoalGuideRect != null)
            {
                Vector3 goalGuidePos = Vector3.zero;
                GoalGuideRect.gameObject.SetActive(showGuide);
                GoalGuideRect.rotation = Quaternion.Euler(0, 0, Time.deltaTime * 90);
                GoalGuideRect.gameObject.GetComponent<Image>().color = GameCoordinator.instance.GetTeamColor(team);
                if (isGoalGuide)
                {
                    ITeamObject.Teams opposingTeam = (team == ITeamObject.Teams.Alpha) ? ITeamObject.Teams.Bravo : ITeamObject.Teams.Alpha;
                    Vector3 pos = GameCoordinator.instance.GetGoalForTeam(opposingTeam).transform.position;
                    goalGuidePos = Camera.main.WorldToScreenPoint(pos);
                }
                else
                {
                    goalGuidePos = Camera.main.WorldToScreenPoint(GunBall.ballPos);
                }
                if (goalGuidePos.z < 0 || goalGuidePos.x < 0 || goalGuidePos.x > Screen.width || goalGuidePos.y < 0 || goalGuidePos.y > Screen.height)
                {
                    bool behind = false;
                    if (goalGuidePos.z < 0)
                    {
                        goalGuidePos.y = -1024;
                        behind = true;
                    }
                    Vector3 indicatorPos = new Vector3();
                    indicatorPos.x = Mathf.Clamp(goalGuidePos.x, 0, Screen.width);
                    indicatorPos.y = Mathf.Clamp(goalGuidePos.y, 0, Screen.height);
                    indicatorPos.z = 0;
                    if (behind)
                    {
                        indicatorPos.x = -1*indicatorPos.x + Screen.width;
                    }
                    GoalGuideRect.position = indicatorPos;
                }
                else
                {
                    GoalGuideRect.position = goalGuidePos;
                }
            }
        }

        Vector3 CastGuide(out Vector3 PredictionPos, out int lastPt, out bool onHit, out bool effectiveHit, Vector3[] CastPoints)
        {
            onHit = false;
            effectiveHit = false;
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
                Physics.SphereCastNonAlloc(startPos, Wpn.GetGuideRadius(), dir, hitsBuffer, dist, Wpn.GetGuideCollisionMask(), QueryTriggerInteraction.Ignore);
                // sort by distance
                System.Array.Sort(hitsBuffer, (a, b) => a.distance.CompareTo(b.distance));
                // iterate through hits and check if they are in our ignore list
                for (int j = 0; j < hitsBuffer.Length; j++)
                {
                    if (hitsBuffer[j].collider == null) continue;
                    //first check if is none type of IShootableObject
                    IShootableObject shootable = hitsBuffer[j].collider.gameObject.GetComponent<IShootableObject>();
                    if (shootable != null)
                    {
                        if (shootable.Type == IShootableObject.ShootableType.None) continue;
                    }

                    bool ignore = false;
                    if (hitsBuffer[j].collider.gameObject == Wpn.Owner.gameObject) continue;
                    if (ignore) continue;
                    // we hit something, return the position
                    onHit = true;
                    ITeamObject teamObject = shootable as ITeamObject;
                    if (teamObject == null && shootable != null)
                    {
                        effectiveHit = true;
                    }
                    else
                    {
                        if (teamObject != null && Wpn.Owner.ObjectTeam != teamObject.ObjectTeam)
                        {
                            effectiveHit = true;
                        }
                    }
                    lastPt = i;
                    return hitsBuffer[j].point;
                }
            }
            return PredictionPos;
        }

        void DrawShotGuide(Vector3[] points)
        {
            WorldToScreenPoint(CastGuide(out PredictionPos, out int LastPoint, out bool on, out bool effective, points), CollidingGuideRect);
            WorldToScreenPoint(PredictionPos, PredictingGuideRect);
            GuideTarget.position = PredictionPos;
            CollidingGuideOnRect.gameObject.SetActive(on);
            CollidingGuideEffectiveRect.gameObject.SetActive(on && effective);
            DrawDebugGuidePath(points);
        }

        void DrawTrajectoryGuide(Vector3[] points)
        {
            if (Wpn.GetGuideWidth() <= 0) return;
            List<Vector3> trajectoryPoints = new List<Vector3>();
            Vector3 endPos = CastGuide(out PredictionPos, out int LastPoint, out bool on, out bool effective, points);
            trajectoryRenderer.startWidth = Wpn.GetGuideWidth();
            trajectoryRenderer.endWidth = Wpn.GetGuideWidth();
            //add points in reverse
            trajectoryPoints.Add(endPos);
            trajectoryPoints.Add(Vector3.MoveTowards(endPos, points[LastPoint], 0.01f));
            for (int i = LastPoint; i > 0; i--)
            {
                trajectoryPoints.Add(points[i]);
                trajectoryPoints.Add(Vector3.MoveTowards(points[i], points[i - 1], 0.01f));
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
}
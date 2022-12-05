using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gunball.Interface;

namespace Gunball.MapObject
{
    public class GoalArea : MonoBehaviour, ITeamObject
    {
        bool goal;
        public ITeamObject.Teams GoalTeam;
        public Animator Spinner;

        public ITeamObject.Teams ObjectTeam { get => GoalTeam; }

        private void OnTriggerEnter(Collider other)
        {
            LayerMask ballLayer = LayerMask.GetMask("Ball");
            if (other.CompareTag("VsBall") && !goal)
            {
                Debug.Log("Goal Area Triggered");
                GunBall ball = other.GetComponent<GunBall>();
                if (ball.Owner != null) return;
                ball.DoDeath();
                goal = true;
                if (GameCoordinator.instance.IsHost)
                {
                    ScoringSystem.instance.AddScore(1, ObjectTeam);
                }
                Invoke(nameof(ResetGoal), 1);
                Spinner.Play("victorySpin");
            }
        }

        void ResetGoal()
        {
            goal = false;
        }

        public void SetTeam(ITeamObject.Teams team)
        {
            return;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gunball.Interface;

namespace Gunball.MapObject
{
    public class GoalArea : MonoBehaviour
    {
        bool goal;
        public Side ThisSide;
        public Animator Spinner;

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log("Goal Area Triggered");
            LayerMask ballLayer = LayerMask.GetMask("Ball");
            if (other.CompareTag("VsBall") && !goal)
            {
                GunBall ball = other.GetComponent<GunBall>();
                if (ball.Owner != null) return;
                ball.DoDeath();
                goal = true;
                FindObjectOfType<ScoringSystem>().SetScore(1, ThisSide);
                Invoke(nameof(ResetGoal), 1);
                Spinner.SetTrigger("spin");
            }
        }

        void ResetGoal()
        {
            goal = false;
        }
    }
}
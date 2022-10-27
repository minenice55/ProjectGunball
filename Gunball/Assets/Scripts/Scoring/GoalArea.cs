using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalArea : MonoBehaviour
{
    bool goal;
    public Side ThisSide;
    public Animator Spinner;

    private void OnTriggerEnter(Collider other)
    {

        if(other.CompareTag("VsBall") && !goal)
        {
            goal = true;
            FindObjectOfType<ScoringSystem>().SetScore(1, ThisSide);
            Invoke(nameof(resetGoal),1);
            Spinner.SetTrigger("spin");
        }
    }

    void resetGoal()
    {
        goal = false;
    }
}
